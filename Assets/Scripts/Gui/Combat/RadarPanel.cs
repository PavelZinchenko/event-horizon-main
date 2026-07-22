using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Services.Resources;
using Zenject;
using UnityEngine.UI;

namespace Gui.Combat
{
    public class RadarPanel : MonoBehaviour
    {
        [Inject] private readonly IResourceLocator _resourceLocator;
        [Inject] private readonly IScene _scene;

        [Inject]
        private void Initialize()
        {
            _radars.AddRange(transform.Children<Radar>());
            _beaconRadars.AddRange(transform.Children<BeaconRadar>());
            var minimap = new GameObject("Preview5CombatMinimap", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(CombatMinimap));
            minimap.transform.SetParent(transform.root, false);
            minimap.transform.SetAsLastSibling();
            var canvas = minimap.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 32000;
            var group = minimap.AddComponent<CanvasGroup>();
            group.interactable = true;
            group.blocksRaycasts = true;
            minimap.GetComponent<CombatMinimap>().Initialize(_scene);

            var targetLine = new GameObject("Preview7TargetLine", typeof(CombatTargetLine));
            targetLine.GetComponent<CombatTargetLine>().Initialize(_scene);
        }

        public void Add(IShip ship)
        {
            (_radars.FirstOrDefault(item => !item.gameObject.activeSelf) ?? CreateRadar()).Open(ship, _scene, _resourceLocator);
        }

        public void AddBeacon(IUnit unit)
        {
            (_beaconRadars.FirstOrDefault(item => !item.gameObject.activeSelf) ?? CreateBeaconRadar()).Open(unit, _scene);
        }

        public void RemoveBeacon(IUnit unit)
        {
            var index = _beaconRadars.FindIndex(item => item.Unit == unit);
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

                    _radars[radarIndex].Open((IShip)item, _scene, _resourceLocator);

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
