using UnityEngine;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using System.Collections.Generic;
using Combat.Component.Helpers;
using GameDatabase.DataModel;
using Services.Resources;

namespace Combat.Component.View
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class CircleView : BaseView, ICustomPrefabIntializer<GameObjectPrefab_CircularOutlineObject>
    {
        [SerializeField] private float _alphaScale = 1.0f;
        [SerializeField] private Color _baseColor = Color.white;
        [SerializeField] private ColorMode _colorMode = ColorMode.TakeFromOwner;

        private Mesh _mesh;
        private Material _material;
        private MeshRenderer _renderer;

        public void Initialize(GameObjectPrefab_CircularOutlineObject data, IResourceLocator resourceLocator)
        {
            var sprite = resourceLocator.GetSprite(data.Image);
            var meshFilter = GetComponent<MeshFilter>();

            _mesh = meshFilter.mesh;
            Material.mainTexture = sprite.texture;
            CreateMesh(_mesh, 0.5f*data.ImageScale, data.Thickness, data.AspectRatio);
        }

        private Material Material => _material != null ? _material : (_material = Renderer.material);
        private MeshRenderer Renderer => _renderer != null ? _renderer : (_renderer = GetComponent<MeshRenderer>());

        public override void Dispose() { }
        protected override void OnGameObjectCreated() { }

        protected override void OnGameObjectDestroyed()
        {
            if (_material != null)
            {
                Destroy(_material);
                _material = null;
            }

            if (_mesh != null)
            {
                Destroy(_mesh);
                _mesh = null;
            }
        }

        protected override void UpdateColor(Color color)
        {
            color = _colorMode.Apply(_baseColor, color);
            color.a *= _alphaScale;
            Material.color = color;
        }

        public static void CreateMesh(Mesh mesh, float radius, float thickness, float aspect)
        {
            mesh.Clear(false);

            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();

            var length = 2 * Mathf.PI * radius;
            var segments = Mathf.FloorToInt(length/(thickness*aspect));
            
            if (segments % 2 == 1) 
                segments++;

            for (var i = 0; i <= segments; ++i)
            {
                var angle = i*2*Mathf.PI/segments;
                var x = Mathf.Cos(angle) * radius;
                var y = Mathf.Sin(angle) * radius;
                vertices.Add(new Vector3(x, y, 0));
                x *= 1.0f - thickness / radius;
                y *= 1.0f - thickness / radius;
                vertices.Add(new Vector3(x, y, 0));
                var odd = i % 2 == 1;
                uv.Add(new Vector2(odd ? 1 : 0, 1));
                uv.Add(new Vector2(odd ? 1 : 0, 0));
            }

            for (var i = 0; i < segments; ++i)
            {
                var p1 = i * 2;
                var p2 = i * 2 + 1;
                var p3 = i * 2 + 2;
                var p4 = i * 2 + 3;
                triangles.Add(p2);
                triangles.Add(p1);
                triangles.Add(p3);
                triangles.Add(p3);
                triangles.Add(p4);
                triangles.Add(p2);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uv.ToArray();
        }

        protected override void UpdateLife(float life) {}
        protected override void UpdatePosition(Vector2 position) {}
        protected override void UpdateRotation(float rotation) {}
        protected override void UpdateSize(float size) {}
    }
}
