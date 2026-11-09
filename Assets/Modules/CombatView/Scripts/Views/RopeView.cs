using UnityEngine;

namespace CombatView.Views
{
    // Copied from legacy Combat.Component.Helpers.DistanceJointVisualizer (migration phase 5b, step 5): the harpoon
    // rope from the hook to its ship. Changes: the renderer passes the ship's drawn position each frame and the rope
    // length at the start (legacy read the joint and its connected body in Update).
    [RequireComponent(typeof(LineRenderer))]
    public class RopeView : MonoBehaviour
    {
        [SerializeField] private int _segments = 16;
        [SerializeField] private Color _color = Color.white;

        private LineRenderer _lineRenderer;
        private Vector3[] _chainPoints;
        private float _maxLength;
        private float _length;

        public Color Color { get => _color; set => _color = value; }

        // A new hook. maxLength is the legacy joint distance: the weapon range.
        public void Initialize(float maxLength)
        {
            if (!_lineRenderer) _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.positionCount = _segments;
            _lineRenderer.enabled = true;
            _lineRenderer.startColor = _color;
            _lineRenderer.endColor = _color;

            _maxLength = maxLength;
            _length = 0;
            _chainPoints = new Vector3[_segments];
            _lineRenderer.SetPositions(_chainPoints);
        }

        // Null hides the rope: the ship has no view.
        public void UpdateRope(Vector2? target)
        {
            if (!_lineRenderer || _chainPoints == null)
                return;

            _lineRenderer.enabled = target.HasValue;
            if (!target.HasValue)
                return;

            UpdateChainPositions(transform.InverseTransformPoint(target.Value));
            _lineRenderer.SetPositions(_chainPoints);
        }

        private void UpdateChainPositions(Vector3 targetPosition)
        {
            var length = targetPosition.sqrMagnitude;
            if (length > _length*_length)
                _length = Mathf.Sqrt(length);

            if (_length < _maxLength*0.5f)
            {
                for (int i = 0; i < _segments; i++)
                    _chainPoints[i] = targetPosition * i / (_segments - 1);
                return;
            }

            var segmentLength = _length / (_segments - 1);

            _chainPoints[0] = Vector3.zero;
            _chainPoints[_segments - 1] = targetPosition;

            // Forward pass
            for (int i = 1; i < _segments-1; i++)
            {
                var direction = (_chainPoints[i] - _chainPoints[i - 1]).normalized;
                _chainPoints[i] = _chainPoints[i - 1] + direction * segmentLength;
            }

            // Backward pass
            for (int i = _segments - 2; i > 0; i--)
            {
                var direction = (_chainPoints[i] - _chainPoints[i + 1]).normalized;
                _chainPoints[i] = _chainPoints[i + 1] + direction * segmentLength;
            }
        }
    }
}
