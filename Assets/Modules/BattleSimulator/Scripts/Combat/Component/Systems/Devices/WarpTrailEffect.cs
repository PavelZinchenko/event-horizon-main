using System.Collections.Generic;
using Combat.Component.Ship;
using Combat.Component.Body;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;
using Combat.Component.Stats;
using UnityEngine;
using System.Linq;

namespace Combat.Component.Systems.Devices
{
    public sealed class WarpTrailEffect : MonoBehaviour
    {
        public static WarpTrailEffect Create(IScene scene, IShip owner)
        {
            var go = new GameObject("Preview5WarpTrail");
            var effect = go.AddComponent<WarpTrailEffect>();
            effect._scene = scene;
            effect._owner = owner;
            effect._line = go.AddComponent<LineRenderer>();
            effect._line.useWorldSpace = true;
            effect._line.widthMultiplier = 7f;
            effect._line.numCapVertices = 12;
            effect._line.numCornerVertices = 12;
            effect._line.material = new Material(Shader.Find("Sprites/Default"));
            effect._line.startColor = new Color(0.002f, 0.003f, 0.006f, 0.96f);
            effect._line.endColor = new Color(0.008f, 0.012f, 0.022f, 0.86f);
            effect._line.sortingOrder = 20;
            effect.CreateFog();
            ActiveTrails.Add(effect);
            return effect;
        }

        public void Record(Vector2 position)
        {
            if (_points.Count > 0 && Vector2.SqrMagnitude(_points[^1] - position) < 1f) return;
            _points.Add(position);
            _line.positionCount = _points.Count;
            _line.SetPosition(_points.Count - 1, new Vector3(position.x, position.y, 0f));
            if (_fog != null)
            {
                for (var i = 0; i < 7; i++)
                {
                    var layer = i % 3;
                    var parameters = new ParticleSystem.EmitParams
                    {
                        position = position + Random.insideUnitCircle * (layer == 0 ? 1.7f : 3.8f),
                        startColor = layer == 0
                            ? new Color(0.001f, 0.002f, 0.006f, Random.Range(0.88f, 1f))
                            : new Color(0.008f, 0.012f, 0.025f, Random.Range(0.7f, 0.9f)),
                        startSize = layer == 0 ? Random.Range(2.2f, 4.5f) : Random.Range(5f, 10f),
                        startLifetime = 3600f,
                        rotation = Random.Range(0f, Mathf.PI * 2f)
                    };
                    _fog.Emit(parameters, 1);
                }
            }
        }

        public static void ApplySceneEffects(IScene scene)
        {
            if (scene == null || ActiveTrails.Count == 0)
                return;

            var slowed = new HashSet<IUnit>();
            lock (scene.Units.LockObject)
            {
                foreach (var unit in scene.Units.Items)
                {
                    if (!unit.IsActive())
                        continue;

                    foreach (var trail in ActiveTrails)
                    {
                        if (trail == null || trail._points.Count < 2 ||
                            !trail.InsideTrail(unit.Body.WorldPosition()))
                            continue;

                        if (unit is IShip ship && ship.Systems.All.OfType<WarpDrive>().Any(drive => drive.IsWarping))
                            break;
                        // Trisolaris vessels are ordinary three-dimensional units.
                        // Imported builds may carry a stale FourDimensional flag;
                        // faction 22 must still be slowed by black-domain trails.
                        if (ShipStats.IsFourDimensionalUnit(unit) && unit.Type.FactionId != 22)
                            break;
                        if (unit.Type.Class == UnitClass.Ship || unit.Type.Class == UnitClass.Drone)
                            slowed.Add(unit);
                        else if (unit.Type.Class == UnitClass.Missile || unit.Type.Class == UnitClass.EnergyBolt)
                            unit.Vanish();
                        break;
                    }
                }
            }

            foreach (var unit in slowed)
            {
                var velocity = unit.Body.Velocity;
                var normalMaximum = unit is IShip ship ? ship.Engine.MaxVelocity : 40f;
                var blackDomainMaximum = Mathf.Max(0.5f, normalMaximum * 0.1f);
                if (velocity.sqrMagnitude <= blackDomainMaximum * blackDomainMaximum)
                    continue;

                var limitedVelocity = velocity.normalized * blackDomainMaximum;
                if (unit.Body is RigidBodyAdapter rigidBody)
                    rigidBody.Velocity = limitedVelocity;
                else
                    unit.Body.ApplyAcceleration(limitedVelocity - velocity);
            }
        }

        private bool InsideTrail(Vector2 position)
        {
            for (var i = 1; i < _points.Count; i++)
            {
                var a = _points[i - 1];
                var b = _points[i];
                var ab = b - a;
                var t = Mathf.Clamp01(Vector2.Dot(position - a, ab) / Mathf.Max(ab.sqrMagnitude, 0.001f));
                if (Vector2.SqrMagnitude(position - (a + ab * t)) <= TrailRadius * TrailRadius) return true;
            }
            return false;
        }

        public static bool TryBlockRay(Vector2 origin, Vector2 direction, float maxRange, out float blockDistance)
        {
            blockDistance = maxRange;
            var rayEnd = origin + direction.normalized * maxRange;
            var blocked = false;
            foreach (var trail in ActiveTrails.Where(item => item != null && item._points.Count > 1))
            {
                for (var i = 1; i < trail._points.Count; i++)
                {
                    if (!ClosestSegmentParameters(origin, rayEnd, trail._points[i - 1], trail._points[i], out var rayT, out var distance))
                        continue;
                    if (distance > TrailRadius)
                        continue;

                    blockDistance = Mathf.Min(blockDistance, Mathf.Max(0f, rayT * maxRange - TrailRadius));
                    blocked = true;
                }
            }
            return blocked;
        }

        public static void ClearAll()
        {
            foreach (var trail in ActiveTrails.ToArray())
                if (trail != null)
                    Destroy(trail.gameObject);
            ActiveTrails.Clear();
            StrategicFieldEffect.ClearAll();
        }

        public static bool IsInsideAnyTrail(Vector2 position, float extraRadius = 0f)
        {
            foreach (var trail in ActiveTrails.Where(item => item != null && item._points.Count > 1))
            {
                for (var i = 1; i < trail._points.Count; ++i)
                {
                    var a = trail._points[i - 1];
                    var b = trail._points[i];
                    var ab = b - a;
                    var t = Mathf.Clamp01(Vector2.Dot(position - a, ab) / Mathf.Max(ab.sqrMagnitude, 0.001f));
                    if (Vector2.Distance(position, a + ab * t) <= TrailRadius + extraRadius)
                        return true;
                }
            }
            return false;
        }

        private static bool ClosestSegmentParameters(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2, out float firstT, out float distance)
        {
            var d1 = q1 - p1;
            var d2 = q2 - p2;
            var r = p1 - p2;
            var a = Vector2.Dot(d1, d1);
            var e = Vector2.Dot(d2, d2);
            var f = Vector2.Dot(d2, r);
            float s;
            float t;

            if (a <= 0.0001f && e <= 0.0001f)
            {
                firstT = 0f;
                distance = Vector2.Distance(p1, p2);
                return true;
            }
            if (a <= 0.0001f)
            {
                s = 0f;
                t = Mathf.Clamp01(f / e);
            }
            else
            {
                var c = Vector2.Dot(d1, r);
                if (e <= 0.0001f)
                {
                    t = 0f;
                    s = Mathf.Clamp01(-c / a);
                }
                else
                {
                    var b = Vector2.Dot(d1, d2);
                    var denominator = a * e - b * b;
                    s = denominator != 0f ? Mathf.Clamp01((b * f - c * e) / denominator) : 0f;
                    t = (b * s + f) / e;
                    if (t < 0f)
                    {
                        t = 0f;
                        s = Mathf.Clamp01(-c / a);
                    }
                    else if (t > 1f)
                    {
                        t = 1f;
                        s = Mathf.Clamp01((b - c) / a);
                    }
                }
            }

            firstT = s;
            distance = Vector2.Distance(p1 + d1 * s, p2 + d2 * t);
            return true;
        }

        private void CreateFog()
        {
            _fog = gameObject.AddComponent<ParticleSystem>();
            var main = _fog.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startSpeed = 0f;
            main.startLifetime = 3600f;
            main.maxParticles = 5000;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = _fog.emission;
            emission.enabled = false;
            var shape = _fog.shape;
            shape.enabled = false;
            var renderer = _fog.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.World;
            var material = new Material(Shader.Find("Sprites/Default"));
            material.mainTexture = CreateFogTexture();
            renderer.material = material;
            renderer.sortingOrder = 19;
            _fog.Play();
        }

        private static Texture2D CreateFogTexture()
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Preview6BlackDomainFog",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            var pixels = new Color32[size * size];
            var random = new System.Random(0xB1A6);
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = (x + 0.5f) / size * 2f - 1f;
                var dy = (y + 0.5f) / size * 2f - 1f;
                var radius = Mathf.Sqrt(dx * dx + dy * dy);
                var fineNoise = (float)random.NextDouble() * 0.22f - 0.11f;
                var cloudNoise = 0.12f * Mathf.Sin(x * 0.29f + Mathf.Sin(y * 0.13f) * 2.1f) +
                                 0.09f * Mathf.Cos(y * 0.23f - Mathf.Sin(x * 0.17f) * 1.8f);
                var filament = 0.07f * Mathf.Sin((x + y) * 0.47f);
                var alpha = Mathf.Clamp01((1f - radius + fineNoise + cloudNoise + filament) * 1.35f);
                alpha = alpha * alpha * (3f - 2f * alpha);
                var blue = (byte)Mathf.Clamp(8f + cloudNoise * 28f, 2f, 18f);
                var gray = (byte)Mathf.Clamp(2f + cloudNoise * 12f, 0f, 8f);
                pixels[y * size + x] = new Color32(gray, (byte)(gray + 1), blue, (byte)(alpha * 252f));
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private void OnDestroy()
        {
            ActiveTrails.Remove(this);
        }

        private IScene _scene;
        private IShip _owner;
        private LineRenderer _line;
        private ParticleSystem _fog;
        private readonly List<Vector2> _points = new();
        private const float TrailRadius = 6f;
        private static readonly List<WarpTrailEffect> ActiveTrails = new();
    }
}
