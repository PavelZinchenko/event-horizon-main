using System.Linq;
using Galaxy;
using Game.Exploration;
using GameDatabase.DataModel;
using UnityEngine;
using UnityEngine.UI;
using GameServices.Player;
using Gui.Exploration;
using Gui.StarMap;
using Services.Gui;
using Services.Localization;
using Services.Messenger;
using Services.ObjectPool;
using Services.Resources;
using Zenject;

namespace ViewModel
{
	public class InformationPanel : MonoBehaviour
	{
	    [Inject] private readonly MotherShip _motherShip;
	    [Inject] private readonly Planet.Factory _planetFactory;
	    [Inject] private readonly IGameObjectFactory _factory;
	    [Inject] private readonly ILocalization _localization;
	    [Inject] private readonly IMessenger _messenger;
	    [Inject] private readonly IResourceLocator _resourceLocator;

        [SerializeField] private Text NameText;
        [SerializeField] private Text FactionNameText;
        [SerializeField] private LayoutGroup Planets;
        [SerializeField] private InputField Bookmark;
        [SerializeField] private LayoutGroup ObjectsGroup;

        public void OnBookmarkChanged(string value)
		{
			if (_suppressBookmarkChangeEvent) return;

			var star = _motherShip.CurrentStar;
			star.Bookmark = value;
		}

		public void Initialize(WindowArgs args)
		{
			_star = _motherShip.CurrentStar;

			NameText.text = _star.Name;

			_suppressBookmarkChangeEvent = true;
			Bookmark.text = _star.Bookmark;
			_suppressBookmarkChangeEvent = false;

			var faction = _star.Region.Faction;
			if (faction != Faction.Empty)
			{
				FactionNameText.gameObject.SetActive(true);
				FactionNameText.color = faction.Color;
				FactionNameText.text = _localization.GetString(faction.Name);
			}
			else
			{
				FactionNameText.gameObject.SetActive(false);
			}

			Planets.transform.InitializeElements<PlanetInfo, Planet>(_planetFactory.CreatePlanets(_star.Id), UpdatePlanetInfo, _factory);
		    ObjectsGroup.transform.InitializeElements<StarSystemObjectItem, StarObjectType>(_star.Objects.ToEnumerable().Where(item => item.IsActive(_star)), UpdateStarObject);
		}

		public void OnObjectClicked(StarSystemObjectItem starSystemObject)
		{
			if (_star.Id != _motherShip.CurrentStar.Id) return;
			if (_star.Occupant.IsAggressive) return;

			_messenger.Broadcast<StarObjectType>(EventType.ArrivedToObject, starSystemObject.Type);
		}

		private void UpdatePlanetInfo(PlanetInfo planet, Planet model)
		{
			planet.UpdatePlanet(model);
		}

        private void UpdateStarObject(StarSystemObjectItem item, StarObjectType type)
        {
            item.Initialize(_motherShip.CurrentStar, type, _localization, _resourceLocator);
        }

		private Galaxy.Star _star;
		private bool _suppressBookmarkChangeEvent = false;
	}
}
