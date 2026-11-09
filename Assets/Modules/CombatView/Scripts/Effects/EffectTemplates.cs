using System.Collections.Generic;
using UnityEngine;

namespace CombatView.Effects
{
    // One template per record element type and image (migration phase 5b, step 3), as legacy
    // PrefabCache.GetEffectPrefab: a configured copy of the type's prefab, kept inactive in the pool storage. The
    // instances of an element come from its template, so the prefab itself never changes.
    internal sealed class EffectTemplates
    {
        public EffectTemplates(PrefabPool prefabs)
        {
            _prefabs = prefabs;
        }

        // Null for Shake, which has no prefab, and for a missing prefab.
        public GameObject Get(EffectKind kind, Sprite image)
        {
            var key = (kind, image);
            if (_templates.TryGetValue(key, out var template))
                return template;

            var path = PrefabPath(kind);
            var prefab = path != null ? _prefabs.Load(path) : null;
            if (prefab)
            {
                template = _prefabs.CreateTemplate(prefab);
                template.GetComponent<IEffectElement>()?.Configure(kind, image);
            }

            _templates.Add(key, template);
            return template;
        }

        // Legacy PrefabCache.GetEffectPrefab, plus the new Laser (owner question 10).
        private static string PrefabPath(EffectKind kind)
        {
            switch (kind)
            {
                case EffectKind.Flash: return "Effects/Flash";
                case EffectKind.FlashAdditive: return "Effects/FlashAdditive";
                case EffectKind.Shockwave: return "Effects/Wave";
                case EffectKind.Smoke: return "Effects/SmokeSimple";
                case EffectKind.SmokeAdditive: return "Effects/SmokeSimpleAdditive";
                case EffectKind.Spark: return "Effects/Spark";
                case EffectKind.Sprite: return "Effects/Sprite";
                case EffectKind.Lightning:
                case EffectKind.LightningStrike: return "Effects/LightningStrike";
                case EffectKind.Laser: return "Effects/Laser";
                default: return null;
            }
        }

        private readonly PrefabPool _prefabs;
        private readonly Dictionary<(EffectKind, Sprite), GameObject> _templates = new Dictionary<(EffectKind, Sprite), GameObject>();
    }
}
