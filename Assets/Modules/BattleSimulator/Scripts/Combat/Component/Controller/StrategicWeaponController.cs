using Combat.Collision;
using Combat.Component.Bullet;
using Combat.Component.Ship;
using Combat.Component.Systems.Devices;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;
using UnityEngine;

namespace Combat.Component.Controller
{
    public sealed class StrategicWeaponController : IController
    {
        public enum WeaponKind { Photon, DualVectorFoil, BlackHole, DarkDomain }

        public WeaponKind Kind => _kind;
        public bool IsActive => _bullet != null && _bullet.IsActive();

        public StrategicWeaponController(Combat.Component.Bullet.Bullet bullet, IScene scene, IShip owner, float range, WeaponKind kind)
        {
            _bullet = bullet;
            _scene = scene;
            _owner = owner;
            _range = Mathf.Max(1f, range);
            _kind = kind;
            _start = bullet.Body.WorldPosition();
        }

        public void UpdatePhysics(float elapsedTime)
        {
            if (!_bullet.IsActive()) return;
            _elapsed += elapsedTime;
            var travelled = Vector2.Distance(_start, _bullet.Body.WorldPosition());

            if (_kind == WeaponKind.Photon && travelled >= _range)
            {
                ApplyAreaDamage(_bullet.Body.WorldPosition(), 24f, 8000f, false);
                _bullet.Vanish();
            }
            else if (_kind == WeaponKind.DualVectorFoil)
            {
                var collision = _bullet.Collider.ActiveCollision;
                if (!_foilStopped && (travelled >= _range || collision != null &&
                    (collision.Type.Class == UnitClass.Ship || collision.Type.Class == UnitClass.Drone)))
                    _foilStopped = true;

                if (_foilStopped)
                {
                    _bullet.Body.ApplyAcceleration(-_bullet.Body.Velocity);
                    if (_elapsed >= 15f)
                    {
                        StrategicFieldEffect.Create(_scene, _owner, _bullet.Body.WorldPosition(), StrategicFieldEffect.FieldKind.DualVectorFoil);
                        _bullet.Vanish();
                    }
                }
            }
            else if ((_kind == WeaponKind.BlackHole || _kind == WeaponKind.DarkDomain) &&
                     (travelled >= _range || _bullet.Collider.ActiveCollision != null &&
                     (_bullet.Collider.ActiveCollision.Type.Class == UnitClass.Ship ||
                      _bullet.Collider.ActiveCollision.Type.Class == UnitClass.Drone)))
            {
                var fieldKind = _kind == WeaponKind.BlackHole
                    ? StrategicFieldEffect.FieldKind.BlackHole
                    : StrategicFieldEffect.FieldKind.DarkDomain;
                StrategicFieldEffect.Create(_scene, _owner, _bullet.Body.WorldPosition(), fieldKind);
                _bullet.Vanish();
            }
        }

        private void ApplyAreaDamage(Vector2 center, float radius, float damage, bool dimensional)
        {
            lock (_scene.Units.LockObject)
            {
                foreach (var unit in _scene.Units.Items)
                {
                    if (unit == null || !unit.IsActive() || unit == _bullet) continue;
                    if (Vector2.Distance(center, unit.Body.WorldPosition()) > radius) continue;
                    if (unit is IShip ship)
                    {
                        var impact = dimensional ? new Impact { TrueDamage = damage } : new Impact { KineticDamage = damage };
                        ship.Affect(impact, _owner);
                    }
                }
            }
        }

        public void Dispose() { }

        private readonly Combat.Component.Bullet.Bullet _bullet;
        private readonly IScene _scene;
        private readonly IShip _owner;
        private readonly float _range;
        private readonly WeaponKind _kind;
        private readonly Vector2 _start;
        private float _elapsed;
        private bool _foilStopped;
    }
}
