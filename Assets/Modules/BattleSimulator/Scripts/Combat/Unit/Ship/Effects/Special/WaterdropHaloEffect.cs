using System.Collections.Generic;
using Combat.Component.Engine;
using Combat.Component.Features;
using Combat.Component.Ship;
using Combat.Component.Stats;
using Combat.Component.Systems;
using Combat.Component.Triggers;
using UnityEngine;

namespace Combat.Component.Ship.Effects.Special
{
    public sealed class WaterdropHaloEffect : IShipEffect
    {
        public bool IsAlive => true;
        public IEngineModification EngineModification => null;
        public IFeaturesModification FeaturesModification => null;
        public ISystemsModification SystemsModification => null;
        public IStatsModification StatsModification => null;
        public IUnitAction UnitAction => null;

        public void UpdatePhysics(IShip ship, float elapsedTime) { }

        public void UpdateView(IShip ship, float elapsedTime)
        {
            _pulseTimer += elapsedTime;
            if (ship.Body.WorldVelocity().sqrMagnitude > 0.25f)
            {
                while (_pulseTimer >= PulseInterval)
                {
                    _pulseTimer -= PulseInterval;
                    SpawnRing();
                    PlayChime(ship.Body.VisualWorldPosition());
                }
            }
            else
            {
                _pulseTimer = Mathf.Min(_pulseTimer, PulseInterval);
            }

            var forward = RotationHelpers.Direction(ship.Body.VisualWorldRotation());
            var scale = ship.Body.WorldScale();
            var tail = ship.Body.VisualWorldPosition() - forward * scale * 0.72f;
            for (var i = _rings.Count - 1; i >= 0; i--)
            {
                var ring = _rings[i];
                ring.Age += elapsedTime;
                if (ring.Age >= RingLifetime)
                {
                    Object.Destroy(ring.Root);
                    _rings.RemoveAt(i);
                    continue;
                }

                var t = Mathf.Clamp01(ring.Age / RingLifetime);
                var radius = Mathf.Lerp(scale * 0.04f, scale * 0.5f, Mathf.SmoothStep(0f, 1f, t));
                var color = t < 0.5f
                    ? Color.Lerp(new Color(0.15f, 0.65f, 1f), new Color(1f, 0.9f, 0.12f), t * 2f)
                    : Color.Lerp(new Color(1f, 0.9f, 0.12f), new Color(1f, 0.08f, 0.02f), (t - 0.5f) * 2f);
                color.a = 1f - Mathf.Pow(t, 3f);
                ring.Line.startColor = ring.Line.endColor = color;
                ring.Line.widthMultiplier = Mathf.Lerp(scale * 0.035f, scale * 0.012f, t);
                for (var point = 0; point < RingSegments; point++)
                {
                    var angle = point * Mathf.PI * 2f / (RingSegments - 1);
                    ring.Line.SetPosition(point, new Vector3(
                        tail.x + Mathf.Cos(angle) * radius,
                        tail.y + Mathf.Sin(angle) * radius,
                        0f));
                }
            }
        }

        public void Dispose()
        {
            foreach (var ring in _rings)
                if (ring.Root != null)
                    Object.Destroy(ring.Root);
            _rings.Clear();
            if (_audioObject != null)
                Object.Destroy(_audioObject);
            if (_chime != null)
                Object.Destroy(_chime);
            if (_material != null)
                Object.Destroy(_material);
        }

        public static void ShowReflection(Vector2 origin, Vector2 direction, float scale)
        {
            var root = new GameObject("WaterdropReflectedLaser");
            var line = root.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.SetPosition(0, origin);
            line.SetPosition(1, origin + direction.normalized * Mathf.Max(35f, scale * 12f));
            line.widthMultiplier = Mathf.Max(0.08f, scale * 0.025f);
            line.startColor = new Color(0.85f, 1f, 1f, 1f);
            line.endColor = new Color(0.12f, 0.75f, 1f, 0.18f);
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.sortingOrder = 60;
            var fade = root.AddComponent<ReflectionFade>();
            fade.Line = line;
        }

        private void SpawnRing()
        {
            if (_material == null)
                _material = new Material(Shader.Find("Sprites/Default"));
            var root = new GameObject("WaterdropPropulsionHalo");
            var line = root.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = true;
            line.positionCount = RingSegments;
            line.numCornerVertices = 4;
            line.material = _material;
            line.sortingOrder = 45;
            _rings.Add(new Ring(root, line));
        }

        private void PlayChime(Vector2 position)
        {
            if (_audioObject == null)
            {
                _audioObject = new GameObject("WaterdropHaloChime");
                _audio = _audioObject.AddComponent<AudioSource>();
                _audio.playOnAwake = false;
                _audio.spatialBlend = 0.45f;
                _audio.volume = 0.18f;
                _chime = CreateChime();
            }
            _audioObject.transform.position = position;
            _audio.PlayOneShot(_chime);
        }

        private static AudioClip CreateChime()
        {
            const int sampleRate = 22050;
            const int sampleCount = 1764;
            var samples = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                var t = i / (float)sampleRate;
                var envelope = Mathf.Exp(-42f * t);
                samples[i] = envelope * (0.75f * Mathf.Sin(2f * Mathf.PI * 1450f * t) +
                                         0.25f * Mathf.Sin(2f * Mathf.PI * 2250f * t));
            }
            var clip = AudioClip.Create("WaterdropHaloChime", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private sealed class Ring
        {
            public Ring(GameObject root, LineRenderer line) { Root = root; Line = line; }
            public readonly GameObject Root;
            public readonly LineRenderer Line;
            public float Age;
        }

        private sealed class ReflectionFade : MonoBehaviour
        {
            public LineRenderer Line;
            private float _age;
            private void Update()
            {
                _age += Time.deltaTime;
                var alpha = 1f - Mathf.Clamp01(_age / 0.18f);
                if (Line != null)
                {
                    var start = Line.startColor; start.a = alpha; Line.startColor = start;
                    var end = Line.endColor; end.a = alpha * 0.35f; Line.endColor = end;
                }
                if (_age >= 0.18f)
                {
                    if (Line != null && Line.material != null)
                        Object.Destroy(Line.material);
                    Object.Destroy(gameObject);
                }
            }
        }

        private const int RingSegments = 49;
        private const float PulseInterval = 0.4f;
        private const float RingLifetime = 0.36f;
        private readonly List<Ring> _rings = new();
        private float _pulseTimer;
        private Material _material;
        private GameObject _audioObject;
        private AudioSource _audio;
        private AudioClip _chime;
    }
}
