using Combat.Component.Collider;
using Combat.Component.View;
using Combat.Services;
using UnityEngine;

namespace Combat.Component.Helpers
{
    public class BulletDependencyInjector : MonoBehaviour, IDependencyInjector
    {
        [SerializeField] private CommonCollider _commonCollider;
        [SerializeField] private CircleCollider _circleCollider;
        [SerializeField] private RayCastCollider _rayCastCollider;
        [SerializeField] private TrailView _trailView;

        public void Initialize(IGameServicesProvider services)
        {
            if (_circleCollider) _circleCollider.Initialize(services.CollisionManager);
            if (_commonCollider) _commonCollider.Initialize(services.CollisionManager);
            if (_rayCastCollider) _rayCastCollider.Initialize(services.CollisionManager);
            if (_trailView) _trailView.Initialize(services.TrailRendererPool);
        }
    }
}
