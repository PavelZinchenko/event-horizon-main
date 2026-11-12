using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CombatView
{
    // Copied from legacy Combat.Services.MaterialCache (migration phase 5b, step 4), without the MonoBehaviour: one
    // material per hue and saturation pair, shared by every ship and turret of that colour.
    public sealed class HsvMaterials : IDisposable
    {
        public HsvMaterials(Material defaultMaterial, Material hsvMaterial)
        {
            _defaultMaterial = defaultMaterial;
            _hsvMaterial = hsvMaterial;
        }

        public Material GetDefaultMaterial() => _defaultMaterial;

        // The default material without an HSV material or for no colour change.
        public Material GetHsvMaterial(float hue, float saturation)
        {
            var hash = GetHsvMaterialHash(hue, saturation);
            if (hash == 0 || !_hsvMaterial) return _defaultMaterial;

            if (!_hsvMaterialCache.TryGetValue(hash, out var material))
            {
                material = Object.Instantiate(_hsvMaterial);
                material.SetColor("_HSVAAdjust", new Color(hue, saturation, 0));
                _hsvMaterialCache.Add(hash, material);
            }

            return material;
        }

        public void Dispose()
        {
            foreach (var item in _hsvMaterialCache.Values)
                Object.Destroy(item);

            _hsvMaterialCache.Clear();
        }

        private static int GetHsvMaterialHash(float hue, float saturation)
        {
            return Mathf.RoundToInt(Mathf.Clamp(hue*255,0,255)) + (Mathf.RoundToInt(Mathf.Clamp(saturation*255,0,255)) << 8);
        }

        private readonly Material _defaultMaterial;
        private readonly Material _hsvMaterial;
        private readonly Dictionary<int, Material> _hsvMaterialCache = new Dictionary<int, Material>();
    }
}
