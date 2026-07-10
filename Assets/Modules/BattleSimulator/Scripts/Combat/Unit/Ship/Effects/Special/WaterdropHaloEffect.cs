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
            EnsureAnimation();
            if (_renderer == null)
                return;

            var moving = ship.Body.WorldVelocity().sqrMagnitude > 0.25f;
            _renderer.enabled = moving;
            if (!moving)
            {
                _elapsed = 0f;
                _lastFrame = -1;
                return;
            }

            _elapsed += elapsedTime;
            var sequenceTime = Mathf.Repeat(_elapsed, SequenceDuration);
            var frame = Mathf.Clamp(Mathf.FloorToInt(sequenceTime / SequenceDuration * _frames.Count), 0, _frames.Count - 1);
            if (frame != _lastFrame)
            {
                if (frame < _lastFrame || _lastFrame < 0)
                    PlayChime(ship.Body.VisualWorldPosition());
                _renderer.sprite = _frames[frame];
                _lastFrame = frame;
            }

            var forward = RotationHelpers.Direction(ship.Body.VisualWorldRotation());
            var scale = ship.Body.WorldScale();
            _root.transform.position = ship.Body.VisualWorldPosition() - forward * scale * 0.72f;
            _root.transform.eulerAngles = new Vector3(0f, 0f, ship.Body.VisualWorldRotation());
            // The source frames are 360 px at 100 PPU (3.6 world units).
            // A scale of scale / 3.6 makes the largest ring approximately one
            // waterdrop diameter, matching the supplied animation artwork.
            _root.transform.localScale = Vector3.one * Mathf.Max(0.01f, scale / 3.6f);
        }

        public void Dispose()
        {
            if (_root != null)
                Object.Destroy(_root);
            foreach (var frame in _frames)
                if (frame != null)
                    Object.Destroy(frame);
            _frames.Clear();
            if (_audioObject != null)
                Object.Destroy(_audioObject);
            if (_chime != null)
                Object.Destroy(_chime);
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

        private void EnsureAnimation()
        {
            if (_root != null)
                return;

            _root = new GameObject("WaterdropPropulsionHaloFrames");
            _renderer = _root.AddComponent<SpriteRenderer>();
            _renderer.sortingOrder = 45;
            for (var i = 1; i <= FrameCount; i++)
            {
                var texture = Resources.Load<Texture2D>($"Textures/WaterdropHaloFrames/{i}");
                if (texture == null)
                    continue;
                _frames.Add(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), 100f));
            }

            if (_frames.Count > 0)
                _renderer.sprite = _frames[0];
            else
                _renderer.enabled = false;
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

        private const int FrameCount = 29;
        private const float SequenceDuration = 0.4f;
        private readonly List<Sprite> _frames = new();
        private GameObject _root;
        private SpriteRenderer _renderer;
        private float _elapsed;
        private int _lastFrame = -1;
        private GameObject _audioObject;
        private AudioSource _audio;
        private AudioClip _chime;
    }
}
