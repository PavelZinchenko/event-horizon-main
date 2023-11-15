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
	public partial class SpecialEventSettings
	{
		partial void OnDataDeserialized(SpecialEventSettingsSerializable serializable, Database.Loader loader);

		public static SpecialEventSettings Create(SpecialEventSettingsSerializable serializable, Database.Loader loader)
		{
			return new SpecialEventSettings(serializable, loader);
		}

		private SpecialEventSettings(SpecialEventSettingsSerializable serializable, Database.Loader loader)
		{
			EnableXmasEvent = serializable.EnableXmasEvent;
			XmasDaysBefore = UnityEngine.Mathf.Clamp(serializable.XmasDaysBefore, 0, 30);
			XmasDaysAfter = UnityEngine.Mathf.Clamp(serializable.XmasDaysAfter, 0, 30);
			XmasQuest = loader.GetQuest(new ItemId<QuestModel>(serializable.XmasQuest));
			EnableEasterEvent = serializable.EnableEasterEvent;
			EasterDaysBefore = UnityEngine.Mathf.Clamp(serializable.EasterDaysBefore, 0, 30);
			EasterDaysAfter = UnityEngine.Mathf.Clamp(serializable.EasterDaysAfter, 0, 30);
			EasterQuest = loader.GetQuest(new ItemId<QuestModel>(serializable.EasterQuest));
			EnableHalloweenEvent = serializable.EnableHalloweenEvent;
			HalloweenDaysBefore = UnityEngine.Mathf.Clamp(serializable.HalloweenDaysBefore, 0, 30);
			HalloweenDaysAfter = UnityEngine.Mathf.Clamp(serializable.HalloweenDaysAfter, 0, 30);
			HalloweenQuest = loader.GetQuest(new ItemId<QuestModel>(serializable.HalloweenQuest));

			OnDataDeserialized(serializable, loader);
		}

		public bool EnableXmasEvent { get; private set; }
		public int XmasDaysBefore { get; private set; }
		public int XmasDaysAfter { get; private set; }
		public QuestModel XmasQuest { get; private set; }
		public bool EnableEasterEvent { get; private set; }
		public int EasterDaysBefore { get; private set; }
		public int EasterDaysAfter { get; private set; }
		public QuestModel EasterQuest { get; private set; }
		public bool EnableHalloweenEvent { get; private set; }
		public int HalloweenDaysBefore { get; private set; }
		public int HalloweenDaysAfter { get; private set; }
		public QuestModel HalloweenQuest { get; private set; }

		public static SpecialEventSettings DefaultValue { get; private set; }
	}
}
