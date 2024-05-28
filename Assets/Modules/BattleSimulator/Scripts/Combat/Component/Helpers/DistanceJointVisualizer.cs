using UnityEngine;

namespace Combat.Component.Helpers
{
    [RequireComponent(typeof(LineRenderer))]
    public class DistanceJointVisualizer : MonoBehaviour
    {
        [SerializeField] private int _segments = 16;
        [SerializeField] private Color _color = Color.white;

        private LineRenderer _lineRenderer;
        private DistanceJoint2D _joint;
        private Transform _target;
        private Vector3[] _chainPoints;
        private float _maxLength;
        private float _length;

        public Color Color { get => _color; set => _color = value; }

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        public void Initialize(DistanceJoint2D joint)
        {
            _joint = joint;
            _target = joint.connectedBody.transform;
            _lineRenderer.positionCount = _segments;
            _lineRenderer.enabled = true;
            _lineRenderer.startColor = _color;
            _lineRenderer.endColor = _color;

            _maxLength = _joint.distance;
            _length = 0;
            _chainPoints = new Vector3[_segments];
            _lineRenderer.SetPositions(_chainPoints);
        }

        private void Update()
        {
            if (_joint == null || _target == null)
            {
                _lineRenderer.enabled = false;
                return;
            }

            UpdateChainPositions();
            _lineRenderer.SetPositions(_chainPoints);
        }

        void UpdateChainPositions()
        {
            var targetPosition = GetTargetPosition();
            
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

            //// Update the start and end points of the chain
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

        //private void UpdateLine()
        //{
        //    var targetPosition = GetTargetPosition();
        //    var sqrMaxDistance = _length*_length;
        //    var sqrDistance = targetPosition.sqrMagnitude;
        //    var maxOffset = 0f;

        //    if (sqrDistance < sqrMaxDistance)
        //        maxOffset = Mathf.Sqrt(sqrMaxDistance - sqrDistance)*0.5f;

        //    for (int i = 0; i < _segments; ++i)
        //    {
        //        var center = targetPosition * ((float)i / (_segments - 1));
        //        var allowance = maxOffset*2f*Mathf.Min(i, _segments-i-1) / _segments;

        //        var position = _positions[i] + _velocities[i] * Time.deltaTime;
        //        var distance = Vector2.Distance(position, center);
        //        if (distance > allowance)
        //        {
        //            var direction = (position - center).normalized;
        //            position = center + direction*allowance;
        //            _velocities[i] = (position - _positions[i]) / Time.deltaTime;
        //        }

        //        _positions[i] = position;
        //    }

        //    _lineRenderer.SetPositions(_positions);
        //}

        private Vector3 GetTargetPosition() => transform.InverseTransformPoint(_target.localPosition);
    }
}
