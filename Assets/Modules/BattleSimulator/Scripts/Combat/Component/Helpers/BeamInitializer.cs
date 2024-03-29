﻿using Combat.Component.View;
using GameDatabase.DataModel;
using Services.Resources;
using UnityEngine;

namespace Combat.Component.Helpers
{
    public class BeamInitializer : MonoBehaviour, IBulletPrefabInitializer
    {
        [SerializeField] private LineRenderer Renderer;
        [SerializeField] private LaserView LaserView;
        [SerializeField] private LightningView LightningView;
        [SerializeField] private ArcView ArcView;

        public void Initialize(BulletPrefab data, IResourceLocator resourceLocator)
        {
            var sprite = resourceLocator.GetSprite(data.Image);
            if (_material) Destroy(_material);
            _material = Renderer.material;
            _material.mainTexture = sprite == null ? null : sprite.texture;

            if (LaserView)
            {
                LaserView.Initialize(data.MainColor, data.MainColorMode);
                LaserView.BorderSize = data.Margins;
                LaserView.Thickness = data.Size;
            }
            else if (LightningView)
            {
                LightningView.Initialize(data.MainColor, data.MainColorMode);
                LightningView.Thickness = data.Size;
            }
            else if (ArcView)
            {
                ArcView.Initialize(data.MainColor, data.MainColorMode, data.Margins, data.Deformation);
            }
        }

        private void OnDestroy()
        {
            Destroy(_material);
        }

        private Material _material;
    }
}
