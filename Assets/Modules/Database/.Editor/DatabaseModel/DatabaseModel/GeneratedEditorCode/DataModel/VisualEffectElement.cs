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
	public partial class VisualEffectElement
	{
		partial void OnDataDeserialized(VisualEffectElementSerializable serializable, Database database);
		partial void OnDataSerialized(ref VisualEffectElementSerializable serializable);

		public static VisualEffectElement Create(VisualEffectElementSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new VisualEffectElement(serializable, database);
		}

		public VisualEffectElement() {}

		private VisualEffectElement(VisualEffectElementSerializable serializable, Database database)
		{
			Type = serializable.Type;
			Image = serializable.Image;
			ColorMode = serializable.ColorMode;
			Color = Helpers.ColorFromString(serializable.Color);
			Quantity = new NumericValue<int>(serializable.Quantity, 1, 100);
			Size = new NumericValue<float>(serializable.Size, 0.001f, 100f);
			GrowthRate = new NumericValue<float>(serializable.GrowthRate, -1f, 100f);
			TurnRate = new NumericValue<float>(serializable.TurnRate, -1000f, 1000f);
			StartTime = new NumericValue<float>(serializable.StartTime, 0f, 1000f);
			Lifetime = new NumericValue<float>(serializable.Lifetime, 0f, 1000f);
			ParticleSize = new NumericValue<float>(serializable.ParticleSize, 0.001f, 100f);
			Offset = serializable.Offset;
			Rotation = new NumericValue<float>(serializable.Rotation, 0f, 360f);
			Loop = serializable.Loop;
			Inverse = serializable.Inverse;
			UseRealTime = serializable.UseRealTime;
			OnDataDeserialized(serializable, database);
		}

		public VisualEffectElementSerializable Serialize()
		{
			var serializable = new VisualEffectElementSerializable();
			serializable.Type = Type;
			serializable.Image = Image;
			serializable.ColorMode = ColorMode;
			serializable.Color = Helpers.ColorToString(Color);
			serializable.Quantity = Quantity.Value;
			serializable.Size = Size.Value;
			serializable.GrowthRate = GrowthRate.Value;
			serializable.TurnRate = TurnRate.Value;
			serializable.StartTime = StartTime.Value;
			serializable.Lifetime = Lifetime.Value;
			serializable.ParticleSize = ParticleSize.Value;
			serializable.Offset = Offset;
			serializable.Rotation = Rotation.Value;
			serializable.Loop = Loop;
			serializable.Inverse = Inverse;
			serializable.UseRealTime = UseRealTime;
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public VisualEffectType Type;
		public string Image;
		public ColorMode ColorMode;
		public System.Drawing.Color Color;
		public NumericValue<int> Quantity = new NumericValue<int>(0, 1, 100);
		public NumericValue<float> Size = new NumericValue<float>(0, 0.001f, 100f);
		public NumericValue<float> GrowthRate = new NumericValue<float>(0, -1f, 100f);
		public NumericValue<float> TurnRate = new NumericValue<float>(0, -1000f, 1000f);
		public NumericValue<float> StartTime = new NumericValue<float>(0, 0f, 1000f);
		public NumericValue<float> Lifetime = new NumericValue<float>(0, 0f, 1000f);
		public NumericValue<float> ParticleSize = new NumericValue<float>(0, 0.001f, 100f);
		public Vector2 Offset;
		public NumericValue<float> Rotation = new NumericValue<float>(0, 0f, 360f);
		public bool Loop;
		public bool Inverse;
		public bool UseRealTime;

		public static VisualEffectElement DefaultValue { get; private set; }
	}
}
