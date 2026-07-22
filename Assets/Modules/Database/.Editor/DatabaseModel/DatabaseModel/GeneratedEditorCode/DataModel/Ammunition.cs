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
	public partial class Ammunition
	{
		partial void OnDataDeserialized(AmmunitionSerializable serializable, Database database);
		partial void OnDataSerialized(ref AmmunitionSerializable serializable);

		public static Ammunition Create(AmmunitionSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new Ammunition(serializable, database);
		}

		private Ammunition(AmmunitionSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<Ammunition>(serializable.Id, serializable.FileName);
				Body.Value = DataModel.BulletBody.Create(serializable.Body, database);
				Controller.Value = DataModel.BulletController.Create(serializable.Controller, database);
				Triggers = serializable.Triggers?.Select(item => BulletTrigger.Create(item, database)).ToArray();
				ImpactType = serializable.ImpactType;
				Effects = serializable.Effects?.Select(item => ImpactEffect.Create(item, database)).ToArray();
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(AmmunitionSerializable serializable)
		{
			serializable.Body = Body.Value?.Serialize();
			serializable.Controller = Controller.Value?.Serialize();
			if (Triggers == null || Triggers.Length == 0)
			    serializable.Triggers = null;
			else
			    serializable.Triggers = Triggers.Select(item => item.Serialize()).ToArray();
			serializable.ImpactType = ImpactType;
			if (Effects == null || Effects.Length == 0)
			    serializable.Effects = null;
			else
			    serializable.Effects = Effects.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<Ammunition> Id;

		public ObjectWrapper<BulletBody> Body = new(DataModel.BulletBody.DefaultValue);
		public ObjectWrapper<BulletController> Controller = new(DataModel.BulletController.DefaultValue);
		public BulletTrigger[] Triggers;
		public BulletImpactType ImpactType;
		public ImpactEffect[] Effects;

		public static Ammunition DefaultValue { get; private set; }
	}
}
