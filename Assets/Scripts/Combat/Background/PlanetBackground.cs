using System;
using Game.Exploration;
using Services.Resources;
using UnityEngine;
using Zenject;

namespace Combat.Background
{
    public class PlanetBackground : MonoBehaviour
    {
        [SerializeField] private float _size = 50f;
        [SerializeField] private Material _gasPlanetMaterial;
        [SerializeField] private Material _barrenPlanetMaterial;
        [SerializeField] private Material _infectedPlanetMaterial;

        [Inject]
        public void Initialize(IResourceLocator resourceLocator, Planet planet)
        {
            _planet = planet;

            // The old code made both sides of the mesh equal to
            // size * screenAspect.  On wide displays that produced a square
            // planetary surface in the middle of the camera, leaving the
            // camera clear colour visible down both sides of exploration.
            // Keep a unit mesh and scale it to the actual camera viewport
            // instead, so the planet always covers the complete view.
            Primitives.CreatePlane(gameObject.GetMesh(), 1f, 1f, 8);
            UpdateViewSize();

            switch (planet.Type)
            {
                case PlanetType.Gas:
                    InitializeGasMaterial();
                    break;
                case PlanetType.Infected:
                    InitializeInfectedMaterial(resourceLocator);
                    break;
                case PlanetType.Barren:
                case PlanetType.Terran:
                    InitializeBarrenMaterial(resourceLocator);
                    break;
                default:
                    throw new ArgumentException("PlanetBackground: Wrong planet type - " + planet.Type);
            }
        }

        private void InitializeGasMaterial()
        {
            gameObject.AddComponent<MeshRenderer>().sharedMaterial = _gasPlanetMaterial;

            //var random = new System.Random(planet.Seed);
            //_material.SetTexture("_DecalTex", resourceLocator.GetNebulaTexture(random.Next()));
            //_material.SetTexture("_CloudsTex", resourceLocator.GetNebulaTexture(random.Next()));
            _gasPlanetMaterial.color = Color.Lerp(_planet.Color, Color.black, 0.75f);
        }

        private void InitializeBarrenMaterial(IResourceLocator resourceLocator)
        {
            gameObject.AddComponent<MeshRenderer>().sharedMaterial = _barrenPlanetMaterial;

            var random = new System.Random(_planet.Seed);
            //_barrenPlanetMaterial.SetTexture("_CloudsTex", resourceLocator.GetNebulaTexture(random.Next()));
            _barrenPlanetMaterial.color = Color.Lerp(_planet.Color, Color.black, 0.3f);
        }

        private void InitializeInfectedMaterial(IResourceLocator resourceLocator)
        {
            gameObject.AddComponent<MeshRenderer>().sharedMaterial = _infectedPlanetMaterial;

            var random = new System.Random(_planet.Seed);
            //_infectedPlanetMaterial.SetTexture("_CloudsTex", resourceLocator.GetNebulaTexture(random.Next()));
            _infectedPlanetMaterial.color = Color.Lerp(_planet.Color, Color.black, 0.3f);
        }

        private void LateUpdate()
        {
            UpdateViewSize();

            switch (_planet.Type)
            {
                case PlanetType.Gas:
                    UpdateGasMaterial();
                    break;
                case PlanetType.Infected:
                    UpdateInfectedMaterial();
                    break;
                case PlanetType.Barren:
                case PlanetType.Terran:
                    UpdateBarrenMaterial();
                    break;
            }
        }

        private void UpdateViewSize()
        {
            var camera = UnityEngine.Camera.main;
            if (camera == null)
            {
                _height = _size * Screen.width / Mathf.Max(1, Screen.height);
                _width = _height * Screen.width / Mathf.Max(1, Screen.height);
            }
            else
            {
                _height = 2f * camera.orthographicSize;
                _width = _height * camera.aspect;
            }

            _width *= BackgroundOverscan;
            _height *= BackgroundOverscan;
            transform.localScale = new Vector3(_width, _height, 1f);
        }

        private void UpdateBarrenMaterial()
        {
            var offset = transform.position;

            offset.x /= _width;
            offset.y /= _height;
            offset.x -= Mathf.FloorToInt(offset.x);
            offset.y -= Mathf.FloorToInt(offset.y);
            _barrenPlanetMaterial.mainTextureOffset = offset;
        }

        private void UpdateInfectedMaterial()
        {
            var offset = transform.position;

            offset.x /= _width;
            offset.y /= _height;
            offset.x -= Mathf.FloorToInt(offset.x);
            offset.y -= Mathf.FloorToInt(offset.y);
            _infectedPlanetMaterial.mainTextureOffset = offset;
        }

        private void UpdateGasMaterial()
        {
            var offset = transform.position;

            offset.x /= _width;
            offset.y /= _height;
            offset.x -= Mathf.FloorToInt(offset.x);
            offset.y -= Mathf.FloorToInt(offset.y);
            _gasPlanetMaterial.mainTextureOffset = offset;

            var decalOffset = offset * 2;
            decalOffset.x -= Mathf.FloorToInt(offset.x);
            decalOffset.y -= Mathf.FloorToInt(offset.y);
            _gasPlanetMaterial.SetTextureOffset("_DecalTex", decalOffset);

            var cloudOffset = offset * 3;
            cloudOffset.x -= Mathf.FloorToInt(offset.x);
            cloudOffset.y -= Mathf.FloorToInt(offset.y);
            _gasPlanetMaterial.SetTextureOffset("_CloudsTex", cloudOffset);
        }

        private Planet _planet;
        private float _width;
        private float _height;

        private const float BackgroundOverscan = 1.05f;
    }
}
