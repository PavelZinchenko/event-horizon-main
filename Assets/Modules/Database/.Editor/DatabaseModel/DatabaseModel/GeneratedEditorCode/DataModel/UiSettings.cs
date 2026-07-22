//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using EditorDatabase.Enums;
using EditorDatabase.Serializable;
using EditorDatabase.Model;

namespace EditorDatabase.DataModel
{
	public partial class UiSettings
	{
		partial void OnDataDeserialized(UiSettingsSerializable serializable, Database database);
		partial void OnDataSerialized(ref UiSettingsSerializable serializable);

		public static UiSettings Create(UiSettingsSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new UiSettings(serializable, database);
		}

		private UiSettings(UiSettingsSerializable serializable, Database database)
		{
			WindowColor = Helpers.ColorFromString(serializable.WindowColor);
			ScrollBarColor = Helpers.ColorFromString(serializable.ScrollBarColor);
			IconColor = Helpers.ColorFromString(serializable.IconColor);
			SelectionColor = Helpers.ColorFromString(serializable.SelectionColor);
			ButtonColor = Helpers.ColorFromString(serializable.ButtonColor);
			ButtonFocusColor = Helpers.ColorFromString(serializable.ButtonFocusColor);
			ButtonTextColor = Helpers.ColorFromString(serializable.ButtonTextColor);
			ButtonIconColor = Helpers.ColorFromString(serializable.ButtonIconColor);
			WarningButtonColor = Helpers.ColorFromString(serializable.WarningButtonColor);
			WarningButtonFocusColor = Helpers.ColorFromString(serializable.WarningButtonFocusColor);
			WarningButtonTextColor = Helpers.ColorFromString(serializable.WarningButtonTextColor);
			WarningButtonIconColor = Helpers.ColorFromString(serializable.WarningButtonIconColor);
			PremiumButtonColor = Helpers.ColorFromString(serializable.PremiumButtonColor);
			PremiumButtonFocusColor = Helpers.ColorFromString(serializable.PremiumButtonFocusColor);
			PremiumButtonTextColor = Helpers.ColorFromString(serializable.PremiumButtonTextColor);
			PremiumButtonIconColor = Helpers.ColorFromString(serializable.PremiumButtonIconColor);
			TextColor = Helpers.ColorFromString(serializable.TextColor);
			ErrorTextColor = Helpers.ColorFromString(serializable.ErrorTextColor);
			HeaderTextColor = Helpers.ColorFromString(serializable.HeaderTextColor);
			PaleTextColor = Helpers.ColorFromString(serializable.PaleTextColor);
			BrightTextColor = Helpers.ColorFromString(serializable.BrightTextColor);
			BackgroundDark = Helpers.ColorFromString(serializable.BackgroundDark);
			LowQualityItemColor = Helpers.ColorFromString(serializable.LowQualityItemColor);
			CommonQualityItemColor = Helpers.ColorFromString(serializable.CommonQualityItemColor);
			MediumQualityItemColor = Helpers.ColorFromString(serializable.MediumQualityItemColor);
			HighQualityItemColor = Helpers.ColorFromString(serializable.HighQualityItemColor);
			PerfectQualityItemColor = Helpers.ColorFromString(serializable.PerfectQualityItemColor);
			AvailableTechColor = Helpers.ColorFromString(serializable.AvailableTechColor);
			UnavailableTechColor = Helpers.ColorFromString(serializable.UnavailableTechColor);
			ObtainedTechColor = Helpers.ColorFromString(serializable.ObtainedTechColor);
			HiddenTechColor = Helpers.ColorFromString(serializable.HiddenTechColor);
			CreditsColor = Helpers.ColorFromString(serializable.CreditsColor);
			StarsColor = Helpers.ColorFromString(serializable.StarsColor);
			MoneyColor = Helpers.ColorFromString(serializable.MoneyColor);
			FuelColor = Helpers.ColorFromString(serializable.FuelColor);
			TokensColor = Helpers.ColorFromString(serializable.TokensColor);
			MainMenuBackgroundImage = serializable.MainMenuBackgroundImage;
			NoCreditsText = serializable.NoCreditsText;
			OnDataDeserialized(serializable, database);
		}

		public void Save(UiSettingsSerializable serializable)
		{
			serializable.WindowColor = Helpers.ColorToString(WindowColor);
			serializable.ScrollBarColor = Helpers.ColorToString(ScrollBarColor);
			serializable.IconColor = Helpers.ColorToString(IconColor);
			serializable.SelectionColor = Helpers.ColorToString(SelectionColor);
			serializable.ButtonColor = Helpers.ColorToString(ButtonColor);
			serializable.ButtonFocusColor = Helpers.ColorToString(ButtonFocusColor);
			serializable.ButtonTextColor = Helpers.ColorToString(ButtonTextColor);
			serializable.ButtonIconColor = Helpers.ColorToString(ButtonIconColor);
			serializable.WarningButtonColor = Helpers.ColorToString(WarningButtonColor);
			serializable.WarningButtonFocusColor = Helpers.ColorToString(WarningButtonFocusColor);
			serializable.WarningButtonTextColor = Helpers.ColorToString(WarningButtonTextColor);
			serializable.WarningButtonIconColor = Helpers.ColorToString(WarningButtonIconColor);
			serializable.PremiumButtonColor = Helpers.ColorToString(PremiumButtonColor);
			serializable.PremiumButtonFocusColor = Helpers.ColorToString(PremiumButtonFocusColor);
			serializable.PremiumButtonTextColor = Helpers.ColorToString(PremiumButtonTextColor);
			serializable.PremiumButtonIconColor = Helpers.ColorToString(PremiumButtonIconColor);
			serializable.TextColor = Helpers.ColorToString(TextColor);
			serializable.ErrorTextColor = Helpers.ColorToString(ErrorTextColor);
			serializable.HeaderTextColor = Helpers.ColorToString(HeaderTextColor);
			serializable.PaleTextColor = Helpers.ColorToString(PaleTextColor);
			serializable.BrightTextColor = Helpers.ColorToString(BrightTextColor);
			serializable.BackgroundDark = Helpers.ColorToString(BackgroundDark);
			serializable.LowQualityItemColor = Helpers.ColorToString(LowQualityItemColor);
			serializable.CommonQualityItemColor = Helpers.ColorToString(CommonQualityItemColor);
			serializable.MediumQualityItemColor = Helpers.ColorToString(MediumQualityItemColor);
			serializable.HighQualityItemColor = Helpers.ColorToString(HighQualityItemColor);
			serializable.PerfectQualityItemColor = Helpers.ColorToString(PerfectQualityItemColor);
			serializable.AvailableTechColor = Helpers.ColorToString(AvailableTechColor);
			serializable.UnavailableTechColor = Helpers.ColorToString(UnavailableTechColor);
			serializable.ObtainedTechColor = Helpers.ColorToString(ObtainedTechColor);
			serializable.HiddenTechColor = Helpers.ColorToString(HiddenTechColor);
			serializable.CreditsColor = Helpers.ColorToString(CreditsColor);
			serializable.StarsColor = Helpers.ColorToString(StarsColor);
			serializable.MoneyColor = Helpers.ColorToString(MoneyColor);
			serializable.FuelColor = Helpers.ColorToString(FuelColor);
			serializable.TokensColor = Helpers.ColorToString(TokensColor);
			serializable.MainMenuBackgroundImage = MainMenuBackgroundImage;
			serializable.NoCreditsText = NoCreditsText;
			OnDataSerialized(ref serializable);
		}

		public System.Drawing.Color WindowColor;
		public System.Drawing.Color ScrollBarColor;
		public System.Drawing.Color IconColor;
		public System.Drawing.Color SelectionColor;
		public System.Drawing.Color ButtonColor;
		public System.Drawing.Color ButtonFocusColor;
		public System.Drawing.Color ButtonTextColor;
		public System.Drawing.Color ButtonIconColor;
		public System.Drawing.Color WarningButtonColor;
		public System.Drawing.Color WarningButtonFocusColor;
		public System.Drawing.Color WarningButtonTextColor;
		public System.Drawing.Color WarningButtonIconColor;
		public System.Drawing.Color PremiumButtonColor;
		public System.Drawing.Color PremiumButtonFocusColor;
		public System.Drawing.Color PremiumButtonTextColor;
		public System.Drawing.Color PremiumButtonIconColor;
		public System.Drawing.Color TextColor;
		public System.Drawing.Color ErrorTextColor;
		public System.Drawing.Color HeaderTextColor;
		public System.Drawing.Color PaleTextColor;
		public System.Drawing.Color BrightTextColor;
		public System.Drawing.Color BackgroundDark;
		public System.Drawing.Color LowQualityItemColor;
		public System.Drawing.Color CommonQualityItemColor;
		public System.Drawing.Color MediumQualityItemColor;
		public System.Drawing.Color HighQualityItemColor;
		public System.Drawing.Color PerfectQualityItemColor;
		public System.Drawing.Color AvailableTechColor;
		public System.Drawing.Color UnavailableTechColor;
		public System.Drawing.Color ObtainedTechColor;
		public System.Drawing.Color HiddenTechColor;
		public System.Drawing.Color CreditsColor;
		public System.Drawing.Color StarsColor;
		public System.Drawing.Color MoneyColor;
		public System.Drawing.Color FuelColor;
		public System.Drawing.Color TokensColor;
		public string MainMenuBackgroundImage;
		public bool NoCreditsText;

		public static UiSettings DefaultValue { get; private set; }
	}
}
