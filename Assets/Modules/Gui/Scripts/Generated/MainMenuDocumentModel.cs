// This code was automatically generated.
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.

using UnityEngine;
using UnityEngine.UIElements;

namespace Gui.Generated
{
    [RequireComponent(typeof(UIDocument))]
    public partial class MainMenuDocumentModel : MonoBehaviour
    {
        private UIDocument _uiDocument;
        private TemplateContainer _PrivacyPolicy;
        private Button _PrivacyPolicy_button;
        private TemplateContainer _NewGame;
        private Button _NewGame_button;
        private Label _NewGame_button_label;
        private TemplateContainer _Continue;
        private Button _Continue_button;
        private Label _Continue_button_label;
        private TemplateContainer _QuickCombat;
        private Button _QuickCombat_button;
        private Label _QuickCombat_button_label;
        private TemplateContainer _Settings;
        private Button _Settings_button;
        private Label _Settings_button_label;
        private TemplateContainer _Constructor;
        private Button _Constructor_button;
        private VisualElement _Constructor_button_icon;
        private TextField _Constructor_button_input;
        private TemplateContainer _RestorePurchases;
        private Button _RestorePurchases_button;
        private Label _RestorePurchases_button_label;
        private TemplateContainer _Exit;
        private Button _Exit_button;
        private Label _Exit_button_label;

        public UIDocument UiDocument => _uiDocument ??= GetComponent<UIDocument>();
        public TemplateContainer PrivacyPolicy => _PrivacyPolicy ??= (TemplateContainer)UiDocument.rootVisualElement[0][0][0];
        public Button PrivacyPolicy_button => _PrivacyPolicy_button ??= (Button)UiDocument.rootVisualElement[0][0][0][0];
        public TemplateContainer NewGame => _NewGame ??= (TemplateContainer)UiDocument.rootVisualElement[0][1];
        public Button NewGame_button => _NewGame_button ??= (Button)UiDocument.rootVisualElement[0][1][0];
        public Label NewGame_button_label => _NewGame_button_label ??= (Label)UiDocument.rootVisualElement[0][1][0][1][0];
        public TemplateContainer Continue => _Continue ??= (TemplateContainer)UiDocument.rootVisualElement[0][2];
        public Button Continue_button => _Continue_button ??= (Button)UiDocument.rootVisualElement[0][2][0];
        public Label Continue_button_label => _Continue_button_label ??= (Label)UiDocument.rootVisualElement[0][2][0][1][0];
        public TemplateContainer QuickCombat => _QuickCombat ??= (TemplateContainer)UiDocument.rootVisualElement[0][3];
        public Button QuickCombat_button => _QuickCombat_button ??= (Button)UiDocument.rootVisualElement[0][3][0];
        public Label QuickCombat_button_label => _QuickCombat_button_label ??= (Label)UiDocument.rootVisualElement[0][3][0][1][0];
        public TemplateContainer Settings => _Settings ??= (TemplateContainer)UiDocument.rootVisualElement[0][4];
        public Button Settings_button => _Settings_button ??= (Button)UiDocument.rootVisualElement[0][4][0];
        public Label Settings_button_label => _Settings_button_label ??= (Label)UiDocument.rootVisualElement[0][4][0][1][0];
        public TemplateContainer Constructor => _Constructor ??= (TemplateContainer)UiDocument.rootVisualElement[0][5];
        public Button Constructor_button => _Constructor_button ??= (Button)UiDocument.rootVisualElement[0][5][0];
        public VisualElement Constructor_button_icon => _Constructor_button_icon ??= (VisualElement)UiDocument.rootVisualElement[0][5][0][0][0];
        public TextField Constructor_button_input => _Constructor_button_input ??= (TextField)UiDocument.rootVisualElement[0][5][0][1][0];
        public TemplateContainer RestorePurchases => _RestorePurchases ??= (TemplateContainer)UiDocument.rootVisualElement[0][6];
        public Button RestorePurchases_button => _RestorePurchases_button ??= (Button)UiDocument.rootVisualElement[0][6][0];
        public Label RestorePurchases_button_label => _RestorePurchases_button_label ??= (Label)UiDocument.rootVisualElement[0][6][0][1][0];
        public TemplateContainer Exit => _Exit ??= (TemplateContainer)UiDocument.rootVisualElement[0][7];
        public Button Exit_button => _Exit_button ??= (Button)UiDocument.rootVisualElement[0][7][0];
        public Label Exit_button_label => _Exit_button_label ??= (Label)UiDocument.rootVisualElement[0][7][0][1][0];
    }
}
