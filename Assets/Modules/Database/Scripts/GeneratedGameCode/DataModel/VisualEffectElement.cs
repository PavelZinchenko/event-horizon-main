


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
			Quantity = UnityEngine.Mathf.Clamp(serializable.Quantity, 1, 100);
			Size = UnityEngine.Mathf.Clamp(serializable.Size, 0.001f, 100f);
			GrowthRate = UnityEngine.Mathf.Clamp(serializable.GrowthRate, -1f, 100f);
			TurnRate = UnityEngine.Mathf.Clamp(serializable.TurnRate, -1000f, 1000f);
			StartTime = UnityEngine.Mathf.Clamp(serializable.StartTime, 0f, 1000f);
			Lifetime = UnityEngine.Mathf.Clamp(serializable.Lifetime, 0f, 1000f);
			ParticleSize = UnityEngine.Mathf.Clamp(serializable.ParticleSize, 0.001f, 100f);
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
		public bool Loop { get; private set; }
		public bool Inverse { get; private set; }
		public bool UseRealTime { get; private set; }

		public static VisualEffectElement DefaultValue { get; private set; }= new(new(), null);
	}
}
