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
	public partial class ComponentMod
	{
		partial void OnDataDeserialized(ComponentModSerializable serializable, Database.Loader loader);

		public static ComponentMod Create(ComponentModSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new ComponentMod(serializable, loader);
		}

		private ComponentMod(ComponentModSerializable serializable, Database.Loader loader)
		{
			Id = new ItemId<ComponentMod>(serializable.Id);
			loader.AddComponentMod(serializable.Id, this);

			Description = serializable.Description;
			Modifications = new ImmutableCollection<StatModification>(serializable.Modifications?.Select(item => StatModification.Create(item, loader)));

			OnDataDeserialized(serializable, loader);
		}

		public readonly ItemId<ComponentMod> Id;

		public string Description { get; private set; }
		public ImmutableCollection<StatModification> Modifications { get; private set; }

		public static ComponentMod DefaultValue { get; private set; }
	}
}
