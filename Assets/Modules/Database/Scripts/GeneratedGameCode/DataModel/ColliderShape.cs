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
	public partial class ColliderShape 
	{
		partial void OnDataDeserialized(ColliderShapeSerializable serializable, Database.Loader loader);

		public static ColliderShape Create(ColliderShapeSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new ColliderShape(serializable, loader);
		}

		private ColliderShape(ColliderShapeSerializable serializable, Database.Loader loader)
		{
			Points = new ImmutableCollection<GameDatabase.Model.Vector2Data>(serializable.Points);
			Radius = Utilites.MathEx.Clamp(serializable.Radius, 0f, 100f);

			OnDataDeserialized(serializable, loader);
		}

		public ImmutableCollection<GameDatabase.Model.Vector2Data> Points { get; private set; }
		public float Radius { get; private set; }

		public static ColliderShape DefaultValue { get; private set; }= new(new(), null);
	}
}
