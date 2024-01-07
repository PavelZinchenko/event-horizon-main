using UnityEngine;
using UnityEngine.UI;

namespace ShipEditor.UI
{
	public class ComponentActionPanel : MonoBehaviour
	{
		public enum Status
		{
			None,
			Locked,
			CanRemove,
			CanInstall,
			NotCompatible,
			AlreadyInstalled,
		}

		[SerializeField] private GameObject _alreadyInstalledLabel;
		[SerializeField] private GameObject _notSuitableLabel;
		[SerializeField] private GameObject _installLabel;
		[SerializeField] private Button _deleteButton;
		[SerializeField] private Button _unlockButton;
		[SerializeField] private Button _unlockAllButton;

		public void Show(Status status)
		{
			gameObject.SetActive(status != Status.None);
			_alreadyInstalledLabel.SetActive(status == Status.AlreadyInstalled);
			_notSuitableLabel.SetActive(status == Status.NotCompatible);
			_installLabel.SetActive(status == Status.CanInstall);
			_deleteButton.gameObject.SetActive(status == Status.CanRemove);
			_unlockButton.gameObject.SetActive(status == Status.Locked);
			_unlockAllButton.gameObject.SetActive(status == Status.Locked);
		}
	}
}
