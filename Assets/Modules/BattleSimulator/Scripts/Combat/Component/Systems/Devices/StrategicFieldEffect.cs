using System.Collections.Generic;
using System.Linq;
using Combat.Collision;
using Combat.Component.Body;
using Combat.Component.Ship;
using Combat.Component.Stats;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;
using UnityEngine;

namespace Combat.Component.Systems.Devices
{
    public sealed class StrategicFieldEffect : MonoBehaviour
    {
        public enum FieldKind { DualVectorFoil, BlackHole, DarkDomain }

        public static StrategicFieldEffect Create(IScene scene, IShip owner, Vector2 position, FieldKind kind)
        {
            var gameObject = new GameObject("StrategicField_" + kind);
            gameObject.transform.position = position;
            var effect = gameObject.AddComponent<StrategicFieldEffect>();
            effect._scene = scene;
            effect._owner = owner;
            effect._kind = kind;
            effect._position = position;
            effect._radius = kind switch
            {
                FieldKind.DarkDomain => 10f,
                FieldKind.BlackHole => 10f,
                _ => 1f
            };
            if (kind == FieldKind.DualVectorFoil)
                effect._foilRadii = Enumerable.Repeat(1f, 64).ToArray();
            effect._lifetime = kind == FieldKind.BlackHole ? 5f : float.PositiveInfinity;
            effect.CreateVisual();
            ActiveFields.Add(effect);
            return effect;
        }

        private void FixedUpdate()
        {
            var elapsed = Time.fixedDeltaTime;
            _age += elapsed;
            if (_age >= _lifetime)
            {
                Destroy(gameObject);
                return;
            }

            if (_kind == FieldKind.DualVectorFoil)
            {
                for (var i = 0; i < _foilRadii.Length; ++i)
                {
                    var angle = i * Mathf.PI * 2f / _foilRadii.Length;
                    var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    var nextRadius = _foilRadii[i] + 150f * elapsed;
                    if (!IsPointBlockedByDarkDomain(_position + direction * nextRadius))
                        _foilRadii[i] = nextRadius;
                }
                _radius = _foilRadii.Max();
            }

            if (_kind == FieldKind.BlackHole)
            {
                if (_blackHoleVisual != null)
                {
                    _blackHoleVisual.Rotate(0f, 0f, 38f * elapsed, Space.Self);
                    var pulse = 1f + Mathf.Sin(_age * 7f) * 0.035f;
                    _blackHoleVisual.localScale = _blackHoleBaseScale * pulse;
                }
                foreach (var field in ActiveFields.ToArray())
                {
                    if (field == null || field == this || field._kind != FieldKind.DualVectorFoil) continue;
                    if (Vector2.Distance(_position, field._position) <= _radius + field._radius)
                        Destroy(field.gameObject);
                }
            }

            lock (_scene.Units.LockObject)
            {
                foreach (var unit in _scene.Units.Items.ToArray())
                {
                    if (unit == null || !unit.IsActive()) continue;
                    var delta = _position - unit.Body.WorldPosition();
                    if (delta.sqrMagnitude > _radius * _radius) continue;

                    if (_kind == FieldKind.BlackHole)
                    {
                        if (unit.Type.Class != UnitClass.Ship && unit.Type.Class != UnitClass.Drone)
                        {
                            unit.Vanish();
                            continue;
                        }
                        var distance = Mathf.Max(0.35f, delta.magnitude);
                        var direction = delta / distance;
                        // A steep inverse-square pull with strong edge force makes
                        // the field effective against heavy ships as well as small
                        // units, while velocity damping prevents orbiting escape.
                        var normalizedDistance = Mathf.Clamp01(distance / _radius);
                        var pullStrength = Mathf.Lerp(12000f, 2200f, normalizedDistance);
                        var pull = direction * pullStrength * elapsed;
                        var damping = -unit.Body.Velocity * (2.5f * elapsed);
                        unit.Body.ApplyAcceleration(pull + damping);
                        if (unit is IShip blackHoleTarget)
                            blackHoleTarget.Affect(new Impact { TrueDamage = 500f * elapsed }, _owner);
                    }
                    else if (_kind == FieldKind.DualVectorFoil)
                    {
                        if (unit is IShip foilTarget)
                        {
                            if (_owner != null && !CombatRelations.AreEnemies(_owner.Type, foilTarget.Type)) continue;
                            if (foilTarget.Systems.All.OfType<LowDimensionalProjectionDevice>().Any()) continue;
                            var angle = Mathf.Atan2(-delta.y, -delta.x);
                            if (angle < 0f) angle += Mathf.PI * 2f;
                            var segment = Mathf.FloorToInt(angle / (Mathf.PI * 2f) * _foilRadii.Length) % _foilRadii.Length;
                            if (delta.magnitude > _foilRadii[segment]) continue;
                            foilTarget.Affect(new Impact { TrueDamage = 2100000000f * elapsed }, _owner);
                            if (!foilTarget.IsActive()) CreateMosaicRemnant(foilTarget.Body.Position, foilTarget.Body.Scale);
                        }
                    }
                    else if (_kind == FieldKind.DarkDomain)
                    {
                        if (ShipStats.IsFourDimensionalUnit(unit)) continue;
                        if (unit.Type.Class == UnitClass.Missile || unit.Type.Class == UnitClass.EnergyBolt)
                        {
                            unit.Vanish();
                            continue;
                        }
                        if (unit is IShip ship && !ship.Systems.All.OfType<WarpDrive>().Any(drive => drive.IsWarping))
                        {
                            var max = Mathf.Max(0.5f, ship.Engine.MaxVelocity * 0.1f);
                            if (unit.Body.Velocity.sqrMagnitude > max * max)
                                unit.Body.ApplyAcceleration(unit.Body.Velocity.normalized * max - unit.Body.Velocity);
                        }
                    }
                }
            }
            UpdateVisual();
        }

        private void CreateVisual()
        {
            _line = gameObject.AddComponent<LineRenderer>();
            _line.useWorldSpace = true;
            _line.loop = true;
            _line.positionCount = 64;
            _line.widthMultiplier = _kind switch
            {
                FieldKind.DualVectorFoil => 1.3f,
                FieldKind.DarkDomain => 4.5f,
                _ => 0.9f
            };
            _line.material = new Material(Shader.Find("Sprites/Default"));
            _line.sortingOrder = 25;
            _line.startColor = _line.endColor = _kind switch
            {
                FieldKind.DualVectorFoil => new Color(0.7f, 0.45f, 1f, 0.7f),
                FieldKind.BlackHole => new Color(0.35f, 0.1f, 0.8f, 0.9f),
                _ => new Color(0.01f, 0.01f, 0.02f, 0.95f)
            };
            if (_kind == FieldKind.DualVectorFoil)
                CreateFoilMosaicVisual();
            else if (_kind == FieldKind.BlackHole)
                CreateBlackHoleVisual();
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_line == null) return;
            for (var i = 0; i < _line.positionCount; ++i)
            {
                var angle = i * Mathf.PI * 2f / _line.positionCount;
                var radius = _kind == FieldKind.DualVectorFoil && _foilRadii != null ? _foilRadii[i] : _radius;
                _line.SetPosition(i, _position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
            UpdateFoilMosaicVisual();
        }

        private void CreateFoilMosaicVisual()
        {
            _mosaicMesh = new Mesh { name = "DualVectorFoilMosaicMesh" };
            var filter = gameObject.AddComponent<MeshFilter>();
            filter.sharedMesh = _mosaicMesh;
            _mosaicRenderer = gameObject.AddComponent<MeshRenderer>();
            var shader = Resources.Load<Shader>("DualVectorFoilBackgroundMosaic") ??
                         Shader.Find("ThreeBody/DualVectorFoilBackgroundMosaic");
            _mosaicMaterial = new Material(shader);
            _mosaicMaterial.SetFloat("_PixelSize", 24f);
            _mosaicRenderer.sharedMaterial = _mosaicMaterial;
            // Render before every ship/projectile SpriteRenderer. Combined with
            // the shader's early transparent queue, GrabPass captures only the
            // opaque starfield; ships are rendered afterwards and stay crisp.
            _mosaicRenderer.sortingOrder = -100;

            var triangles = new int[_foilRadii.Length * 3];
            for (var i = 0; i < _foilRadii.Length; ++i)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
            _mosaicTriangles = triangles;
        }

        private void UpdateFoilMosaicVisual()
        {
            if (_mosaicMesh == null || _foilRadii == null) return;
            var count = _foilRadii.Length;
            var vertices = new Vector3[count + 2];
            var uv = new Vector2[count + 2];
            vertices[0] = Vector3.zero;
            uv[0] = Vector2.zero;
            for (var i = 0; i <= count; ++i)
            {
                var segment = i % count;
                var angle = segment * Mathf.PI * 2f / count;
                var local = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _foilRadii[segment];
                vertices[i + 1] = local;
                uv[i + 1] = local;
            }
            _mosaicMesh.Clear();
            _mosaicMesh.vertices = vertices;
            _mosaicMesh.uv = uv;
            _mosaicMesh.triangles = _mosaicTriangles;
            _mosaicMesh.RecalculateBounds();
        }

        private void CreateBlackHoleVisual()
        {
            var visual = new GameObject("BlackHoleAccretionDisk");
            visual.transform.SetParent(transform, false);
            _blackHoleVisual = visual.transform;
            _blackHoleBaseScale = new Vector3(_radius * 2.8f, _radius * 1.75f, 1f);
            _blackHoleVisual.localScale = _blackHoleBaseScale;
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = BlackHoleSprite;
            renderer.color = Color.white;
            renderer.sortingOrder = 31;
        }

        private static Sprite BlackHoleSprite
        {
            get
            {
                if (_blackHoleSprite != null) return _blackHoleSprite;
                const int size = 192;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    name = "BlackHoleAccretionDisk",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
                var pixels = new Color32[size * size];
                var center = (size - 1) * 0.5f;
                for (var y = 0; y < size; ++y)
                for (var x = 0; x < size; ++x)
                {
                    var dx = (x - center) / center;
                    var dy = (y - center) / center;
                    var distance = Mathf.Sqrt(dx * dx + dy * dy);
                    var angle = Mathf.Atan2(dy, dx);
                    Color color;
                    if (distance < 0.30f)
                        color = new Color(0f, 0f, 0.008f, 1f);
                    else if (distance < 0.52f)
                    {
                        var ring = Mathf.Clamp01(1f - Mathf.Abs(distance - 0.405f) / 0.115f);
                        var streak = 0.72f + 0.28f * Mathf.Sin(angle * 9f + distance * 38f);
                        color = Color.Lerp(new Color(0.38f, 0.04f, 0.85f, ring),
                            new Color(1f, 0.46f, 0.06f, ring), streak);
                    }
                    else
                    {
                        var glow = Mathf.Clamp01((0.92f - distance) / 0.40f);
                        color = new Color(0.18f, 0.03f, 0.48f, glow * 0.42f);
                    }
                    pixels[y * size + x] = color;
                }
                texture.SetPixels32(pixels);
                texture.Apply(false, true);
                _blackHoleSprite = Sprite.Create(texture, new Rect(0, 0, size, size),
                    new Vector2(0.5f, 0.5f), size);
                return _blackHoleSprite;
            }
        }

        public static bool TryBlockRay(Vector2 origin, Vector2 direction, float maxRange, out float distance)
        {
            distance = maxRange;
            var blocked = false;
            var normalized = direction.normalized;
            foreach (var field in ActiveFields.Where(item => item != null &&
                         (item._kind == FieldKind.BlackHole || item._kind == FieldKind.DarkDomain)))
            {
                var toCenter = field._position - origin;
                var along = Mathf.Clamp(Vector2.Dot(toCenter, normalized), 0f, maxRange);
                var closest = origin + normalized * along;
                if (Vector2.Distance(closest, field._position) > field._radius) continue;
                distance = Mathf.Min(distance, Mathf.Max(0f, along - field._radius));
                blocked = true;
            }
            return blocked;
        }

        public static bool IsBlockedByDarkDomain(Vector2 position, float radius)
        {
            if (WarpTrailEffect.IsInsideAnyTrail(position, radius)) return true;
            return ActiveFields.Any(item => item != null && item._kind == FieldKind.DarkDomain &&
                                            Vector2.Distance(item._position, position) <= item._radius + radius);
        }

        private static bool IsPointBlockedByDarkDomain(Vector2 position)
        {
            if (WarpTrailEffect.IsInsideAnyTrail(position, 0f)) return true;
            return ActiveFields.Any(item => item != null && item._kind == FieldKind.DarkDomain &&
                                            Vector2.Distance(item._position, position) <= item._radius);
        }

        private static void CreateMosaicRemnant(Vector2 position, float scale)
        {
            var root = new GameObject("DimensionalMosaicRemnant");
            root.transform.position = position;
            for (var x = -2; x <= 2; ++x)
            for (var y = -2; y <= 2; ++y)
            {
                if (Random.value < 0.22f) continue;
                var block = new GameObject("MosaicBlock");
                block.transform.SetParent(root.transform, false);
                block.transform.localPosition = new Vector3(x, y, 0f) * Mathf.Max(0.2f, scale * 0.16f);
                block.transform.localScale = Vector3.one * Mathf.Max(0.25f, scale * Random.Range(0.13f, 0.22f));
                var renderer = block.AddComponent<SpriteRenderer>();
                renderer.sprite = MosaicSprite;
                var shade = Random.Range(0.08f, 0.55f);
                renderer.color = new Color(shade, shade, shade, 0.95f);
                renderer.sortingOrder = 40;
            }
        }

        private static Sprite MosaicSprite => _mosaicSprite ??= Sprite.Create(Texture2D.whiteTexture,
            new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        public static void ClearAll()
        {
            foreach (var field in ActiveFields.ToArray())
                if (field != null) Destroy(field.gameObject);
            ActiveFields.Clear();
        }

        private void OnDestroy()
        {
            ActiveFields.Remove(this);
            if (_mosaicMaterial != null) Destroy(_mosaicMaterial);
            if (_mosaicMesh != null) Destroy(_mosaicMesh);
        }

        private static readonly List<StrategicFieldEffect> ActiveFields = new();
        private IScene _scene;
        private IShip _owner;
        private FieldKind _kind;
        private Vector2 _position;
        private float _radius;
        private float _lifetime;
        private float _age;
        private LineRenderer _line;
        private float[] _foilRadii;
        private static Sprite _mosaicSprite;
        private Mesh _mosaicMesh;
        private MeshRenderer _mosaicRenderer;
        private Material _mosaicMaterial;
        private int[] _mosaicTriangles;
        private Transform _blackHoleVisual;
        private Vector3 _blackHoleBaseScale;
        private static Sprite _blackHoleSprite;
    }
}
