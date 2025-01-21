


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
	public abstract partial class BulletController : IDefaultVariablesResolver
	{
		partial void OnDataDeserialized(BulletControllerSerializable serializable, Database.Loader loader);

		public static BulletController Create(BulletControllerSerializable serializable, Database.Loader loader)
		{
			if (serializable == null) return DefaultValue;

			switch (serializable.Type)
		    {
				case BulletControllerType.Projectile:
					return new BulletController_Projectile(serializable, loader);
				case BulletControllerType.Homing:
					return new BulletController_Homing(serializable, loader);
				case BulletControllerType.Beam:
					return new BulletController_Beam(serializable, loader);
				case BulletControllerType.Parametric:
					return new BulletController_Parametric(serializable, loader);
				case BulletControllerType.Harpoon:
					return new BulletController_Harpoon(serializable, loader);
				case BulletControllerType.AuraEmitter:
					return new BulletController_AuraEmitter(serializable, loader);
				case BulletControllerType.StickyMine:
					return new BulletController_StickyMine(serializable, loader);
				default:
                    throw new DatabaseException("BulletController: Invalid content type - " + serializable.Type);
			}
		}

		public abstract T Create<T>(IBulletControllerFactory<T> factory);

		protected BulletController(BulletControllerSerializable serializable, Database.Loader loader)
		{
			var variableResolver = GetVariableResolver();
			Type = serializable.Type;

			OnDataDeserialized(serializable, loader);
		}

		protected abstract IVariableResolver GetVariableResolver();

		protected abstract class BaseVariableResolver : IVariableResolver
		{
			protected abstract BulletController Context { get; }

			public virtual IFunction<Variant> ResolveFunction(string name)
            {
				return ((IVariableResolver)Context).ResolveFunction(name);
			}

			public virtual Expression<Variant> ResolveVariable(string name)
			{
				return ((IVariableResolver)Context).ResolveVariable(name);
			}

		}

		public BulletControllerType Type { get; private set; }

		public static BulletController DefaultValue { get; private set; } = Create(new(), null);
	}

	public interface IBulletControllerFactory<T>
    {
	    T Create(BulletController_Projectile content);
	    T Create(BulletController_Homing content);
	    T Create(BulletController_Beam content);
	    T Create(BulletController_Parametric content);
	    T Create(BulletController_Harpoon content);
	    T Create(BulletController_AuraEmitter content);
	    T Create(BulletController_StickyMine content);
    }

    public partial class BulletController_Projectile : BulletController
    {
		partial void OnDataDeserialized(BulletControllerSerializable serializable, Database.Loader loader);

  		public BulletController_Projectile(BulletControllerSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			var variableResolver = GetVariableResolver();

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBulletControllerFactory<T> factory)
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
			private BulletController_Projectile _context;
			
			protected override BulletController Context => _context;

			public VariableResolver(BulletController_Projectile context)
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
    public partial class BulletController_Homing : BulletController
    {
		partial void OnDataDeserialized(BulletControllerSerializable serializable, Database.Loader loader);

  		public BulletController_Homing(BulletControllerSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			var variableResolver = GetVariableResolver();
			StartingVelocityModifier = UnityEngine.Mathf.Clamp(serializable.StartingVelocityModifier, 0f, 1000f);
			IgnoreRotation = serializable.IgnoreRotation;
			SmartAim = serializable.SmartAim;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBulletControllerFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float StartingVelocityModifier { get; private set; }
		public bool IgnoreRotation { get; private set; }
		public bool SmartAim { get; private set; }

		private IVariableResolver _iVariableResolver;
		protected override IVariableResolver GetVariableResolver() {
			if(_iVariableResolver == null)
				_iVariableResolver = new VariableResolver(this);
			return _iVariableResolver;
		}

		private class VariableResolver : BaseVariableResolver
		{
			private BulletController_Homing _context;
			
			protected override BulletController Context => _context;

			public VariableResolver(BulletController_Homing context)
			{
				_context = context;
			}

			public override IFunction<Variant> ResolveFunction(string name)
            {
				return base.ResolveFunction(name);
			}

			public override Expression<Variant> ResolveVariable(string name)
			{
				if (name == "StartingVelocityModifier") return GetStartingVelocityModifier;
				if (name == "IgnoreRotation") return GetIgnoreRotation;
				if (name == "SmartAim") return GetSmartAim;
				return base.ResolveVariable(name);
			}

			private Variant GetStartingVelocityModifier() => _context.StartingVelocityModifier;
			private Variant GetIgnoreRotation() => _context.IgnoreRotation;
			private Variant GetSmartAim() => _context.SmartAim;
		}

    }
    public partial class BulletController_Beam : BulletController
    {
		partial void OnDataDeserialized(BulletControllerSerializable serializable, Database.Loader loader);

  		public BulletController_Beam(BulletControllerSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			var variableResolver = GetVariableResolver();

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBulletControllerFactory<T> factory)
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
			private BulletController_Beam _context;
			
			protected override BulletController Context => _context;

			public VariableResolver(BulletController_Beam context)
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
    public partial class BulletController_Parametric : BulletController
    {
		partial void OnDataDeserialized(BulletControllerSerializable serializable, Database.Loader loader);

  		public BulletController_Parametric(BulletControllerSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			var variableResolver = GetVariableResolver();
			_x = new Expressions.FloatToFloat(serializable.X, -2147483648, 2147483647, variableResolver) { ParamName1 = "t" };
			X = _x.Evaluate;
			_y = new Expressions.FloatToFloat(serializable.Y, -2147483648, 2147483647, variableResolver) { ParamName1 = "t" };
			Y = _y.Evaluate;
			_rotation = new Expressions.FloatToFloat(serializable.Rotation, -2147483648, 2147483647, variableResolver) { ParamName1 = "t" };
			Rotation = _rotation.Evaluate;
			_size = new Expressions.FloatToFloat(serializable.Size, -2147483648, 2147483647, variableResolver) { ParamName1 = "t" };
			Size = _size.Evaluate;
			_length = new Expressions.FloatToFloat(serializable.Length, -2147483648, 2147483647, variableResolver) { ParamName1 = "t" };
			Length = _length.Evaluate;

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBulletControllerFactory<T> factory)
        {
            return factory.Create(this);
        }

		private readonly Expressions.FloatToFloat _x;
		public delegate float XDelegate(float t);
		public XDelegate X { get; private set; }
		private readonly Expressions.FloatToFloat _y;
		public delegate float YDelegate(float t);
		public YDelegate Y { get; private set; }
		private readonly Expressions.FloatToFloat _rotation;
		public delegate float RotationDelegate(float t);
		public RotationDelegate Rotation { get; private set; }
		private readonly Expressions.FloatToFloat _size;
		public delegate float SizeDelegate(float t);
		public SizeDelegate Size { get; private set; }
		private readonly Expressions.FloatToFloat _length;
		public delegate float LengthDelegate(float t);
		public LengthDelegate Length { get; private set; }

		private IVariableResolver _iVariableResolver;
		protected override IVariableResolver GetVariableResolver() {
			if(_iVariableResolver == null)
				_iVariableResolver = new VariableResolver(this);
			return _iVariableResolver;
		}

		private class VariableResolver : BaseVariableResolver
		{
			private BulletController_Parametric _context;
			
			protected override BulletController Context => _context;

			public VariableResolver(BulletController_Parametric context)
			{
				_context = context;
			}

			public override IFunction<Variant> ResolveFunction(string name)
            {
				if (name == "X") return _context._x;
				if (name == "Y") return _context._y;
				if (name == "Rotation") return _context._rotation;
				if (name == "Size") return _context._size;
				if (name == "Length") return _context._length;
				return base.ResolveFunction(name);
			}

			public override Expression<Variant> ResolveVariable(string name)
			{
				return base.ResolveVariable(name);
			}

		}

    }
    public partial class BulletController_Harpoon : BulletController
    {
		partial void OnDataDeserialized(BulletControllerSerializable serializable, Database.Loader loader);

  		public BulletController_Harpoon(BulletControllerSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			var variableResolver = GetVariableResolver();

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBulletControllerFactory<T> factory)
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
			private BulletController_Harpoon _context;
			
			protected override BulletController Context => _context;

			public VariableResolver(BulletController_Harpoon context)
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
    public partial class BulletController_AuraEmitter : BulletController
    {
		partial void OnDataDeserialized(BulletControllerSerializable serializable, Database.Loader loader);

  		public BulletController_AuraEmitter(BulletControllerSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			var variableResolver = GetVariableResolver();

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBulletControllerFactory<T> factory)
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
			private BulletController_AuraEmitter _context;
			
			protected override BulletController Context => _context;

			public VariableResolver(BulletController_AuraEmitter context)
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
    public partial class BulletController_StickyMine : BulletController
    {
		partial void OnDataDeserialized(BulletControllerSerializable serializable, Database.Loader loader);

  		public BulletController_StickyMine(BulletControllerSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			var variableResolver = GetVariableResolver();
			Lifetime = UnityEngine.Mathf.Clamp(serializable.Lifetime, 0f, 3.402823E+38f);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IBulletControllerFactory<T> factory)
        {
            return factory.Create(this);
        }

		public float Lifetime { get; private set; }

		private IVariableResolver _iVariableResolver;
		protected override IVariableResolver GetVariableResolver() {
			if(_iVariableResolver == null)
				_iVariableResolver = new VariableResolver(this);
			return _iVariableResolver;
		}

		private class VariableResolver : BaseVariableResolver
		{
			private BulletController_StickyMine _context;
			
			protected override BulletController Context => _context;

			public VariableResolver(BulletController_StickyMine context)
			{
				_context = context;
			}

			public override IFunction<Variant> ResolveFunction(string name)
            {
				return base.ResolveFunction(name);
			}

			public override Expression<Variant> ResolveVariable(string name)
			{
				if (name == "Lifetime") return GetLifetime;
				return base.ResolveVariable(name);
			}

			private Variant GetLifetime() => _context.Lifetime;
		}

    }

}

