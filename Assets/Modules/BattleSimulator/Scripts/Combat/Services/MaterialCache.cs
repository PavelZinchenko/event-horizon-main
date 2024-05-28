using System.Collections.Generic;
using UnityEngine;

namespace Combat.Services
{
    public class MaterialCache : MonoBehaviour
    {
        [SerializeField] private Material _defaultMaterial;
        [SerializeField] private Material _hsvMaterial;

        private readonly Dictionary<int, Material> _hsvMaterialCache = new();

        public Material GetDefaultMaterial() => _defaultMaterial;

        public Material GetHsvMaterial(float hue, float saturation)
        {
            var hash = GetHsvMaterialHash(hue, saturation);
            if (hash == 0) return _defaultMaterial;

            if (!_hsvMaterialCache.TryGetValue(hash, out var material))
            {
                material = Instantiate(_hsvMaterial);
                material.SetColor("_HSVAAdjust", new Color(hue, saturation, 0));
                _hsvMaterialCache.Add(hash, material);
            }

            return material;
        }

        private void OnDestroy()
        {
            foreach (var item in _hsvMaterialCache.Values)
                Destroy(item);

            _hsvMaterialCache.Clear();
        }

        private int GetHsvMaterialHash(float hue, float saturation)
        {
            return Mathf.RoundToInt(Mathf.Clamp(hue*255,0,255)) + (Mathf.RoundToInt(Mathf.Clamp(saturation*255,0,255)) << 8);
        }
    }
}
