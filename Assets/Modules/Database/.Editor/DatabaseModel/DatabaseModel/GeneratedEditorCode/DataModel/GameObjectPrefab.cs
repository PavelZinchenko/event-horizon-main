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

	public interface IGameObjectPrefabContent
	{
		void Load(GameObjectPrefabSerializable serializable, Database database);
		void Save(ref GameObjectPrefabSerializable serializable);
	}

	public partial class GameObjectPrefab : IDataAdapter
	{
		partial void OnDataDeserialized(GameObjectPrefabSerializable serializable, Database database);
		partial void OnDataSerialized(ref GameObjectPrefabSerializable serializable);

		private static IGameObjectPrefabContent CreateContent(ObjectPrefabType type)
		{
			switch (type)
			{
				case ObjectPrefabType.Undefined:
					return new GameObjectPrefabEmptyContent();
				case ObjectPrefabType.WormTailSegment:
					return new GameObjectPrefab_WormTailSegment();
				case ObjectPrefabType.CircularSpriteObject:
					return new GameObjectPrefab_CircularSpriteObject();
				case ObjectPrefabType.CircularOutlineObject:
					return new GameObjectPrefab_CircularOutlineObject();
				default:
					throw new DatabaseException("GameObjectPrefab: Invalid content type - " + type);
			}
		}

		public static GameObjectPrefab Create(GameObjectPrefabSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new GameObjectPrefab(serializable, database);
		}

		public GameObjectPrefab()
		{
			_content = new GameObjectPrefabEmptyContent();
		}

		private GameObjectPrefab(GameObjectPrefabSerializable serializable, Database database)
		{
			Id = new ItemId<GameObjectPrefab>(serializable);

			Type = serializable.Type;
			_content = CreateContent(serializable.Type);
			_content.Load(serializable, database);

			OnDataDeserialized(serializable, database);
		}

		public void Save(GameObjectPrefabSerializable serializable)
		{
			serializable.Image1 = string.Empty;
			serializable.Image2 = string.Empty;
			serializable.ImageScale = 1f;
			serializable.Thickness = 0.1f;
			serializable.AspectRatio = 1f;
			serializable.ImageOffset = 0f;
			serializable.Length = 0f;
			serializable.Offset1 = 0f;
			serializable.Offset2 = 0f;
			serializable.Angle1 = 0f;
			serializable.Angle2 = 0f;
			_content.Save(ref serializable);
			serializable.Type = Type;
			OnDataSerialized(ref serializable);
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

		public readonly ItemId<GameObjectPrefab> Id;

		private IGameObjectPrefabContent _content;
		public ObjectPrefabType Type;

		public static GameObjectPrefab DefaultValue { get; private set; }
	}

	public class GameObjectPrefabEmptyContent : IGameObjectPrefabContent
	{
		public void Load(GameObjectPrefabSerializable serializable, Database database) {}
		public void Save(ref GameObjectPrefabSerializable serializable) {}
	}

	public partial class GameObjectPrefab_WormTailSegment : IGameObjectPrefabContent
	{
		partial void OnDataDeserialized(GameObjectPrefabSerializable serializable, Database database);
		partial void OnDataSerialized(ref GameObjectPrefabSerializable serializable);

		public void Load(GameObjectPrefabSerializable serializable, Database database)
		{
			BodyImage = serializable.Image1;
			JointImage = serializable.Image2;
			JointImageScale = new NumericValue<float>(serializable.ImageScale, 0f, 10f);
			JointImageOffset = new NumericValue<float>(serializable.ImageOffset, -1f, 1f);
			BoneLength = new NumericValue<float>(serializable.Length, 0f, 1f);
			JointOffset = new NumericValue<float>(serializable.Offset1, 0f, 1f);
			HeadOffset = new NumericValue<float>(serializable.Offset2, -1f, 1f);
			MaxRotation = new NumericValue<float>(serializable.Angle1, 0f, 180f);
			MaxHeadRotation = new NumericValue<float>(serializable.Angle2, 0f, 180f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref GameObjectPrefabSerializable serializable)
		{
			serializable.Image1 = BodyImage;
			serializable.Image2 = JointImage;
			serializable.ImageScale = JointImageScale.Value;
			serializable.ImageOffset = JointImageOffset.Value;
			serializable.Length = BoneLength.Value;
			serializable.Offset1 = JointOffset.Value;
			serializable.Offset2 = HeadOffset.Value;
			serializable.Angle1 = MaxRotation.Value;
			serializable.Angle2 = MaxHeadRotation.Value;
			OnDataSerialized(ref serializable);
		}

		public string BodyImage;
		public string JointImage;
		public NumericValue<float> JointImageScale = new NumericValue<float>(0, 0f, 10f);
		public NumericValue<float> JointImageOffset = new NumericValue<float>(0, -1f, 1f);
		public NumericValue<float> BoneLength = new NumericValue<float>(0, 0f, 1f);
		public NumericValue<float> JointOffset = new NumericValue<float>(0, 0f, 1f);
		public NumericValue<float> HeadOffset = new NumericValue<float>(0, -1f, 1f);
		public NumericValue<float> MaxRotation = new NumericValue<float>(0, 0f, 180f);
		public NumericValue<float> MaxHeadRotation = new NumericValue<float>(0, 0f, 180f);
	}

	public partial class GameObjectPrefab_CircularSpriteObject : IGameObjectPrefabContent
	{
		partial void OnDataDeserialized(GameObjectPrefabSerializable serializable, Database database);
		partial void OnDataSerialized(ref GameObjectPrefabSerializable serializable);

		public void Load(GameObjectPrefabSerializable serializable, Database database)
		{
			Image = serializable.Image1;
			ImageScale = new NumericValue<float>(serializable.ImageScale, 0f, 10f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref GameObjectPrefabSerializable serializable)
		{
			serializable.Image1 = Image;
			serializable.ImageScale = ImageScale.Value;
			OnDataSerialized(ref serializable);
		}

		public string Image;
		public NumericValue<float> ImageScale = new NumericValue<float>(0, 0f, 10f);
	}

	public partial class GameObjectPrefab_CircularOutlineObject : IGameObjectPrefabContent
	{
		partial void OnDataDeserialized(GameObjectPrefabSerializable serializable, Database database);
		partial void OnDataSerialized(ref GameObjectPrefabSerializable serializable);

		public void Load(GameObjectPrefabSerializable serializable, Database database)
		{
			Image = serializable.Image1;
			ImageScale = new NumericValue<float>(serializable.ImageScale, 0f, 10f);
			Thickness = new NumericValue<float>(serializable.Thickness, 0f, 1f);
			AspectRatio = new NumericValue<float>(serializable.AspectRatio, 0f, 100f);

			OnDataDeserialized(serializable, database);
		}

		public void Save(ref GameObjectPrefabSerializable serializable)
		{
			serializable.Image1 = Image;
			serializable.ImageScale = ImageScale.Value;
			serializable.Thickness = Thickness.Value;
			serializable.AspectRatio = AspectRatio.Value;
			OnDataSerialized(ref serializable);
		}

		public string Image;
		public NumericValue<float> ImageScale = new NumericValue<float>(0, 0f, 10f);
		public NumericValue<float> Thickness = new NumericValue<float>(0, 0f, 1f);
		public NumericValue<float> AspectRatio = new NumericValue<float>(0, 0f, 100f);
	}

}

