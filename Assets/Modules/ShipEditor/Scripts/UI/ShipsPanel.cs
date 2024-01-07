using UnityEngine;
using ShipEditor.Model;
using Zenject;

namespace ShipEditor.UI
{
    public class ShipsPanel : MonoBehaviour
    {
		[Inject] private readonly IShipEditorModel _shipEditor;
		
		[SerializeField] private ShipListContentFiller _contentFiller;
        [SerializeField] private ListScrollRect _shipList;

        private void Start()
        {
			_contentFiller.Initialize(_shipEditor.Inventory.Ships);
			_shipList.RefreshContent();
		}

		public void OnShipClicked(ShipItem item)
		{
			_shipEditor.SelectShip(item.Ship);
		}

		public bool Visible
		{
			get => gameObject.activeSelf;
			set => gameObject.SetActive(value);
		}
	}
}
