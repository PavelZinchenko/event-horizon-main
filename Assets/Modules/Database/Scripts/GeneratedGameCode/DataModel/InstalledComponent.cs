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
	public partial class InstalledComponent 
	{
		partial void OnDataDeserialized(InstalledComponentSerializable serializable, Database.Loader loader);

		public static InstalledComponent Create(InstalledComponentSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new InstalledComponent(serializable, loader);
		}

		private InstalledComponent(InstalledComponentSerializable serializable, Database.Loader loader)
		{
			Component = loader?.GetComponent(new ItemId<Component>(serializable.ComponentId)) ?? Component.DefaultValue;
			if (loader != null && Component == null)
			    throw new DatabaseException("ObjectTemplate.Component cannot be null - " + serializable.ComponentId);
			Modification = loader?.GetComponentMod(new ItemId<ComponentMod>(serializable.Modification)) ?? ComponentMod.DefaultValue;
			Quality = serializable.Quality;
			X = UnityEngine.Mathf.Clamp(serializable.X, -32768, 32767);
			Y = UnityEngine.Mathf.Clamp(serializable.Y, -32768, 32767);
			BarrelId = UnityEngine.Mathf.Clamp(serializable.BarrelId, 0, 255);
			Behaviour = UnityEngine.Mathf.Clamp(serializable.Behaviour, 0, 10);
			KeyBinding = UnityEngine.Mathf.Clamp(serializable.KeyBinding, -10, 10);

			OnDataDeserialized(serializable, loader);
		}

		public Component Component { get; private set; }
		public ComponentMod Modification { get; private set; }
		public ModificationQuality Quality { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }
		public int BarrelId { get; private set; }
		public int Behaviour { get; private set; }
		public int KeyBinding { get; private set; }

		public static InstalledComponent DefaultValue { get; private set; }= new(new(), null);
	}
}
