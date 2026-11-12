using System.Collections.Generic;
using CombatView.Effects;
using UnityEngine;

namespace CombatView
{
    // The effects that live on one ship view (migration phase 5b, step 4): legacy EngineLightEffect for each engine's
    // light and trail, ShieldEffect for the shield outline, and ShipRetreatingEffect with its sound loop while the ship
    // retreats. They are children of the ship view; Dispose gives them back before the view goes.
    internal sealed class ShipEffects
    {
        public ShipEffects(ShipLook look, Transform view, ShipEffectAssets assets)
        {
            _look = look;
            _view = view;
            _assets = assets;

            foreach (var engine in look.Engines)
            {
                // Legacy ShipFactory.CreateEngineLight and CreateTrail: the size of the light is in the ship's units.
                AddEngineEffect(EngineLightPrefab, engine.Position, 5f * engine.Size / look.Scale);
                if (assets.Trails != null)
                    AddEngineEffect(look.IsDrone ? DroneTrailPrefab : ShipTrailPrefab, engine.Position, engine.Size);
            }

            if (look.HasShield)
            {
                _shield = assets.Effects.Create(assets.Load(ShieldPrefab), view);
                if (_shield != null)
                    _shield.Color = look.EngineColor;
            }
        }

        // Once per tick, from the ship's snapshot. Legacy reads the throttle only while the engine has propulsion;
        // the core's throttle is the control in force.
        public void Capture(float throttle, float shieldShare, bool retreating)
        {
            _throttle = throttle;
            _shieldShare = shieldShare;

            if (retreating && _retreat == null)
                StartRetreat();
            else if (!retreating && _retreat != null)
                StopRetreat();
        }

        // Once per frame, before the effect pool ticks. color: the ship's colour with its opacity (legacy
        // Features.Color); opacity: legacy Features.Opacity.
        public void Update(float deltaTime, Color color, float opacity)
        {
            _power = Mathf.Max(_throttle, _power - 3 * deltaTime);
            foreach (var effect in _engines)
            {
                effect.Color = color * _look.EngineColor;
                effect.Life = _power;
            }

            if (_shield != null)
                _shield.Life = opacity * _shieldShare;

            if (_retreat != null)
            {
                _retreatTime += deltaTime;
                _retreat.Life = 0.5f + 0.25f * Mathf.Sin(_retreatTime);
            }
        }

        public void Dispose()
        {
            foreach (var effect in _engines)
                effect.Dispose();
            _engines.Clear();
            _shield?.Dispose();
            _shield = null;
            StopRetreat();
        }

        private void AddEngineEffect(string prefab, Vector2 position, float size)
        {
            var effect = _assets.Effects.Create(_assets.Load(prefab), _view);
            if (effect == null)
                return;

            if (effect is TrailEffect trail)
                trail.Pool = _assets.Trails;
            effect.Position = position;
            effect.Size = size;
            effect.Color = _look.EngineColor;
            effect.Life = 0f;
            _engines.Add(effect);
        }

        // Legacy ShipRetreatingEffect: a Wave in the warp colour at 1.5 x the ship's scale that follows the ship. As a
        // child of the view it takes the ship's scale.
        private void StartRetreat()
        {
            _retreatTime = 0f;
            _retreatSound = _assets.Sounds.Start(_assets.RetreatSound);
            _retreat = _assets.Effects.Create(_assets.Load(WavePrefab), _view);
            if (_retreat == null)
                return;

            _retreat.Color = WarpColor;
            _retreat.Size = 1.5f;
        }

        private void StopRetreat()
        {
            _retreat?.Dispose();
            _retreat = null;
            _assets.Sounds.Stop(_retreatSound);
            _retreatSound = 0;
        }

        // Legacy ColorTable.ShipWarpColor.
        public static readonly Color WarpColor = new Color(0.5f, 0.5f, 1f);

        private const string EngineLightPrefab = "Effects/EngineLight";
        private const string ShipTrailPrefab = "Effects/ShipTrail";
        private const string DroneTrailPrefab = "Effects/DroneTrail";
        private const string ShieldPrefab = "Effects/MainShield";
        private const string WavePrefab = "Effects/Wave";

        private readonly ShipLook _look;
        private readonly Transform _view;
        private readonly ShipEffectAssets _assets;
        private readonly List<IEffect> _engines = new List<IEffect>();
        private IEffect _shield;
        private IEffect _retreat;
        private int _retreatSound;
        private float _retreatTime;
        private float _throttle;
        private float _shieldShare;
        private float _power;
    }

    // What the ship effects share: the effect pool, the prefabs, the trails (null when trails are off) and the sounds.
    internal sealed class ShipEffectAssets
    {
        public ShipEffectAssets(EffectPool effects, PrefabPool prefabs, TrailPool trails, CombatSounds sounds)
        {
            Effects = effects;
            Prefabs = prefabs;
            Trails = trails;
            Sounds = sounds;
        }

        public EffectPool Effects { get; }
        public PrefabPool Prefabs { get; }
        public TrailPool Trails { get; }
        public CombatSounds Sounds { get; }

        // The legacy combat Settings component's ShipRetreatSound; the adapter sets it.
        public SoundClip RetreatSound { get; set; }

        public GameObject Load(string path) => Prefabs.Load(path);
    }
}
