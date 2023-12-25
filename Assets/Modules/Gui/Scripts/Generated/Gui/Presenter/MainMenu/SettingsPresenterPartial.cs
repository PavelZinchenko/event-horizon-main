// This code was automatically generated.
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.

using Gui.Presenter;
using UnityEngine.UIElements;

namespace Gui.Presenter.MainMenu
{
    public partial class SettingsPresenter : PresenterBase
    {
        private TemplateContainer _Settings;
        private VisualElement _Settings_Content;
        private TemplateContainer _Settings_Content_SettingsGeneral;
        private VisualElement _Settings_Content_SettingsGeneral_General;
        private DropdownField _Settings_Content_SettingsGeneral_General_Languages;
        private MySlider _Settings_Content_SettingsGeneral_General_MusicVolume;
        private MySlider _Settings_Content_SettingsGeneral_General_SoundVolume;
        private MyIntSlider _Settings_Content_SettingsGeneral_General_GraphicsQuality;
        private Toggle _Settings_Content_SettingsGeneral_General_FullScreenMode;
        private Toggle _Settings_Content_SettingsGeneral_General_RunInBackground;
        private GroupBox _Settings_Buttons;
        private TemplateContainer _Settings_Buttons_General;
        private RadioButton _Settings_Buttons_General_Tab;
        private TemplateContainer _Settings_Buttons_Combat;
        private RadioButton _Settings_Buttons_Combat_Tab;
        private TemplateContainer _Settings_Buttons_Controls;
        private RadioButton _Settings_Buttons_Controls_Tab;
        private TemplateContainer _Settings_Buttons_Account;
        private RadioButton _Settings_Buttons_Account_Tab;
        private TemplateContainer _Settings_Buttons_LoadSave;
        private RadioButton _Settings_Buttons_LoadSave_Tab;
        private TemplateContainer _Settings_Buttons_Database;
        private RadioButton _Settings_Buttons_Database_Tab;

        public override VisualElement RootElement => Settings;
        public TemplateContainer Settings => _Settings ??= (TemplateContainer)UiDocument.rootVisualElement[4];
        public VisualElement Settings_Content => _Settings_Content ??= (VisualElement)UiDocument.rootVisualElement[4][0][0];
        public TemplateContainer Settings_Content_SettingsGeneral => _Settings_Content_SettingsGeneral ??= (TemplateContainer)UiDocument.rootVisualElement[4][0][0][0];
        public VisualElement Settings_Content_SettingsGeneral_General => _Settings_Content_SettingsGeneral_General ??= (VisualElement)UiDocument.rootVisualElement[4][0][0][0][0];
        public DropdownField Settings_Content_SettingsGeneral_General_Languages => _Settings_Content_SettingsGeneral_General_Languages ??= (DropdownField)UiDocument.rootVisualElement[4][0][0][0][0][0];
        public MySlider Settings_Content_SettingsGeneral_General_MusicVolume => _Settings_Content_SettingsGeneral_General_MusicVolume ??= (MySlider)UiDocument.rootVisualElement[4][0][0][0][0][1];
        public MySlider Settings_Content_SettingsGeneral_General_SoundVolume => _Settings_Content_SettingsGeneral_General_SoundVolume ??= (MySlider)UiDocument.rootVisualElement[4][0][0][0][0][2];
        public MyIntSlider Settings_Content_SettingsGeneral_General_GraphicsQuality => _Settings_Content_SettingsGeneral_General_GraphicsQuality ??= (MyIntSlider)UiDocument.rootVisualElement[4][0][0][0][0][3];
        public Toggle Settings_Content_SettingsGeneral_General_FullScreenMode => _Settings_Content_SettingsGeneral_General_FullScreenMode ??= (Toggle)UiDocument.rootVisualElement[4][0][0][0][0][4];
        public Toggle Settings_Content_SettingsGeneral_General_RunInBackground => _Settings_Content_SettingsGeneral_General_RunInBackground ??= (Toggle)UiDocument.rootVisualElement[4][0][0][0][0][5];
        public GroupBox Settings_Buttons => _Settings_Buttons ??= (GroupBox)UiDocument.rootVisualElement[4][0][1];
        public TemplateContainer Settings_Buttons_General => _Settings_Buttons_General ??= (TemplateContainer)UiDocument.rootVisualElement[4][0][1][0];
        public RadioButton Settings_Buttons_General_Tab => _Settings_Buttons_General_Tab ??= (RadioButton)UiDocument.rootVisualElement[4][0][1][0][0];
        public TemplateContainer Settings_Buttons_Combat => _Settings_Buttons_Combat ??= (TemplateContainer)UiDocument.rootVisualElement[4][0][1][1];
        public RadioButton Settings_Buttons_Combat_Tab => _Settings_Buttons_Combat_Tab ??= (RadioButton)UiDocument.rootVisualElement[4][0][1][1][0];
        public TemplateContainer Settings_Buttons_Controls => _Settings_Buttons_Controls ??= (TemplateContainer)UiDocument.rootVisualElement[4][0][1][2];
        public RadioButton Settings_Buttons_Controls_Tab => _Settings_Buttons_Controls_Tab ??= (RadioButton)UiDocument.rootVisualElement[4][0][1][2][0];
        public TemplateContainer Settings_Buttons_Account => _Settings_Buttons_Account ??= (TemplateContainer)UiDocument.rootVisualElement[4][0][1][3];
        public RadioButton Settings_Buttons_Account_Tab => _Settings_Buttons_Account_Tab ??= (RadioButton)UiDocument.rootVisualElement[4][0][1][3][0];
        public TemplateContainer Settings_Buttons_LoadSave => _Settings_Buttons_LoadSave ??= (TemplateContainer)UiDocument.rootVisualElement[4][0][1][4];
        public RadioButton Settings_Buttons_LoadSave_Tab => _Settings_Buttons_LoadSave_Tab ??= (RadioButton)UiDocument.rootVisualElement[4][0][1][4][0];
        public TemplateContainer Settings_Buttons_Database => _Settings_Buttons_Database ??= (TemplateContainer)UiDocument.rootVisualElement[4][0][1][5];
        public RadioButton Settings_Buttons_Database_Tab => _Settings_Buttons_Database_Tab ??= (RadioButton)UiDocument.rootVisualElement[4][0][1][5][0];
    }
}
