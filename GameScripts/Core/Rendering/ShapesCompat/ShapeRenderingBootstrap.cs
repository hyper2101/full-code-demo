using UnityEngine;

namespace Shapes
{
    [DefaultExecutionOrder(1000)]
    public class ShapeRenderingBootstrap : MonoBehaviour
    {
        private Camera mainCamera;

        private void Awake()
        {
            ShapeRenderManager.Initialize();
        }

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (mainCamera == null) return;
            
            ShapeCameraContext ctx = new ShapeCameraContext
            {
                Camera = mainCamera,
                VPMatrix = mainCamera.projectionMatrix * mainCamera.worldToCameraMatrix,
                Zoom = mainCamera.orthographicSize // Simple zoom representation
            };

            ShapeRenderManager.Render(ctx);
        }

        private void OnDestroy()
        {
            ShapeRenderManager.Shutdown();
        }
    }
}
