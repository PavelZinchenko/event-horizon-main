using GameServices.Player;
using GameStateMachine.States;
using Gui.Windows;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ViewModel
{
	public class ShipPanelViewModel : MonoBehaviour
	{
	    [Inject] private readonly PlayerFleet _playerFleet;
	    [Inject] private readonly OpenShipEditorSignal.Trigger _openEditorTrigger;

		public GameObject ShipButtonPrefab;
		public AnimatedWindow CompanionPanel;
		public ToggleGroup CompanionToggleGroup;

		public void OnShipSelected()
		{
            // TODO
			//var ship = _playerFleet.ActiveShip;

			//if (ship != null)
			//	GetComponent<IWindow>().Open();
			//else
			//	GetComponent<IWindow>().Close();
		}

		public void OnCompanionToggleValueChanged(bool isOn)
		{
			if (isOn || CompanionToggleGroup.AnyTogglesOn())
				CompanionPanel.Open();
			else
				CompanionPanel.Close();
		}

		public void OpenShipConstructor()
		{
            // TODO
		    //_openConstructorTrigger.Fire();
		}
	}
}
