


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
	public abstract partial class BulletTrigger : IDefaultVariablesResolver
	{
		partial void OnDataDeserialized(BulletTriggerSerializable serializable, Database.Loader loader);

		public static BulletTrigger Create(BulletTriggerSerializable serializable, Database.Loader loader)
		{
			if (serializable == null) return DefaultValue;

			switch (serializable.EffectType)
		    {
				case BulletEffectType.None:
					return new BulletTrigger_None(serializable, loader);
				case BulletEffectType.PlaySfx:
					return new BulletTrigger_PlaySfx(serializable, loader);
				case BulletEffectType.SpawnBullet:
					return new BulletTrigger_SpawnBullet(serializable, loader);
				case BulletEffectType.Detonate:
					return new BulletTrigger_Detonate(serializable, loader);
				case BulletEffectType.SpawnStaticSfx:
					return new BulletTrigger_SpawnStaticSfx(serializable, loader);
				case BulletEffectType.GravityField:
					return new BulletTrigger_GravityField(serializable, loader);
				default:
                    throw new DatabaseException("BulletTrigger: Invalid content type - " + serializable.EffectType);
			}
		}

		public abstract T Create<T>(IBulletTriggerFactory<T> factory);

		protected BulletTrigger(BulletTriggerSerializable serializable, Database.Loader loader)
		{
			var variableResolver = GetVariableResolver();
			Condition = serializable.Condition;
			EffectType = serializable.EffectType;
			Cooldown = UnityEngine.Mathf.Clamp(serializable.Cooldown, 0f, 1000f);

			OnDataDeserialized(serializable, loader);
		}

		protected abstract IVariableResolver GetVariableResolver();

		protected abstract class BaseVariableResolver : IVariableResolver
		{
			protected abstract BulletTrigger Context { get; }

			public virtual IFunction<Variant> ResolveFunction(string name)
            {
				return ((IVariableResolver)Context).ResolveFunction(name);
			}

			public virtual Expression<Variant> ResolveVariable(string name)
			{
				if (name == "Cooldown") return GetCooldown;
				return ((IVariableResolver)Context).ResolveVariable(name);
			}

			private Variant GetCooldown() => Context.Cooldown;
		}

		public BulletTriggerCondition Condition { get; private set; }
		public BulletEffectType EffectType { get; private set; }
		public float Cooldown { get; private set; }

		public static BulletTrigger DefaultValue { get; private set; } = Create(new(), null);
	}

	public interface IBulletTriggerFactory<T>
    {
	    T Create(BulletTrigger_None content);
	    T Create(BulletTrigger_PlaySfx content);
	    T Create(BulletTrigger_SpawnBullet content);
	    T Create(BulletTrigger_Detonate content);
	    T Create(BulletTrigger_SpawnStaticSfx content);
	    T Create(BulletTrigger_GravityField content);
    }

    public partial class BulletTrigger_None : BulletTrigger
    {
		partial void OnDataDeserialized(BulletTriggerSerializable serializable, Database.Loader loader);

  		public BulletTrigger_None(BulletTriggerSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			var variableResolver = GetVariableResolver();

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBulletTriggerFactory<T> factory)
        {
            return factory.Create(this);
        }


		private IVariableResolver _iVariableResolver;
		protected override IVariableResolver GetVariableResolver() {
			if(_iVariableResolver == null)
				_iVariableResolver = new VariableResolver(this);
			return _iVariableResolver;
		}

		private class VariableResolver : BaseVariableResolver
		{
			private BulletTrigger_None _context;
			
			protected override BulletTrigger Context => _context;

			public VariableResolver(BulletTrigger_None context)
			{
				_context = context;
			}

			public override IFunction<Variant> ResolveFunction(string name)
            {
				return base.ResolveFunction(name);
			}

			public override Expression<Variant> ResolveVariable(string name)
			{
				return base.ResolveVariable(name);
			}

		}

    }
    public partial class BulletTrigger_PlaySfx : BulletTrigger
    {
		partial void OnDataDeserialized(BulletTriggerSerializable serializable, Database.Loader loader);

  		public BulletTrigger_PlaySfx(BulletTriggerSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			var variableResolver = GetVariableResolver();
			VisualEffect = loader?.GetVisualEffect(new ItemId<VisualEffect>(serializable.VisualEffect)) ?? VisualEffect.DefaultValue;
			AudioClip = new AudioClipId(serializable.AudioClip);
			Color = new ColorData(serializable.Color);
			ColorMode = serializable.ColorMode;
			Size = UnityEngine.Mathf.Clamp(serializable.Size, 0f, 100f);
			Lifetime = UnityEngine.Mathf.Clamp(serializable.Lifetime, 0f, 1000f);
			OncePerCollision = serializable.OncePerCollision;
			UseBulletPosition = serializable.UseBulletPosition;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBulletTriggerFactory<T> factory)
        {
            return factory.Create(this);
        }

		public VisualEffect VisualEffect { get; private set; }
		public AudioClipId AudioClip { get; private set; }
		public ColorData Color { get; private set; }
		public ColorMode ColorMode { get; private set; }
		public float Size { get; private set; }
		public float Lifetime { get; private set; }
		public bool OncePerCollision { get; private set; }
		public bool UseBulletPosition { get; private set; }

		private IVariableResolver _iVariableResolver;
		protected override IVariableResolver GetVariableResolver() {
			if(_iVariableResolver == null)
				_iVariableResolver = new VariableResolver(this);
			return _iVariableResolver;
		}

		private class VariableResolver : BaseVariableResolver
		{
			private BulletTrigger_PlaySfx _context;
			
			protected override BulletTrigger Context => _context;

			public VariableResolver(BulletTrigger_PlaySfx context)
			{
				_context = context;
			}

			public override IFunction<Variant> ResolveFunction(string name)
            {
				return base.ResolveFunction(name);
			}

			public override Expression<Variant> ResolveVariable(string name)
			{
				if (name == "Size") return GetSize;
				if (name == "Lifetime") return GetLifetime;
				if (name == "OncePerCollision") return GetOncePerCollision;
				if (name == "UseBulletPosition") return GetUseBulletPosition;
				return base.ResolveVariable(name);
			}

			private Variant GetSize() => _context.Size;
			private Variant GetLifetime() => _context.Lifetime;
			private Variant GetOncePerCollision() => _context.OncePerCollision;
			private Variant GetUseBulletPosition() => _context.UseBulletPosition;
		}

    }
    public partial class BulletTrigger_SpawnBullet : BulletTrigger
    {
		partial void OnDataDeserialized(BulletTriggerSerializable serializable, Database.Loader loader);

  		public BulletTrigger_SpawnBullet(BulletTriggerSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			var variableResolver = GetVariableResolver();
			AudioClip = new AudioClipId(serializable.AudioClip);
			Ammunition = loader?.GetAmmunition(new ItemId<Ammunition>(serializable.Ammunition)) ?? Ammunition.DefaultValue;
			Color = new ColorData(serializable.Color);
			ColorMode = serializable.ColorMode;
			Quantity = UnityEngine.Mathf.Clamp(serializable.Quantity, 0, 1000);
			Size = UnityEngine.Mathf.Clamp(serializable.Size, 0f, 100f);
			RandomFactor = UnityEngine.Mathf.Clamp(serializable.RandomFactor, 0f, 1f);
			PowerMultiplier = UnityEngine.Mathf.Clamp(serializable.PowerMultiplier, 0f, 3.402823E+38f);
			MaxNestingLevel = UnityEngine.Mathf.Clamp(serializable.MaxNestingLevel, 0, 100);
			_rotation = new Expressions.IntToFloat(serializable.Rotation, -2147483648, 2147483647, variableResolver) { ParamName1 = "i" };
			Rotation = _rotation.Evaluate;
			_offsetX = new Expressions.IntToFloat(serializable.OffsetX, -2147483648, 2147483647, variableResolver) { ParamName1 = "i" };
			OffsetX = _offsetX.Evaluate;
			_offsetY = new Expressions.IntToFloat(serializable.OffsetY, -2147483648, 2147483647, variableResolver) { ParamName1 = "i" };
			OffsetY = _offsetY.Evaluate;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBulletTriggerFactory<T> factory)
        {
            return factory.Create(this);
        }

		public AudioClipId AudioClip { get; private set; }
		public Ammunition Ammunition { get; private set; }
		public ColorData Color { get; private set; }
		public ColorMode ColorMode { get; private set; }
		public int Quantity { get; private set; }
		public float Size { get; private set; }
		public float RandomFactor { get; private set; }
		public float PowerMultiplier { get; private set; }
		public int MaxNestingLevel { get; private set; }
		private readonly Expressions.IntToFloat _rotation;
		public delegate float RotationDelegate(int i);
		public RotationDelegate Rotation { get; private set; }
		private readonly Expressions.IntToFloat _offsetX;
		public delegate float OffsetXDelegate(int i);
		public OffsetXDelegate OffsetX { get; private set; }
		private readonly Expressions.IntToFloat _offsetY;
		public delegate float OffsetYDelegate(int i);
		public OffsetYDelegate OffsetY { get; private set; }

		private IVariableResolver _iVariableResolver;
		protected override IVariableResolver GetVariableResolver() {
			if(_iVariableResolver == null)
				_iVariableResolver = new VariableResolver(this);
			return _iVariableResolver;
		}

		private class VariableResolver : BaseVariableResolver
		{
			private BulletTrigger_SpawnBullet _context;
			
			protected override BulletTrigger Context => _context;

			public VariableResolver(BulletTrigger_SpawnBullet context)
			{
				_context = context;
			}

			public override IFunction<Variant> ResolveFunction(string name)
            {
				if (name == "Rotation") return _context._rotation;
				if (name == "OffsetX") return _context._offsetX;
				if (name == "OffsetY") return _context._offsetY;
				return base.ResolveFunction(name);
			}

			public override Expression<Variant> ResolveVariable(string name)
			{
				if (name == "Quantity") return GetQuantity;
				if (name == "Size") return GetSize;
				if (name == "RandomFactor") return GetRandomFactor;
				if (name == "PowerMultiplier") return GetPowerMultiplier;
				if (name == "MaxNestingLevel") return GetMaxNestingLevel;
				return base.ResolveVariable(name);
			}

			private Variant GetQuantity() => _context.Quantity;
			private Variant GetSize() => _context.Size;
			private Variant GetRandomFactor() => _context.RandomFactor;
			private Variant GetPowerMultiplier() => _context.PowerMultiplier;
			private Variant GetMaxNestingLevel() => _context.MaxNestingLevel;
		}

    }
    public partial class BulletTrigger_Detonate : BulletTrigger
    {
		partial void OnDataDeserialized(BulletTriggerSerializable serializable, Database.Loader loader);

  		public BulletTrigger_Detonate(BulletTriggerSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			var variableResolver = GetVariableResolver();

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBulletTriggerFactory<T> factory)
        {
            return factory.Create(this);
        }


		private IVariableResolver _iVariableResolver;
		protected override IVariableResolver GetVariableResolver() {
			if(_iVariableResolver == null)
				_iVariableResolver = new VariableResolver(this);
			return _iVariableResolver;
		}

		private class VariableResolver : BaseVariableResolver
		{
			private BulletTrigger_Detonate _context;
			
			protected override BulletTrigger Context => _context;

			public VariableResolver(BulletTrigger_Detonate context)
			{
				_context = context;
			}

			public override IFunction<Variant> ResolveFunction(string name)
            {
				return base.ResolveFunction(name);
			}

			public override Expression<Variant> ResolveVariable(string name)
			{
				return base.ResolveVariable(name);
			}

		}

    }
    public partial class BulletTrigger_SpawnStaticSfx : BulletTrigger
    {
		partial void OnDataDeserialized(BulletTriggerSerializable serializable, Database.Loader loader);

  		public BulletTrigger_SpawnStaticSfx(BulletTriggerSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			var variableResolver = GetVariableResolver();
			VisualEffect = loader?.GetVisualEffect(new ItemId<VisualEffect>(serializable.VisualEffect)) ?? VisualEffect.DefaultValue;
			AudioClip = new AudioClipId(serializable.AudioClip);
			Color = new ColorData(serializable.Color);
			ColorMode = serializable.ColorMode;
			Size = UnityEngine.Mathf.Clamp(serializable.Size, 0f, 100f);
			Lifetime = UnityEngine.Mathf.Clamp(serializable.Lifetime, 0f, 1000f);
			OncePerCollision = serializable.OncePerCollision;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBulletTriggerFactory<T> factory)
        {
            return factory.Create(this);
        }

		public VisualEffect VisualEffect { get; private set; }
		public AudioClipId AudioClip { get; private set; }
		public ColorData Color { get; private set; }
		public ColorMode ColorMode { get; private set; }
		public float Size { get; private set; }
		public float Lifetime { get; private set; }
		public bool OncePerCollision { get; private set; }

		private IVariableResolver _iVariableResolver;
		protected override IVariableResolver GetVariableResolver() {
			if(_iVariableResolver == null)
				_iVariableResolver = new VariableResolver(this);
			return _iVariableResolver;
		}

		private class VariableResolver : BaseVariableResolver
		{
			private BulletTrigger_SpawnStaticSfx _context;
			
			protected override BulletTrigger Context => _context;

			public VariableResolver(BulletTrigger_SpawnStaticSfx context)
			{
				_context = context;
			}

			public override IFunction<Variant> ResolveFunction(string name)
            {
				return base.ResolveFunction(name);
			}

			public override Expression<Variant> ResolveVariable(string name)
			{
				if (name == "Size") return GetSize;
				if (name == "Lifetime") return GetLifetime;
				if (name == "OncePerCollision") return GetOncePerCollision;
				return base.ResolveVariable(name);
			}

			private Variant GetSize() => _context.Size;
			private Variant GetLifetime() => _context.Lifetime;
			private Variant GetOncePerCollision() => _context.OncePerCollision;
		}

    }
    public partial class BulletTrigger_GravityField : BulletTrigger
    {
		partial void OnDataDeserialized(BulletTriggerSerializable serializable, Database.Loader loader);

  		public BulletTrigger_GravityField(BulletTriggerSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			var variableResolver = GetVariableResolver();
			Size = UnityEngine.Mathf.Clamp(serializable.Size, 0f, 100f);
			PowerMultiplier = UnityEngine.Mathf.Clamp(serializable.PowerMultiplier, 0f, 3.402823E+38f);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBulletTriggerFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float Size { get; private set; }
		public float PowerMultiplier { get; private set; }

		private IVariableResolver _iVariableResolver;
		protected override IVariableResolver GetVariableResolver() {
			if(_iVariableResolver == null)
				_iVariableResolver = new VariableResolver(this);
			return _iVariableResolver;
		}

		private class VariableResolver : BaseVariableResolver
		{
			private BulletTrigger_GravityField _context;
			
			protected override BulletTrigger Context => _context;

			public VariableResolver(BulletTrigger_GravityField context)
			{
				_context = context;
			}

			public override IFunction<Variant> ResolveFunction(string name)
            {
				return base.ResolveFunction(name);
			}

			public override Expression<Variant> ResolveVariable(string name)
			{
				if (name == "Size") return GetSize;
				if (name == "PowerMultiplier") return GetPowerMultiplier;
				return base.ResolveVariable(name);
			}

			private Variant GetSize() => _context.Size;
			private Variant GetPowerMultiplier() => _context.PowerMultiplier;
		}

    }

}

