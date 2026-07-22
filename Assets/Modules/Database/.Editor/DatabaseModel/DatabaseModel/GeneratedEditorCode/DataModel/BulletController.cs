//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using System.Collections.Generic;
using EditorDatabase.Enums;
using EditorDatabase.Serializable;
using EditorDatabase.Model;

namespace EditorDatabase.DataModel
{

	public interface IBulletControllerContent
	{
		void Load(BulletControllerSerializable serializable, Database database);
		void Save(ref BulletControllerSerializable serializable);
	}

	public partial class BulletController : IDataAdapter
	{
		partial void OnDataDeserialized(BulletControllerSerializable serializable, Database database);
		partial void OnDataSerialized(ref BulletControllerSerializable serializable);

		private static IBulletControllerContent CreateContent(BulletControllerType type)
		{
			switch (type)
			{
				case BulletControllerType.Projectile:
					return new BulletControllerEmptyContent();
				case BulletControllerType.Homing:
					return new BulletController_Homing();
				case BulletControllerType.Beam:
					return new BulletControllerEmptyContent();
				case BulletControllerType.Parametric:
					return new BulletController_Parametric();
				case BulletControllerType.Harpoon:
					return new BulletControllerEmptyContent();
				case BulletControllerType.AuraEmitter:
					return new BulletControllerEmptyContent();
				case BulletControllerType.StickyMine:
					return new BulletController_StickyMine();
				default:
					throw new DatabaseException("BulletController: Invalid content type - " + type);
			}
		}

		public static BulletController Create(BulletControllerSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new BulletController(serializable, database);
		}

		public BulletController()
		{
			_content = new BulletControllerEmptyContent();
		}

		private BulletController(BulletControllerSerializable serializable, Database database)
		{
			Type = serializable.Type;
			_content = CreateContent(serializable.Type);
			_content.Load(serializable, database);

			OnDataDeserialized(serializable, database);
		}

		public BulletControllerSerializable Serialize()
		{
			var serializable = new BulletControllerSerializable();
			serializable.StartingVelocityModifier = 1f;
			serializable.IgnoreRotation = false;
			serializable.SmartAim = false;
			serializable.Lifetime = 0f;
			serializable.X = "0";
			serializable.Y = "0";
			serializable.Rotation = "0";
			serializable.Size = "1";
			serializable.Length = "1";
			_content.Save(ref serializable);
			serializable.Type = Type;
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public event System.Action LayoutChangedEvent;
		public event System.Action DataChangedEvent;

		public IEnumerable<IProperty> Properties
		{
			get
			{
				var type = GetType();

				yield return new Property(this, type.GetField("Type"), OnTypeChanged);

				foreach (var item in _content.GetType().GetFields().Where(f => f.IsPublic && !f.IsStatic))
					yield return new Property(_content, item, DataChangedEvent);
			}
		}

		private void OnTypeChanged()
		{
			_content = CreateContent(Type);
			DataChangedEvent?.Invoke();
			LayoutChangedEvent?.Invoke();
		}

		private IBulletControllerContent _content;
		public BulletControllerType Type;

		public static BulletController DefaultValue { get; private set; }
	}

	public class BulletControllerEmptyContent : IBulletControllerContent
	{
		public void Load(BulletControllerSerializable serializable, Database database) {}
		public void Save(ref BulletControllerSerializable serializable) {}
	}

	public partial class BulletController_Homing : IBulletControllerContent
	{
		partial void OnDataDeserialized(BulletControllerSerializable serializable, Database database);
		partial void OnDataSerialized(ref BulletControllerSerializable serializable);

		public void Load(BulletControllerSerializable serializable, Database database)
		{
			StartingVelocityModifier = new NumericValue<float>(serializable.StartingVelocityModifier, 0f, 1000f);
			IgnoreRotation = serializable.IgnoreRotation;
			SmartAim = serializable.SmartAim;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BulletControllerSerializable serializable)
		{
			serializable.StartingVelocityModifier = StartingVelocityModifier.Value;
			serializable.IgnoreRotation = IgnoreRotation;
			serializable.SmartAim = SmartAim;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> StartingVelocityModifier = new NumericValue<float>(0, 0f, 1000f);
		public bool IgnoreRotation;
		public bool SmartAim;
	}

	public partial class BulletController_Parametric : IBulletControllerContent
	{
		partial void OnDataDeserialized(BulletControllerSerializable serializable, Database database);
		partial void OnDataSerialized(ref BulletControllerSerializable serializable);

		public void Load(BulletControllerSerializable serializable, Database database)
		{
			X = serializable.X;
			Y = serializable.Y;
			Rotation = serializable.Rotation;
			Size = serializable.Size;
			Length = serializable.Length;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BulletControllerSerializable serializable)
		{
			serializable.X = X;
			serializable.Y = Y;
			serializable.Rotation = Rotation;
			serializable.Size = Size;
			serializable.Length = Length;
			OnDataSerialized(ref serializable);
		}

		public string X;
		public string Y;
		public string Rotation;
		public string Size;
		public string Length;
	}

	public partial class BulletController_StickyMine : IBulletControllerContent
	{
		partial void OnDataDeserialized(BulletControllerSerializable serializable, Database database);
		partial void OnDataSerialized(ref BulletControllerSerializable serializable);

		public void Load(BulletControllerSerializable serializable, Database database)
		{
			Lifetime = new NumericValue<float>(serializable.Lifetime, 0f, 3.402823E+38f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BulletControllerSerializable serializable)
		{
			serializable.Lifetime = Lifetime.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> Lifetime = new NumericValue<float>(0, 0f, 3.402823E+38f);
	}

}

