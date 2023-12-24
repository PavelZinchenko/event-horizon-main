using System.Linq;
using UnityEngine.UIElements;
using Zenject;
using Services.Localization;

namespace Gui.Presenter.MainMenu
{
	public partial class SettingsPresenter : PresenterBase
	{
		[Inject] private readonly ILocalization _localization;

		private void Start()
		{
			Settings_Buttons_General_Tab.SetSelected(true);

			var languages = Settings_Content_SettingsGeneral_General_Languages;
			var localizations = _localization.LoadLocalizationList();

			languages.choices = localizations.Select(item => item.name).ToList();
		}

		private void OnEnable()
		{
			Settings_Buttons_General_Tab.RegisterValueChangedCallback(OnGeneralTabValueChanged);
			Settings_Buttons_Account_Tab.RegisterValueChangedCallback(OnAccountTabValueChanged);
			Settings_Buttons_Combat_Tab.RegisterValueChangedCallback(OnCombatTabValueChanged);
			Settings_Buttons_Controls_Tab.RegisterValueChangedCallback(OnControlsTabValueChanged);
			Settings_Buttons_Database_Tab.RegisterValueChangedCallback(OnDatabaseTabValueChanged);
			Settings_Buttons_LoadSave_Tab.RegisterValueChangedCallback(OnLoadSaveTabValueChanged);

			Settings_Content_SettingsGeneral_General_Languages.RegisterValueChangedCallback(OnLocalizationChanged);
		}

		private void OnDisable()
		{
			Settings_Buttons_General_Tab.UnregisterValueChangedCallback(OnGeneralTabValueChanged);
			Settings_Buttons_Account_Tab.UnregisterValueChangedCallback(OnAccountTabValueChanged);
			Settings_Buttons_Combat_Tab.UnregisterValueChangedCallback(OnCombatTabValueChanged);
			Settings_Buttons_Controls_Tab.UnregisterValueChangedCallback(OnControlsTabValueChanged);
			Settings_Buttons_Database_Tab.UnregisterValueChangedCallback(OnDatabaseTabValueChanged);
			Settings_Buttons_LoadSave_Tab.UnregisterValueChangedCallback(OnLoadSaveTabValueChanged);

			Settings_Content_SettingsGeneral_General_Languages.UnregisterValueChangedCallback(OnLocalizationChanged);
		}

		private void OnGeneralTabValueChanged(ChangeEvent<bool> e)
		{
		}

		private void OnAccountTabValueChanged(ChangeEvent<bool> e)
		{
		}

		private void OnCombatTabValueChanged(ChangeEvent<bool> e)
		{
		}

		private void OnControlsTabValueChanged(ChangeEvent<bool> e)
		{
		}

		private void OnDatabaseTabValueChanged(ChangeEvent<bool> e)
		{
		}

		private void OnLoadSaveTabValueChanged(ChangeEvent<bool> e)
		{
		}

		private void OnLocalizationChanged(ChangeEvent<string> e)
		{
			UnityEngine.Debug.LogError(e.newValue);
		}
	}
}
