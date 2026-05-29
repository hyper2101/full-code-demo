using UnityEngine;

namespace Shapes
{
    public interface IShapeDataSource
    {
        bool Visible { get; }
        int RenderLayer { get; }
        bool HasBounds { get; }
        Bounds Bounds { get; }
        bool IsDirty { get; set; }
    }

    public enum ShapeRenderLayer
    {
        Background = 0,
        BoardConnector = 1000,
        CardHighlight = 2000,
        CombatOverlay = 3000,
        UIOverlay = 4000,
        Tooltip = 5000
    }

    public abstract class ShapeRenderer : MonoBehaviour, IShapeDataSource
    {
        [SerializeField] private Color _color = Color.white;
        public Color Color 
        { 
            get => _color; 
            set { if (_color != value) { _color = value; IsDirty = true; } } 
        }

        [SerializeField] private int _sortingLayerID;
        public int SortingLayerID 
        { 
            get => _sortingLayerID; 
            set { if (_sortingLayerID != value) { _sortingLayerID = value; IsDirty = true; } } 
        }

        [SerializeField] private int _renderQueue;
        public int RenderQueue 
        { 
            get => _renderQueue; 
            set { if (_renderQueue != value) { _renderQueue = value; IsDirty = true; } } 
        }

        public bool Visible => isActiveAndEnabled;
        public virtual int RenderLayer => _sortingLayerID + _renderQueue;
        public virtual bool HasBounds => false;
        public virtual Bounds Bounds => default;
        public bool IsDirty { get; set; } = true;

        protected virtual void OnEnable()
        {
            ShapeRenderManager.Register(this);
            IsDirty = true;
        }

        protected virtual void OnDisable()
        {
            ShapeRenderManager.Unregister(this);
        }

        protected virtual void OnDestroy()
        {
            ShapeRenderManager.Unregister(this);
        }
    }

    public class Rectangle : ShapeRenderer
    {
        [SerializeField] private float _dashOffset;
        public float DashOffset 
        { 
            get => _dashOffset; 
            set { if (_dashOffset != value) { _dashOffset = value; IsDirty = true; } } 
        }

        [SerializeField] private float _thickness = 0.1f;
        public float Thickness 
        { 
            get => _thickness; 
            set { if (_thickness != value) { _thickness = value; IsDirty = true; } } 
        }

        [SerializeField] private float _cornerRadius;
        public float CornerRadius 
        { 
            get => _cornerRadius; 
            set { if (_cornerRadius != value) { _cornerRadius = value; IsDirty = true; } } 
        }

        public override bool HasBounds => true;
        public override Bounds Bounds => new Bounds(transform.position, new Vector3(transform.localScale.x, transform.localScale.y, 0));
    }

    public class Line : ShapeRenderer
    {
        [SerializeField] private Vector3 _start;
        public Vector3 Start 
        { 
            get => _start; 
            set { if (_start != value) { _start = value; IsDirty = true; } } 
        }

        [SerializeField] private Vector3 _end;
        public Vector3 End 
        { 
            get => _end; 
            set { if (_end != value) { _end = value; IsDirty = true; } } 
        }

        [SerializeField] private float _thickness = 0.1f;
        public float Thickness 
        { 
            get => _thickness; 
            set { if (_thickness != value) { _thickness = value; IsDirty = true; } } 
        }

        public override bool HasBounds => true;
        public override Bounds Bounds 
        {
            get 
            {
                Vector3 center = (_start + _end) / 2f;
                Vector3 size = new Vector3(Mathf.Abs(_start.x - _end.x) + _thickness, Mathf.Abs(_start.y - _end.y) + _thickness, Mathf.Abs(_start.z - _end.z) + _thickness);
                return new Bounds(center, size);
            }
        }
    }

    public class Disc : ShapeRenderer
    {
        [SerializeField] private float _radius = 0.5f;
        public float Radius 
        { 
            get => _radius; 
            set { if (_radius != value) { _radius = value; IsDirty = true; } } 
        }

        [SerializeField] private float _thickness = 0.1f;
        public float Thickness 
        { 
            get => _thickness; 
            set { if (_thickness != value) { _thickness = value; IsDirty = true; } } 
        }

        public override bool HasBounds => true;
        public override Bounds Bounds => new Bounds(transform.position, new Vector3(_radius * 2, _radius * 2, _radius * 2));
    }

    public class Polyline : ShapeRenderer
    {
        public float Thickness;
        public void AddPoint(Vector3 point) {}
    }
}
