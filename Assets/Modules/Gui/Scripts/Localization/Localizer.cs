using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using Services.Localization;

namespace Gui.Localization
{
    [RequireComponent(typeof(UIDocument))]
    public class Localizer : MonoBehaviour
    {
        [Inject] private readonly ILocalization _localization;

        private UIDocument _uIDocument;

        private void Awake()
        {
            _uIDocument = GetComponent<UIDocument>();
            _uIDocument.rootVisualElement.Query<TextElement>().ForEach(Localize);
        }

        private void Localize(TextElement label)
        {
            label.text = _localization.GetString(label.text);
        }
    }
}
