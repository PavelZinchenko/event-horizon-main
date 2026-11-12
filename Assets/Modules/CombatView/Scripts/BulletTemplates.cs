using System.Collections.Generic;
using CombatView.Views;
using UnityEngine;

namespace CombatView
{
    // One template per bullet look (migration phase 5b, step 5), as legacy PrefabCache.GetBulletPrefab: a configured
    // copy of the shape's prefab, kept inactive in the pool storage. The instances come from the template, so the
    // prefab itself never changes.
    internal sealed class BulletTemplates
    {
        public BulletTemplates(PrefabPool prefabs)
        {
            _prefabs = prefabs;
        }

        // Null for a missing prefab.
        public GameObject Get(BulletLook look)
        {
            if (_templates.TryGetValue(look, out var template))
                return template;

            var prefab = _prefabs.Load(PrefabPath(look.Kind));
            if (prefab)
            {
                template = _prefabs.CreateTemplate(prefab);
                var setup = template.GetComponent<BulletViewSetup>();
                if (setup)
                    setup.Configure(look);
                else
                    template = null;
            }

            _templates.Add(look, template);
            return template;
        }

        public void Clear() => _templates.Clear();

        // Legacy PrefabCache.GetBulletPrefab.
        private static string PrefabPath(BulletViewKind kind)
        {
            switch (kind)
            {
                case BulletViewKind.Rocket: return "Bullets/CommonRocket";
                case BulletViewKind.Laser: return "Bullets/Laser";
                case BulletViewKind.Lightning: return "Bullets/Lightning";
                case BulletViewKind.EnergyBeam: return "Bullets/EnergyBeam";
                case BulletViewKind.PiercingLaser: return "Bullets/PiercingLaserBeam";
                case BulletViewKind.Spark: return "Bullets/Spark";
                case BulletViewKind.Mine: return "Bullets/Mine";
                case BulletViewKind.Arc: return "Bullets/Arc";
                case BulletViewKind.BlackHole: return "Bullets/WormHole";
                case BulletViewKind.Harpoon: return "Bullets/Hook";
                case BulletViewKind.CircularSaw: return "Bullets/CircularSaw";
                default: return "Bullets/CommonProjectile";
            }
        }

        private readonly PrefabPool _prefabs;
        private readonly Dictionary<BulletLook, GameObject> _templates = new Dictionary<BulletLook, GameObject>();
    }
}
