// This code was automatically generated.
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.

using Gui.Presenter;
using UnityEngine.UIElements;

namespace Gui.Presenter.MainMenu
{
    public partial class PrivacyPolicyButtonPresenter : PresenterBase
    {
        private TemplateContainer _PrivacyPolicy;
        private Button _PrivacyPolicy_button;

        public override VisualElement RootElement => PrivacyPolicy;
        public TemplateContainer PrivacyPolicy => _PrivacyPolicy ??= (TemplateContainer)UiDocument.rootVisualElement[1];
        public Button PrivacyPolicy_button => _PrivacyPolicy_button ??= (Button)UiDocument.rootVisualElement[1][0];
    }
}
