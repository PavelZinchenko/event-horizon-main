


//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using GameDatabase.Enums;
using GameDatabase.Serializable;
using GameDatabase.Model;

namespace GameDatabase.DataModel
{
	public partial class UiSettings 
	{
		partial void OnDataDeserialized(UiSettingsSerializable serializable, Database.Loader loader);

		public static UiSettings Create(UiSettingsSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new UiSettings(serializable, loader);
		}

		private UiSettings(UiSettingsSerializable serializable, Database.Loader loader)
		{
			WindowColor = new ColorData(serializable.WindowColor);
			ScrollBarColor = new ColorData(serializable.ScrollBarColor);
			IconColor = new ColorData(serializable.IconColor);
			SelectionColor = new ColorData(serializable.SelectionColor);
			ButtonColor = new ColorData(serializable.ButtonColor);
			ButtonFocusColor = new ColorData(serializable.ButtonFocusColor);
			ButtonTextColor = new ColorData(serializable.ButtonTextColor);
			ButtonIconColor = new ColorData(serializable.ButtonIconColor);
			WarningButtonColor = new ColorData(serializable.WarningButtonColor);
			WarningButtonFocusColor = new ColorData(serializable.WarningButtonFocusColor);
			WarningButtonTextColor = new ColorData(serializable.WarningButtonTextColor);
			WarningButtonIconColor = new ColorData(serializable.WarningButtonIconColor);
			TextColor = new ColorData(serializable.TextColor);
			ErrorTextColor = new ColorData(serializable.ErrorTextColor);
			HeaderTextColor = new ColorData(serializable.HeaderTextColor);
			PaleTextColor = new ColorData(serializable.PaleTextColor);
			BrightTextColor = new ColorData(serializable.BrightTextColor);
			LowQualityItemColor = new ColorData(serializable.LowQualityItemColor);
			CommonQualityItemColor = new ColorData(serializable.CommonQualityItemColor);
			MediumQualityItemColor = new ColorData(serializable.MediumQualityItemColor);
			HighQualityItemColor = new ColorData(serializable.HighQualityItemColor);
			PerfectQualityItemColor = new ColorData(serializable.PerfectQualityItemColor);

			OnDataDeserialized(serializable, loader);
		}

		public ColorData WindowColor { get; private set; }
		public ColorData ScrollBarColor { get; private set; }
		public ColorData IconColor { get; private set; }
		public ColorData SelectionColor { get; private set; }
		public ColorData ButtonColor { get; private set; }
		public ColorData ButtonFocusColor { get; private set; }
		public ColorData ButtonTextColor { get; private set; }
		public ColorData ButtonIconColor { get; private set; }
		public ColorData WarningButtonColor { get; private set; }
		public ColorData WarningButtonFocusColor { get; private set; }
		public ColorData WarningButtonTextColor { get; private set; }
		public ColorData WarningButtonIconColor { get; private set; }
		public ColorData TextColor { get; private set; }
		public ColorData ErrorTextColor { get; private set; }
		public ColorData HeaderTextColor { get; private set; }
		public ColorData PaleTextColor { get; private set; }
		public ColorData BrightTextColor { get; private set; }
		public ColorData LowQualityItemColor { get; private set; }
		public ColorData CommonQualityItemColor { get; private set; }
		public ColorData MediumQualityItemColor { get; private set; }
		public ColorData HighQualityItemColor { get; private set; }
		public ColorData PerfectQualityItemColor { get; private set; }

		public static UiSettings DefaultValue { get; private set; }
	}
}
