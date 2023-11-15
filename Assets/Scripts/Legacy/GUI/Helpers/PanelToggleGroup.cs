using UnityEngine;

namespace Gui
{
    public class PanelToggleGroup : MonoBehaviour
    {
        public GameObject[] Panels;

        public void Show(GameObject panel)
        {
            foreach (var item in Panels)
                if (item)
                    item.SetActive(item == panel);
        }
    }
}
