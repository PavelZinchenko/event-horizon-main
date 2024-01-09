using Services.Localization;
using UnityEngine;
using UnityEngine.UI;
using Services.Resources;
using Zenject;

namespace Gui.ComponentList
{
    public class GroupListItem : MonoBehaviour
    {
        [Inject] private readonly IResourceLocator _resourceLocator;
        [Inject] private readonly ILocalization _localization;

        [SerializeField] private Image _icon;
        [SerializeField] private GameObject _expandIcon;
        [SerializeField] private GameObject _collapseIcon;
        [SerializeField] private Text _nameText;
        [SerializeField] private Selectable _button;

        public void Initialize(IComponentTreeNode node, IComponentTreeNode activeNode)
        {
            Node = node;
            _icon.sprite = _resourceLocator.GetSprite(node.Icon);
            _icon.color = node.Color;
            _nameText.text = _localization.Localize(node.Name);

            var isParent = node.IsParent(activeNode);
            var isActive = node == activeNode;
            var isChild = !isParent && !isActive;
            var isEmpty = isChild && node.ItemCount == 0;

            if (_expandIcon) _expandIcon.gameObject.SetActive(isChild && !isEmpty);
            if (_collapseIcon) _collapseIcon.gameObject.SetActive(isParent || isActive);
            if (_button) _button.interactable = !isEmpty;
        }

        public IComponentTreeNode Node { get; private set; }
    }
}
