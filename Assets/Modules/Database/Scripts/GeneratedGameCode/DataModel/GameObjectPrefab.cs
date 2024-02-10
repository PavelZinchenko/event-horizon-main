


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
	public abstract partial class GameObjectPrefab 
	{
		partial void OnDataDeserialized(GameObjectPrefabSerializable serializable, Database.Loader loader);

		public static GameObjectPrefab Create(GameObjectPrefabSerializable serializable, Database.Loader loader)
		{
			if (serializable == null) return DefaultValue;

			switch (serializable.Type)
		    {
				case ObjectPrefabType.Undefined:
					return new GameObjectPrefab_Undefined(serializable, loader);
				case ObjectPrefabType.WormTailSegment:
					return new GameObjectPrefab_WormTailSegment(serializable, loader);
				default:
                    throw new DatabaseException("GameObjectPrefab: Invalid content type - " + serializable.Type);
			}
		}

		public abstract T Create<T>(IGameObjectPrefabFactory<T> factory);

		protected GameObjectPrefab(GameObjectPrefabSerializable serializable, Database.Loader loader)
		{
			Id = new ItemId<GameObjectPrefab>(serializable.Id);
			loader.AddGameObjectPrefab(serializable.Id, this);

			Type = serializable.Type;

			OnDataDeserialized(serializable, loader);
		}


		public readonly ItemId<GameObjectPrefab> Id;

		public ObjectPrefabType Type { get; private set; }

		public static GameObjectPrefab DefaultValue { get; private set; }
	}

	public interface IGameObjectPrefabFactory<T>
    {
	    T Create(GameObjectPrefab_Undefined content);
	    T Create(GameObjectPrefab_WormTailSegment content);
    }

    public partial class GameObjectPrefab_Undefined : GameObjectPrefab
    {
		partial void OnDataDeserialized(GameObjectPrefabSerializable serializable, Database.Loader loader);

  		public GameObjectPrefab_Undefined(GameObjectPrefabSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IGameObjectPrefabFactory<T> factory)
        {
            return factory.Create(this);
        }



    }
    public partial class GameObjectPrefab_WormTailSegment : GameObjectPrefab
    {
		partial void OnDataDeserialized(GameObjectPrefabSerializable serializable, Database.Loader loader);

  		public GameObjectPrefab_WormTailSegment(GameObjectPrefabSerializable serializable, Database.Loader loader)
            : base(serializable, loader)
        {
			BodyImage = new SpriteId(serializable.Image1, SpriteId.Type.Satellite);
			JointImage = new SpriteId(serializable.Image2, SpriteId.Type.Satellite);

            OnDataDeserialized(serializable, loader);
        }

        public override T Create<T>(IGameObjectPrefabFactory<T> factory)
        {
            return factory.Create(this);
        }

		public SpriteId BodyImage { get; private set; }
		public SpriteId JointImage { get; private set; }


    }

}

