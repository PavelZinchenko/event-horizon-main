using UnityEngine;
using Game.Exploration;

namespace GameContent
{
    [CreateAssetMenu(fileName = "PlanetSettings", menuName = "ScriptableObjects/PlanetSettings", order = 1)]
    public class PlanetSettings : ScriptableObject
    {
        [SerializeField] private Sprite _asteroidBelt;
        [SerializeField] private Sprite _infectedPlanet;
        [SerializeField] private Sprite[] _gasPlanets;
        [SerializeField] private Sprite[] _barrenPlanets;
        [SerializeField] private Sprite[] _terranPlanets;

        public Sprite GetPlanetImage(PlanetType type, int seed)
        {
            switch (type)
            {
                case PlanetType.Infected:
                    return _infectedPlanet;
                case PlanetType.Barren:
                    return _barrenPlanets[Mathf.Abs(seed) % _barrenPlanets.Length];
                case PlanetType.Gas:
                    return _gasPlanets[Mathf.Abs(seed) % _gasPlanets.Length];
                case PlanetType.Terran:
                    return _terranPlanets[Mathf.Abs(seed) % _terranPlanets.Length];
                case PlanetType.Asteroids:
                    return _asteroidBelt;
                default:
                    throw new System.ArgumentException();
            }
        }
    }
}
