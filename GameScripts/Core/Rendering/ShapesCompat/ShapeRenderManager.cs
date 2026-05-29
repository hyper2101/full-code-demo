using UnityEngine;
using System.Collections.Generic;

namespace Shapes
{
    public struct ShapeCameraContext
    {
        public Camera Camera;
        public Matrix4x4 VPMatrix;
        public float Zoom;
    }

    public static class ShapeRenderManager
    {
        private static HashSet<IShapeDataSource> activeShapes = new HashSet<IShapeDataSource>();
        
        private static Mesh sharedQuad;
        private static Mesh sharedDisc;
        private static Material sharedUnlitMaterial;
        
        // Batching data
        private static Matrix4x4[] matrixBatch = new Matrix4x4[1023];
        private static Vector4[] colorBatch = new Vector4[1023];
        private static MaterialPropertyBlock propBlock;

        public static void Initialize()
        {
            activeShapes.Clear();
            
            // Generate Shared Quad Mesh
            sharedQuad = new Mesh { name = "ShapeSharedQuad" };
            sharedQuad.vertices = new Vector3[] {
                new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0)
            };
            sharedQuad.triangles = new int[] { 0, 2, 1, 2, 3, 1 };

            // Generate Shared Disc Mesh (24 sides)
            sharedDisc = new Mesh { name = "ShapeSharedDisc" };
            int segments = 24;
            Vector3[] vertices = new Vector3[segments + 1];
            int[] triangles = new int[segments * 3];
            vertices[0] = Vector3.zero;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle) * 0.5f, Mathf.Sin(angle) * 0.5f, 0);
            }
            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1 == segments) ? 1 : i + 2;
            }
            sharedDisc.vertices = vertices;
            sharedDisc.triangles = triangles;

            Shader unlitShader = Shader.Find("Unlit/Color");
            if (unlitShader == null) unlitShader = Shader.Find("Hidden/Internal-Colored");
            sharedUnlitMaterial = new Material(unlitShader);
            sharedUnlitMaterial.enableInstancing = true;

            propBlock = new MaterialPropertyBlock();
        }

        public static void Register(IShapeDataSource shape)
        {
            if (shape != null) activeShapes.Add(shape);
        }

        public static void Unregister(IShapeDataSource shape)
        {
            if (shape != null) activeShapes.Remove(shape);
        }

        public static void Render(ShapeCameraContext ctx)
        {
            if (activeShapes.Count == 0) return;

            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(ctx.Camera);
            
            // Step 5: Sorting by RenderLayer
            SortedDictionary<int, List<IShapeDataSource>> layeredShapes = new SortedDictionary<int, List<IShapeDataSource>>();

            foreach (var shape in activeShapes)
            {
                if (!shape.Visible) continue;
                if (shape.HasBounds && !GeometryUtility.TestPlanesAABB(frustumPlanes, shape.Bounds)) continue;

                if (!layeredShapes.ContainsKey(shape.RenderLayer))
                    layeredShapes[shape.RenderLayer] = new List<IShapeDataSource>();
                layeredShapes[shape.RenderLayer].Add(shape);
                shape.IsDirty = false;
            }

            foreach (var kvp in layeredShapes)
            {
                DrawLayer(kvp.Value);
            }
        }

        private static void DrawLayer(List<IShapeDataSource> shapes)
        {
            int quadBatchCount = 0;
            int discBatchCount = 0;

            Matrix4x4[] discMatrixBatch = new Matrix4x4[1023];
            Vector4[] discColorBatch = new Vector4[1023];

            foreach (var shape in shapes)
            {
                if (shape is Rectangle rect)
                {
                    matrixBatch[quadBatchCount] = Matrix4x4.TRS(rect.transform.position, rect.transform.rotation, rect.transform.lossyScale);
                    colorBatch[quadBatchCount] = rect.Color;
                    quadBatchCount++;
                    if (quadBatchCount == 1023) { FlushBatch(sharedQuad, matrixBatch, colorBatch, quadBatchCount); quadBatchCount = 0; }
                }
                else if (shape is Line line)
                {
                    Vector3 center = (line.Start + line.End) / 2f;
                    Vector3 dir = line.End - line.Start;
                    float length = dir.magnitude;
                    if (length < 0.0001f) continue;

                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    Quaternion rot = Quaternion.Euler(0, 0, angle);
                    Vector3 scale = new Vector3(length, line.Thickness, 1f);

                    matrixBatch[quadBatchCount] = Matrix4x4.TRS(center, rot, scale);
                    colorBatch[quadBatchCount] = line.Color;
                    quadBatchCount++;
                    if (quadBatchCount == 1023) { FlushBatch(sharedQuad, matrixBatch, colorBatch, quadBatchCount); quadBatchCount = 0; }
                }
                else if (shape is Disc disc)
                {
                    float d = disc.Radius * 2f;
                    discMatrixBatch[discBatchCount] = Matrix4x4.TRS(disc.transform.position, disc.transform.rotation, new Vector3(d, d, 1f));
                    discColorBatch[discBatchCount] = disc.Color;
                    discBatchCount++;
                    if (discBatchCount == 1023) { FlushBatch(sharedDisc, discMatrixBatch, discColorBatch, discBatchCount); discBatchCount = 0; }
                }
            }

            if (quadBatchCount > 0) FlushBatch(sharedQuad, matrixBatch, colorBatch, quadBatchCount);
            if (discBatchCount > 0) FlushBatch(sharedDisc, discMatrixBatch, discColorBatch, discBatchCount);
        }

        private static void FlushBatch(Mesh mesh, Matrix4x4[] mBatch, Vector4[] cBatch, int count)
        {
            propBlock.SetVectorArray("_Color", cBatch);
            Graphics.DrawMeshInstanced(mesh, 0, sharedUnlitMaterial, mBatch, count, propBlock);
        }

        public static void Shutdown()
        {
            activeShapes.Clear();
            if (sharedQuad != null) GameObject.Destroy(sharedQuad);
            if (sharedDisc != null) GameObject.Destroy(sharedDisc);
            if (sharedUnlitMaterial != null) GameObject.Destroy(sharedUnlitMaterial);
        }
    }
}
