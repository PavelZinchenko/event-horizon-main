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
using CodeWriter.ExpressionParser;

namespace GameDatabase.DataModel
{
	public partial class Satellite : IDefaultVariablesResolver
	{
		partial void OnDataDeserialized(SatelliteSerializable serializable, Database.Loader loader);

		public static Satellite Create(SatelliteSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new Satellite(serializable, loader);
		}

		private Satellite(SatelliteSerializable serializable, Database.Loader loader)
		{
			Id = new ItemId<Satellite>(serializable.Id);
			loader.AddSatellite(serializable.Id, this);

			var variableResolver = new VariableResolver(this);
			Name = serializable.Name;
			ModelImage = new SpriteId(serializable.ModelImage, SpriteId.Type.Satellite);
			ModelScale = Utilites.MathEx.Clamp(serializable.ModelScale, 0.1f, 100f);
			SizeClass = serializable.SizeClass;
			Layout = new Layout(serializable.Layout);
			Barrels = new ImmutableCollection<Barrel>(serializable.Barrels?.Select(item => Barrel.Create(item, loader)));
			Steadiness = Utilites.MathEx.Clamp(serializable.Steadiness, 0.1f, 10f);
			_hitPoints = new Expressions.FloatToFloat(serializable.HitPoints, 0, 2147483647, variableResolver) { ParamName1 = "ship_hp" };
			HitPoints = _hitPoints.Evaluate;
			RespawnTime = Utilites.MathEx.Clamp(serializable.RespawnTime, 0, 2147483647);

			OnDataDeserialized(serializable, loader);
		}

		public readonly ItemId<Satellite> Id;

		public string Name { get; private set; }
		public SpriteId ModelImage { get; private set; }
		public float ModelScale { get; private set; }
		public SizeClass SizeClass { get; private set; }
		public Layout Layout { get; private set; }
		public ImmutableCollection<Barrel> Barrels { get; private set; }
		public float Steadiness { get; private set; }
		private readonly Expressions.FloatToFloat _hitPoints;
		public delegate float HitPointsDelegate(float ship_hp);
		public HitPointsDelegate HitPoints { get; private set; }
		public int RespawnTime { get; private set; }

		public static Satellite DefaultValue { get; private set; }

		private class VariableResolver : IVariableResolver
		{
			private Satellite _context;

			public VariableResolver(Satellite context)
			{
				_context = context;
			}

			public IFunction<Variant> ResolveFunction(string name)
            {
				if (name == "HitPoints") return _context._hitPoints;
				return ((IVariableResolver)_context).ResolveFunction(name);
			}

			public Expression<Variant> ResolveVariable(string name)
			{
				if (name == "ModelScale") return GetModelScale;
				if (name == "Steadiness") return GetSteadiness;
				if (name == "RespawnTime") return GetRespawnTime;
				return ((IVariableResolver)_context).ResolveVariable(name);
			}

			private Variant GetModelScale() => _context.ModelScale;
			private Variant GetSteadiness() => _context.Steadiness;
			private Variant GetRespawnTime() => _context.RespawnTime;
		}
	}
}
