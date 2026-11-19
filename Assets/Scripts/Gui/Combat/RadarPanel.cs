using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Integration;
using Combat.Integration.Legacy;
using Services.Resources;
using Zenject;

namespace Gui.Combat
{
    public class RadarPanel : MonoBehaviour
    {
        [Inject] private readonly IResourceLocator _resourceLocator;
        [Inject] private readonly IScenePresentation _scene;

        [Inject]
        private void Initialize()
        {
            _radars.AddRange(transform.Children<Radar>());
            _beaconRadars.AddRange(transform.Children<BeaconRadar>());
        }

        // Temporary entry for exploration, which has no combat adapter, until phase 6.
        public void Add(IShip ship)
        {
            Add(LegacyShipView.Of(ship));
        }

        public void Add(IShipView ship)
        {
            (_radars.FirstOrDefault(item => !item.gameObject.activeSelf) ?? CreateRadar()).Open(ship, _scene, _resourceLocator);
        }

        // Temporary entries for the legacy exploration scene.
        public void AddBeacon(IUnit unit) => AddBeacon(LegacyBeaconTarget.Of(unit));
        public void RemoveBeacon(IUnit unit) => RemoveBeacon(LegacyBeaconTarget.Of(unit));

        public void AddBeacon(IBeaconTarget target)
        {
            (_beaconRadars.FirstOrDefault(item => !item.gameObject.activeSelf) ?? CreateBeaconRadar()).Open(target, _scene);
        }

        public void RemoveBeacon(IBeaconTarget target)
        {
            var index = _beaconRadars.FindIndex(item => item.Target == target);
            if (index >= 0)
                _beaconRadars[index].Close();
        }

        public void Initialize(IEnumerable<IUnit> items)
        {
            var radarCount = _radars.Count;

            var radarIndex = 0;
            var simpleRadarIndex = 0;
            foreach (var item in items)
            {
                if (item.Type.Class == UnitClass.Ship)
                {
                    if (radarIndex >= radarCount)
                        CreateRadar();

                    _radars[radarIndex].Open(LegacyShipView.Of((IShip)item), _scene, _resourceLocator);

                    radarIndex++;
                }
            }

            for (var i = radarIndex; i < radarCount; ++i)
                _radars[i].Close();
        }

        private Radar CreateRadar()
        {
            var radar = GameObject.Instantiate(_radars[0]);
            radar.RectTransform.SetParent(transform);
            radar.RectTransform.SetParent(transform);
            radar.RectTransform.localScale = Vector3.one;
            _radars.Add(radar);
            return radar;
        }

        private BeaconRadar CreateBeaconRadar()
        {
            var radar = GameObject.Instantiate(_beaconRadars[0]);
            radar.RectTransform.SetParent(transform);
            radar.RectTransform.SetParent(transform);
            radar.RectTransform.localScale = Vector3.one;
            _beaconRadars.Add(radar);
            return radar;
        }

        private readonly List<Radar> _radars = new List<Radar>();
        private readonly List<BeaconRadar> _beaconRadars = new List<BeaconRadar>();
    }
}
