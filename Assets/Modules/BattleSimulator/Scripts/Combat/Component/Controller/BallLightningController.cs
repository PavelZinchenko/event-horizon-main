using System;
using Combat.Collision;
using Combat.Collision.Manager;
using Combat.Component.Body;
using Combat.Component.Bullet;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Effects;
using Combat.Factory;
using Combat.Helpers;
using Combat.Scene;
using Combat.Unit;
using GameDatabase.Enums;
using UnityEngine;

namespace Combat.Component.Controller
{
    /// <summary>
    /// Controller for the Starship Earth macro-electron.  The projectile travels
    /// normally until it reaches an enemy or its range limit, then becomes a
    /// stationary ball-lightning source for one second before releasing the
    /// colour-scaled radial discharge.
    /// </summary>
    public sealed class BallLightningController : IController
    {
        public BallLightningController(Combat.Component.Bullet.Bullet bullet, IScene scene, EffectFactory effectFactory,
            IShip owner, float range)
        {
            _bullet = bullet;
            _scene = scene;
            _effectFactory = effectFactory;
            _owner = owner;
            _range = Mathf.Max(1f, range);
            // The database projectile uses the lightning bolt prefab as its
            // collision shell.  Its line renderer is intentionally hidden;
            // the macro-electron is rendered as a translucent spherical orb
            // below and therefore remains visible while it is travelling or
            // waiting to discharge.
            HideProjectileView();
            CreateOrbVisual();
        }

        public bool IsArmed => _armed;
        public bool IsActive => _bullet.IsActive();
        public int DisplayTier => Mathf.Clamp(_chargedVisualTier, 0, TierNames.Length - 1);
        public string DisplayTextureName => _armed ? TierNames[DisplayTier] : "MacroAtom";
        public Color DisplayColor => _armed ? TierColors[DisplayTier] : Color.white;

        public void ReceiveDamage(float damage)
        {
            // Once the discharge has completed the macro-electron is fully
            // energized. Late collision callbacks must not add more charge
            // or change its colour.
            if (_discharged)
                return;

            if (damage > 0f)
                _receivedDamage += damage;
            Arm();
            if (_armed)
                UpdateChargedVisual(Mathf.Clamp(Mathf.FloorToInt(_receivedDamage / 200f), 0, 6), _discharged);
        }

        public void Arm()
        {
            if (_armed || !_bullet.IsActive())
                return;

            _armed = true;
            _armTimer = 1f;
            _tickTimer = 0f;
            _origin = _bullet.Body.WorldPosition();
            _bullet.Body.ApplyAcceleration(-_bullet.Body.Velocity);
            UpdateChargedVisual(0, false);
        }

        public void UpdatePhysics(float elapsedTime)
        {
            if (!_bullet.IsActive())
                return;

            HideProjectileView();
            if (!_lockRequested && _owner.Type.Side == UnitSide.Player)
            {
                _scene.LockUnit(_bullet);
                _lockRequested = true;
            }

            UpdateOrbVisual(elapsedTime);

            if (!_initialized)
            {
                _origin = _bullet.Body.WorldPosition();
                _initialized = true;
            }

            if (!_armed)
            {
                if (Vector2.Distance(_origin, _bullet.Body.WorldPosition()) >= _range ||
                    HasReachedTarget())
                    Arm();
                return;
            }

            _armTimer -= elapsedTime;
            if (_armTimer > 0f)
                return;

            if (_bullet.Collider != null)
                _bullet.Collider.Enabled = false;

            _tickTimer -= elapsedTime;
            if (!_discharged)
            {
                UpdateChargedVisual(Mathf.Clamp(Mathf.FloorToInt(_receivedDamage / 200f), 0, 6), true);
                TransferPlayerLockToNearestEnemy();
                Discharge();
                _discharged = true;
                _tickTimer = 0.5f;
            }
            else if (_tickTimer <= 0f)
            {
                Discharge();
                _tickTimer = 0.5f;
            }

            _duration -= elapsedTime;
            if (_duration <= 0f)
                _bullet.Detonate();
        }

        private void CreateOrbVisual()
        {
            _macroAtomSprite = LoadSprite("Textures/BallLightning/MacroAtom");
            _orb = _macroAtomSprite != null
                ? _effectFactory.CreateEffect("OrbAdditive", _macroAtomSprite, _bullet.Body)
                : _effectFactory.CreateEffect("OrbAdditive", _bullet.Body);
            _orb.Position = Vector2.zero;
            _orb.Size = Mathf.Clamp(_bullet.Body.WorldScale() * 1.25f, 0.85f, 2.2f);
            _orb.Color = new Color(1f, 1f, 1f, 0.84f);
            _orb.Run(999999f, Vector2.zero, 0f);

            _orbGlow = _macroAtomSprite != null
                ? _effectFactory.CreateEffect("OrbAdditive", _macroAtomSprite, _bullet.Body)
                : _effectFactory.CreateEffect("OrbAdditive", _bullet.Body);
            _orbGlow.Position = Vector2.zero;
            _orbGlow.Size = _orb.Size * 1.65f;
            _orbGlow.Color = new Color(0.45f, 0.85f, 1f, 0.18f);
            _orbGlow.Run(999999f, Vector2.zero, 0f);
        }

        private void UpdateChargedVisual(int tier, bool bright)
        {
            if (!_armed)
                return;
            tier = Mathf.Clamp(tier, 0, 6);
            if (_chargedVisualTier == tier && _chargedVisualBright == bright)
                return;

            _chargedVisualTier = tier;
            _chargedVisualBright = bright;
            var textureName = TierNames[tier];
            var texture = Resources.Load<Texture2D>($"Textures/BallLightning/{textureName}");
            if (texture == null)
                return;

            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), 512f);
            var size = _orb != null ? _orb.Size : Mathf.Clamp(_bullet.Body.WorldScale() * 1.25f, 0.85f, 2.2f);
            _orb?.Dispose();
            _orbGlow?.Dispose();
            if (_chargedSprite != null)
                UnityEngine.Object.Destroy(_chargedSprite);
            _orb = _effectFactory.CreateEffect("OrbAdditive", sprite, _bullet.Body);
            _orb.Position = Vector2.zero;
            _orb.Size = size;
            _orb.Color = new Color(1f, 1f, 1f, bright ? 0.95f : 0.38f);
            _orb.Run(999999f, Vector2.zero, 0f);

            _orbGlow = _effectFactory.CreateEffect("OrbAdditive", sprite, _bullet.Body);
            _orbGlow.Position = Vector2.zero;
            _orbGlow.Size = size * (bright ? 1.65f : 1.25f);
            _orbGlow.Color = new Color(1f, 1f, 1f, bright ? 0.34f : 0.08f);
            _orbGlow.Run(999999f, Vector2.zero, 0f);
            _chargedSprite = sprite;
        }

        private static Sprite LoadSprite(string path)
        {
            var texture = Resources.Load<Texture2D>(path);
            return texture == null
                ? null
                : Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), 512f);
        }

        private void UpdateOrbVisual(float elapsedTime)
        {
            _orbPulse += elapsedTime;
            if (_orb == null || !_orb.IsAlive)
                return;

            var pulse = 1f + 0.13f * Mathf.Sin(_orbPulse * 8f);
            var baseSize = Mathf.Clamp(_bullet.Body.WorldScale() * 1.25f, 0.85f, 2.2f);
            _orb.Size = baseSize * pulse;
            if (_orbGlow != null && _orbGlow.IsAlive)
                _orbGlow.Size = baseSize * (1.65f + 0.18f * Mathf.Sin(_orbPulse * 5f));

            if (_armed && _chargedVisualBright)
            {
                _orb.Color = new Color(1f, 1f, 1f, 0.95f);
                if (_orbGlow != null && _orbGlow.IsAlive)
                    _orbGlow.Color = new Color(1f, 1f, 1f, 0.34f);
            }
        }

        private bool HasReachedTarget()
        {
            var origin = _bullet.Body.WorldPosition();
            var hitRadius = Mathf.Max(0.75f, _bullet.Body.WorldScale() * 0.8f);
            lock (_scene.Units.LockObject)
            {
                foreach (var unit in _scene.Units.Items)
                {
                    if (unit == null || unit == _bullet || unit == _owner ||
                        unit.Type.Owner == _owner || !unit.IsActive())
                        continue;
                    if (unit.Type.Class == UnitClass.BackgroundObject || unit.Type.Class == UnitClass.Loot)
                        continue;

                    // Only an enemy ship or another macro-electron ends the
                    // flight. Ignore the emitter's other units and transient
                    // projectiles so the shot cannot arm at its launch point.
                    if (unit is IShip ship)
                    {
                        if (!CombatRelations.AreEnemies(_owner.Type, ship.Type))
                            continue;
                    }
                    else if (!IsBallLightning(unit))
                        continue;

                    if (Vector2.Distance(origin, unit.Body.WorldPosition()) <= hitRadius + unit.Body.WorldScale() * 0.5f)
                        return true;
                }
            }
            return false;
        }

        private void Discharge()
        {
            var tier = Mathf.Clamp(Mathf.FloorToInt(_receivedDamage / 200f), 0, 6);
            // The residual discharge is intentionally short-lived.  Raise
            // each pulse to preserve its threat during the smaller window.
            var damage = 80f * Mathf.Pow(1.7f, tier);
            var color = TierColors[tier];

            if (!_durationInitialized)
            {
                _duration = 8f * Mathf.Pow(1.2f, tier);
                _durationInitialized = true;
            }

            var sourcePosition = _bullet.Body.WorldPosition();
            var units = _scene.Units.Items;
            for (var i = 0; i < units.Count; ++i)
            {
                var target = units[i];
                if (target == null || !target.IsActive() || target == _bullet || target == _owner)
                    continue;
                var targetIsBallLightning = IsBallLightning(target);
                // Ball lightning is a valid target even for ships on the same
                // side. This lets the emitter and allied ball lightning
                // charge one another without making ordinary friendly fire
                // possible for other units.
                if (!targetIsBallLightning && !CombatRelations.AreEnemies(_owner.Type, target.Type))
                    continue;

                var targetPosition = target.Body.WorldPosition();
                var distance = Vector2.Distance(sourcePosition, targetPosition);
                if (distance > 30f)
                    continue;

                if (target is IShip targetShip)
                {
                    targetShip.Affect(new Impact { EnergyDamage = damage }, _owner);
                    targetShip.Body.ApplyAcceleration(-targetShip.Body.Velocity * 0.7f);
                }
                else if (targetIsBallLightning)
                {
                    // An activated macro-electron can excite an opposing
                    // unarmed macro-electron.  Deliver the hit through the
                    // target bullet's damage handler so it follows the same
                    // charging path as an ordinary weapon impact.
                    var collision = CollisionData.FromObjects(_bullet, target, sourcePosition, true,
                        Mathf.Max(Time.fixedDeltaTime, 0.02f));
                    target.OnCollision(new Impact { EnergyDamage = damage }, _bullet, collision);
                }
                else
                    continue;

                var lightning = _effectFactory.CreateEffect("Lightning", target.Body);
                if (lightning == null || !lightning.IsAlive)
                    continue;
                lightning.Position = Vector2.zero;
                lightning.Rotation = RotationHelpers.Angle(sourcePosition - targetPosition);
                lightning.Size = Mathf.Max(1f, distance);
                lightning.Color = color;
                lightning.Run(0.48f, Vector2.zero, 0f);
                CreateDischargeArc(sourcePosition, targetPosition, color);
            }
        }

        private static bool IsBallLightning(IUnit unit)
        {
            return unit is Combat.Component.Bullet.Bullet bullet &&
                   bullet.Controller is BallLightningController;
        }

        private void TransferPlayerLockToNearestEnemy()
        {
            if (_owner.Type.Side != UnitSide.Player || _scene.LockedTarget != _bullet)
                return;

            IShip nearest = null;
            var nearestDistance = float.MaxValue;
            lock (_scene.Ships.LockObject)
            {
                foreach (var ship in _scene.Ships.Items)
                {
                    if (!ship.IsActive() || !CombatRelations.AreEnemies(_owner.Type, ship.Type))
                        continue;

                    var distance = Vector2.SqrMagnitude(ship.Body.WorldPosition() - _bullet.Body.WorldPosition());
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = ship;
                    }
                }
            }

            _scene.LockTarget(nearest);
        }

        private void HideProjectileView()
        {
            // The rocket prefab still carries a ship-shaped sprite. Keep its
            // collision body for targeting, but never expose that fallback
            // sprite behind the macro-electron orb.
            _bullet.View.Size = 0f;
            _bullet.View.Color = Color.clear;
        }

        private void CreateDischargeArc(Vector2 source, Vector2 target, Color color)
        {
            var root = new GameObject("BallLightningArc");
            var line = root.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 8;
            line.widthMultiplier = Mathf.Clamp(0.08f + _bullet.Body.WorldScale() * 0.04f, 0.08f, 0.22f);
            line.numCornerVertices = 2;
            line.numCapVertices = 3;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.sortingOrder = 80;

            var direction = target - source;
            var length = direction.magnitude;
            var perpendicular = length > 0.001f ? new Vector2(-direction.y, direction.x).normalized : Vector2.up;
            for (var i = 0; i < line.positionCount; ++i)
            {
                var t = i / (float)(line.positionCount - 1);
                var jitter = i == 0 || i == line.positionCount - 1
                    ? 0f
                    : UnityEngine.Random.Range(-0.22f, 0.22f) * Mathf.Max(1f, length * 0.08f);
                line.SetPosition(i, Vector2.Lerp(source, target, t) + perpendicular * jitter);
            }

            var start = new GradientColorKey(color, 0f);
            var end = new GradientColorKey(new Color(color.r, color.g, color.b, 0.15f), 1f);
            line.colorGradient = new Gradient
            {
                colorKeys = new[] { start, end },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(Mathf.Clamp01(color.a), 0f),
                    new GradientAlphaKey(0.05f, 1f),
                }
            };
            var fade = root.AddComponent<BallLightningArcFade>();
            fade.Initialize(line, 0.52f);
        }

        public void Dispose()
        {
            _orb?.Dispose();
            _orbGlow?.Dispose();
            if (_macroAtomSprite != null)
                UnityEngine.Object.Destroy(_macroAtomSprite);
            if (_chargedSprite != null && _chargedSprite != _macroAtomSprite)
                UnityEngine.Object.Destroy(_chargedSprite);
            _orb = null;
            _orbGlow = null;
            _macroAtomSprite = null;
            _chargedSprite = null;
        }

        private sealed class BallLightningArcFade : MonoBehaviour
        {
            public void Initialize(LineRenderer line, float lifetime)
            {
                _line = line;
                _lifetime = lifetime;
            }

            private void Update()
            {
                _elapsed += Time.deltaTime;
                var alpha = 1f - Mathf.Clamp01(_elapsed / Mathf.Max(0.01f, _lifetime));
                if (_line != null)
                {
                    var gradient = _line.colorGradient;
                    var keys = gradient.colorKeys;
                    var alphaKeys = gradient.alphaKeys;
                    for (var i = 0; i < alphaKeys.Length; ++i)
                        alphaKeys[i].alpha *= alpha;
                    gradient.alphaKeys = alphaKeys;
                    _line.colorGradient = gradient;
                }

                if (_elapsed >= _lifetime)
                {
                    if (_line != null && _line.material != null)
                        UnityEngine.Object.Destroy(_line.material);
                    UnityEngine.Object.Destroy(gameObject);
                }
            }

            private LineRenderer _line;
            private float _lifetime;
            private float _elapsed;
        }

        private static readonly Color[] TierColors =
        {
            new Color(1f, 0.08f, 0.08f, 0.95f),
            new Color(1f, 0.46f, 0.05f, 0.95f),
            new Color(1f, 0.9f, 0.05f, 0.95f),
            new Color(0.15f, 1f, 0.25f, 0.95f),
            new Color(0.15f, 0.55f, 1f, 0.95f),
            new Color(0.25f, 0.2f, 1f, 0.95f),
            new Color(0.75f, 0.25f, 1f, 0.95f),
        };

        private static readonly string[] TierNames =
        {
            "BallLightningRed",
            "BallLightningOrange",
            "BallLightningYellow",
            "BallLightningGreen",
            "BallLightningBlue",
            "BallLightningIndigo",
            "BallLightningPurple",
        };

        private readonly Combat.Component.Bullet.Bullet _bullet;
        private readonly IScene _scene;
        private readonly EffectFactory _effectFactory;
        private readonly IShip _owner;
        private readonly float _range;
        private Vector2 _origin;
        private float _receivedDamage;
        private float _armTimer;
        private float _tickTimer;
        private float _duration;
        private bool _initialized;
        private bool _armed;
        private bool _discharged;
        private bool _durationInitialized;
        private IEffect _orb;
        private IEffect _orbGlow;
        private float _orbPulse;
        private bool _lockRequested;
        private int _chargedVisualTier = -1;
        private bool _chargedVisualBright;
        private Sprite _macroAtomSprite;
        private Sprite _chargedSprite;
    }
}
