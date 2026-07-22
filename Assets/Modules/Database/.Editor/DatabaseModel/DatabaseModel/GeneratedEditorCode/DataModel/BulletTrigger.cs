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

	public interface IBulletTriggerContent
	{
		void Load(BulletTriggerSerializable serializable, Database database);
		void Save(ref BulletTriggerSerializable serializable);
	}

	public partial class BulletTrigger : IDataAdapter
	{
		partial void OnDataDeserialized(BulletTriggerSerializable serializable, Database database);
		partial void OnDataSerialized(ref BulletTriggerSerializable serializable);

		private static IBulletTriggerContent CreateContent(BulletEffectType type)
		{
			switch (type)
			{
				case BulletEffectType.None:
					return new BulletTriggerEmptyContent();
				case BulletEffectType.PlaySfx:
					return new BulletTrigger_PlaySfx();
				case BulletEffectType.SpawnBullet:
					return new BulletTrigger_SpawnBullet();
				case BulletEffectType.Detonate:
					return new BulletTriggerEmptyContent();
				case BulletEffectType.SpawnStaticSfx:
					return new BulletTrigger_SpawnStaticSfx();
				case BulletEffectType.GravityField:
					return new BulletTrigger_GravityField();
				default:
					throw new DatabaseException("BulletTrigger: Invalid content type - " + type);
			}
		}

		public static BulletTrigger Create(BulletTriggerSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new BulletTrigger(serializable, database);
		}

		public BulletTrigger()
		{
			_content = new BulletTriggerEmptyContent();
		}

		private BulletTrigger(BulletTriggerSerializable serializable, Database database)
		{
			Condition = serializable.Condition;
			EffectType = serializable.EffectType;
			Cooldown = new NumericValue<float>(serializable.Cooldown, 0f, 1000f);
			_content = CreateContent(serializable.EffectType);
			_content.Load(serializable, database);

			OnDataDeserialized(serializable, database);
		}

		public BulletTriggerSerializable Serialize()
		{
			var serializable = new BulletTriggerSerializable();
			serializable.VisualEffect = 0;
			serializable.AudioClip = string.Empty;
			serializable.Ammunition = 0;
			serializable.Color = string.Empty;
			serializable.ColorMode = 0;
			serializable.Quantity = 0;
			serializable.Size = 0f;
			serializable.Lifetime = 0f;
			serializable.RandomFactor = 0f;
			serializable.PowerMultiplier = 0f;
			serializable.MaxNestingLevel = 0;
			serializable.OncePerCollision = false;
			serializable.UseBulletPosition = false;
			serializable.SyncLifetimeWithBullet = false;
			serializable.Rotation = "IF(Quantity <= 1, 0, RANDOM(0, 360))";
			serializable.OffsetX = "IF(Quantity <= 1, 0, Size / 2)";
			serializable.OffsetY = "0";
			_content.Save(ref serializable);
			serializable.Condition = Condition;
			serializable.EffectType = EffectType;
			serializable.Cooldown = Cooldown.Value;
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

				yield return new Property(this, type.GetField("Condition"), DataChangedEvent);
				yield return new Property(this, type.GetField("EffectType"), OnTypeChanged);
				yield return new Property(this, type.GetField("Cooldown"), DataChangedEvent);

				foreach (var item in _content.GetType().GetFields().Where(f => f.IsPublic && !f.IsStatic))
					yield return new Property(_content, item, DataChangedEvent);
			}
		}

		private void OnTypeChanged()
		{
			_content = CreateContent(EffectType);
			DataChangedEvent?.Invoke();
			LayoutChangedEvent?.Invoke();
		}

		private IBulletTriggerContent _content;
		public BulletTriggerCondition Condition;
		public BulletEffectType EffectType;
		public NumericValue<float> Cooldown = new NumericValue<float>(0, 0f, 1000f);

		public static BulletTrigger DefaultValue { get; private set; }
	}

	public class BulletTriggerEmptyContent : IBulletTriggerContent
	{
		public void Load(BulletTriggerSerializable serializable, Database database) {}
		public void Save(ref BulletTriggerSerializable serializable) {}
	}

	public partial class BulletTrigger_PlaySfx : IBulletTriggerContent
	{
		partial void OnDataDeserialized(BulletTriggerSerializable serializable, Database database);
		partial void OnDataSerialized(ref BulletTriggerSerializable serializable);

		public void Load(BulletTriggerSerializable serializable, Database database)
		{
			VisualEffect = database.GetVisualEffectId(serializable.VisualEffect);
			AudioClip = serializable.AudioClip;
			Color = Helpers.ColorFromString(serializable.Color);
			ColorMode = serializable.ColorMode;
			Size = new NumericValue<float>(serializable.Size, 0f, 100f);
			Lifetime = new NumericValue<float>(serializable.Lifetime, 0f, 1000f);
			OncePerCollision = serializable.OncePerCollision;
			UseBulletPosition = serializable.UseBulletPosition;
			SyncLifetimeWithBullet = serializable.SyncLifetimeWithBullet;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BulletTriggerSerializable serializable)
		{
			serializable.VisualEffect = VisualEffect.Value;
			serializable.AudioClip = AudioClip;
			serializable.Color = Helpers.ColorToString(Color);
			serializable.ColorMode = ColorMode;
			serializable.Size = Size.Value;
			serializable.Lifetime = Lifetime.Value;
			serializable.OncePerCollision = OncePerCollision;
			serializable.UseBulletPosition = UseBulletPosition;
			serializable.SyncLifetimeWithBullet = SyncLifetimeWithBullet;
			OnDataSerialized(ref serializable);
		}

		public ItemId<VisualEffect> VisualEffect = ItemId<VisualEffect>.Empty;
		public string AudioClip;
		public System.Drawing.Color Color;
		public ColorMode ColorMode;
		public NumericValue<float> Size = new NumericValue<float>(0, 0f, 100f);
		public NumericValue<float> Lifetime = new NumericValue<float>(0, 0f, 1000f);
		public bool OncePerCollision;
		public bool UseBulletPosition;
		public bool SyncLifetimeWithBullet;
	}

	public partial class BulletTrigger_SpawnBullet : IBulletTriggerContent
	{
		partial void OnDataDeserialized(BulletTriggerSerializable serializable, Database database);
		partial void OnDataSerialized(ref BulletTriggerSerializable serializable);

		public void Load(BulletTriggerSerializable serializable, Database database)
		{
			AudioClip = serializable.AudioClip;
			Ammunition = database.GetAmmunitionId(serializable.Ammunition);
			Color = Helpers.ColorFromString(serializable.Color);
			ColorMode = serializable.ColorMode;
			Quantity = new NumericValue<int>(serializable.Quantity, 0, 1000);
			Size = new NumericValue<float>(serializable.Size, 0f, 100f);
			RandomFactor = new NumericValue<float>(serializable.RandomFactor, 0f, 1f);
			PowerMultiplier = new NumericValue<float>(serializable.PowerMultiplier, 0f, 3.402823E+38f);
			MaxNestingLevel = new NumericValue<int>(serializable.MaxNestingLevel, 0, 100);
			Rotation = serializable.Rotation;
			OffsetX = serializable.OffsetX;
			OffsetY = serializable.OffsetY;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BulletTriggerSerializable serializable)
		{
			serializable.AudioClip = AudioClip;
			serializable.Ammunition = Ammunition.Value;
			serializable.Color = Helpers.ColorToString(Color);
			serializable.ColorMode = ColorMode;
			serializable.Quantity = Quantity.Value;
			serializable.Size = Size.Value;
			serializable.RandomFactor = RandomFactor.Value;
			serializable.PowerMultiplier = PowerMultiplier.Value;
			serializable.MaxNestingLevel = MaxNestingLevel.Value;
			serializable.Rotation = Rotation;
			serializable.OffsetX = OffsetX;
			serializable.OffsetY = OffsetY;
			OnDataSerialized(ref serializable);
		}

		public string AudioClip;
		public ItemId<Ammunition> Ammunition = ItemId<Ammunition>.Empty;
		public System.Drawing.Color Color;
		public ColorMode ColorMode;
		public NumericValue<int> Quantity = new NumericValue<int>(0, 0, 1000);
		public NumericValue<float> Size = new NumericValue<float>(0, 0f, 100f);
		public NumericValue<float> RandomFactor = new NumericValue<float>(0, 0f, 1f);
		public NumericValue<float> PowerMultiplier = new NumericValue<float>(0, 0f, 3.402823E+38f);
		public NumericValue<int> MaxNestingLevel = new NumericValue<int>(0, 0, 100);
		public string Rotation;
		public string OffsetX;
		public string OffsetY;
	}

	public partial class BulletTrigger_SpawnStaticSfx : IBulletTriggerContent
	{
		partial void OnDataDeserialized(BulletTriggerSerializable serializable, Database database);
		partial void OnDataSerialized(ref BulletTriggerSerializable serializable);

		public void Load(BulletTriggerSerializable serializable, Database database)
		{
			VisualEffect = database.GetVisualEffectId(serializable.VisualEffect);
			AudioClip = serializable.AudioClip;
			Color = Helpers.ColorFromString(serializable.Color);
			ColorMode = serializable.ColorMode;
			Size = new NumericValue<float>(serializable.Size, 0f, 100f);
			Lifetime = new NumericValue<float>(serializable.Lifetime, 0f, 1000f);
			OncePerCollision = serializable.OncePerCollision;

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BulletTriggerSerializable serializable)
		{
			serializable.VisualEffect = VisualEffect.Value;
			serializable.AudioClip = AudioClip;
			serializable.Color = Helpers.ColorToString(Color);
			serializable.ColorMode = ColorMode;
			serializable.Size = Size.Value;
			serializable.Lifetime = Lifetime.Value;
			serializable.OncePerCollision = OncePerCollision;
			OnDataSerialized(ref serializable);
		}

		public ItemId<VisualEffect> VisualEffect = ItemId<VisualEffect>.Empty;
		public string AudioClip;
		public System.Drawing.Color Color;
		public ColorMode ColorMode;
		public NumericValue<float> Size = new NumericValue<float>(0, 0f, 100f);
		public NumericValue<float> Lifetime = new NumericValue<float>(0, 0f, 1000f);
		public bool OncePerCollision;
	}

	public partial class BulletTrigger_GravityField : IBulletTriggerContent
	{
		partial void OnDataDeserialized(BulletTriggerSerializable serializable, Database database);
		partial void OnDataSerialized(ref BulletTriggerSerializable serializable);

		public void Load(BulletTriggerSerializable serializable, Database database)
		{
			Size = new NumericValue<float>(serializable.Size, 0f, 100f);
			PowerMultiplier = new NumericValue<float>(serializable.PowerMultiplier, 0f, 3.402823E+38f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref BulletTriggerSerializable serializable)
		{
			serializable.Size = Size.Value;
			serializable.PowerMultiplier = PowerMultiplier.Value;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> Size = new NumericValue<float>(0, 0f, 100f);
		public NumericValue<float> PowerMultiplier = new NumericValue<float>(0, 0f, 3.402823E+38f);
	}

}

