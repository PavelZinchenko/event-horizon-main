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
	public partial class VisualEffectElement 
	{
		partial void OnDataDeserialized(VisualEffectElementSerializable serializable, Database.Loader loader);

		public static VisualEffectElement Create(VisualEffectElementSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new VisualEffectElement(serializable, loader);
		}

		private VisualEffectElement(VisualEffectElementSerializable serializable, Database.Loader loader)
		{
			Type = serializable.Type;
			Image = new SpriteId(serializable.Image, SpriteId.Type.Effect);
			ColorMode = serializable.ColorMode;
			Color = new ColorData(serializable.Color);
			Quantity = Utilites.MathEx.Clamp(serializable.Quantity, 1, 100);
			Size = Utilites.MathEx.Clamp(serializable.Size, 0.001f, 100f);
			GrowthRate = Utilites.MathEx.Clamp(serializable.GrowthRate, -1f, 100f);
			TurnRate = Utilites.MathEx.Clamp(serializable.TurnRate, -1000f, 1000f);
			StartTime = Utilites.MathEx.Clamp(serializable.StartTime, 0f, 1000f);
			Lifetime = Utilites.MathEx.Clamp(serializable.Lifetime, 0f, 1000f);
			ParticleSize = Utilites.MathEx.Clamp(serializable.ParticleSize, 0.001f, 100f);
			Offset = serializable.Offset;
			Rotation = Utilites.MathEx.Clamp(serializable.Rotation, 0f, 360f);
			Loop = serializable.Loop;
			Inverse = serializable.Inverse;
			UseRealTime = serializable.UseRealTime;

			OnDataDeserialized(serializable, loader);
		}

		public VisualEffectType Type { get; private set; }
		public SpriteId Image { get; private set; }
		public ColorMode ColorMode { get; private set; }
		public ColorData Color { get; private set; }
		public int Quantity { get; private set; }
		public float Size { get; private set; }
		public float GrowthRate { get; private set; }
		public float TurnRate { get; private set; }
		public float StartTime { get; private set; }
		public float Lifetime { get; private set; }
		public float ParticleSize { get; private set; }
		public GameDatabase.Model.Vector2Data Offset { get; private set; }
		public float Rotation { get; private set; }
		public bool Loop { get; private set; }
		public bool Inverse { get; private set; }
		public bool UseRealTime { get; private set; }

		public static VisualEffectElement DefaultValue { get; private set; }= new(new(), null);
	}
}
