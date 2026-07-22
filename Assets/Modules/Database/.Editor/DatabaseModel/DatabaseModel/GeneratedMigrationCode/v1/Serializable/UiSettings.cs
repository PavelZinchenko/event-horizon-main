//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using EditorDatabase.Model;
using DatabaseMigration.v1.Enums;

namespace DatabaseMigration.v1.Serializable
{
	[Serializable]
	public class UiSettingsSerializable : SerializableItem
	{
		public UiSettingsSerializable()
		{
			ItemType = ItemType.UiSettings;
			FileName = "UiSettings.json";
		}

		[DefaultValue("#50C0FF")]
		public string WindowColor = "#50C0FF";
		[DefaultValue("#C050C0FF")]
		public string ScrollBarColor = "#C050C0FF";
		[DefaultValue("#80FFFF")]
		public string IconColor = "#80FFFF";
		[DefaultValue("#80FFFF")]
		public string SelectionColor = "#80FFFF";
		[DefaultValue("#50C0FF")]
		public string ButtonColor = "#50C0FF";
		[DefaultValue("#4050C0FF")]
		public string ButtonFocusColor = "#4050C0FF";
		[DefaultValue("#80FFFF")]
		public string ButtonTextColor = "#80FFFF";
		[DefaultValue("#E080FFFF")]
		public string ButtonIconColor = "#E080FFFF";
		[DefaultValue("#FF8050")]
		public string WarningButtonColor = "#FF8050";
		[DefaultValue("#20FF8050")]
		public string WarningButtonFocusColor = "#20FF8050";
		[DefaultValue("#FFFFC0")]
		public string WarningButtonTextColor = "#FFFFC0";
		[DefaultValue("#FFFFC0")]
		public string WarningButtonIconColor = "#FFFFC0";
		[DefaultValue("#FFFFC0")]
		public string PremiumButtonColor = "#FFFFC0";
		[DefaultValue("#40FFFFC0")]
		public string PremiumButtonFocusColor = "#40FFFFC0";
		[DefaultValue("#FFFFE0")]
		public string PremiumButtonTextColor = "#FFFFE0";
		[DefaultValue("#FFFFC0")]
		public string PremiumButtonIconColor = "#FFFFC0";
		[DefaultValue("#80FFFF")]
		public string TextColor = "#80FFFF";
		[DefaultValue("#FF4040")]
		public string ErrorTextColor = "#FF4040";
		[DefaultValue("#FFFFC0")]
		public string HeaderTextColor = "#FFFFC0";
		[DefaultValue("#A0FFFFFF")]
		public string PaleTextColor = "#A0FFFFFF";
		[DefaultValue("#FFFFFF")]
		public string BrightTextColor = "#FFFFFF";
		[DefaultValue("#000000")]
		public string BackgroundDark = "#000000";
		[DefaultValue("#C0C0C0")]
		public string LowQualityItemColor = "#C0C0C0";
		[DefaultValue("#80FFFF")]
		public string CommonQualityItemColor = "#80FFFF";
		[DefaultValue("#80FF80")]
		public string MediumQualityItemColor = "#80FF80";
		[DefaultValue("#F09FFF")]
		public string HighQualityItemColor = "#F09FFF";
		[DefaultValue("#FFDF51")]
		public string PerfectQualityItemColor = "#FFDF51";
		[DefaultValue("#FFFFC0")]
		public string AvailableTechColor = "#FFFFC0";
		[DefaultValue("#808080")]
		public string UnavailableTechColor = "#808080";
		[DefaultValue("#50C0FF")]
		public string ObtainedTechColor = "#50C0FF";
		[DefaultValue("#8080FF")]
		public string HiddenTechColor = "#8080FF";
		[DefaultValue("#00FF00")]
		public string CreditsColor = "#00FF00";
		[DefaultValue("#FFF0A0")]
		public string StarsColor = "#FFF0A0";
		[DefaultValue("#FFF0A0")]
		public string MoneyColor = "#FFF0A0";
		[DefaultValue("#00FFFF")]
		public string FuelColor = "#00FFFF";
		[DefaultValue("#8080FF")]
		public string TokensColor = "#8080FF";
		[DefaultValue("")]
		public string MainMenuBackgroundImage;
		public bool NoCreditsText;
	}
}
