using System.Linq;
using GameServices.Player;
using GameStateMachine.States;
using Constructor.Ships;
using GameDatabase;
using GameDatabase.DataModel;
using UnityEngine;
using Zenject;
using Gui.Constructor;

namespace ViewModel
{
	public class FleetPanelViewModel : MonoBehaviour
	{
	    [InjectOptional] private readonly PlayerFleet _playerFleet;
		[SerializeField] private ShipListContentFiller _contentFiller;
		[SerializeField] private ListScrollRect _shipList;
	    [Inject] private readonly IDatabase _database;

		public void Open(bool isEditorMode)
		{
            gameObject.SetActive(true);
			UpdateActiveShips(isEditorMode);
		}

	    public void Close()
	    {
	        gameObject.SetActive(false);
	    }

		private void UpdateActiveShips(bool isEditorMode)
		{
			if (!gameObject.activeSelf)
				return;

			var ships = Enumerable.Empty<IShip>();

            if (isEditorMode)
                ships = _database.ShipBuildList.OrderBy(build => build.Ship.Id.Value).Select<ShipBuild,IShip>(build => new EditorModeShip(build, _database));
            else if (_playerFleet != null)
			    ships = _playerFleet.ActiveShipGroup.Ships;

			_contentFiller.Initialize(ships);
			_shipList.RefreshContent();
		}

		public void ShipButtonClicked(ShipItem item)
		{
        }
    }
}
