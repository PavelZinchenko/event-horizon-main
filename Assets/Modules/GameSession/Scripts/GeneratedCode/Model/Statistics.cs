//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using Session.Utils;

namespace Session.Model
{
	public class Statistics : IDataChangedCallback
	{
		private readonly IDataChangedCallback _parent;
		private ObservableSet<int> _unlockedShips;
		private int _defeatedEnemies;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		public Statistics(IDataChangedCallback parent)
		{
			_parent = parent;
			_unlockedShips = new ObservableSet<int>(this);
			_defeatedEnemies = default(int);
		}

		public Statistics(SessionDataReader reader, IDataChangedCallback parent)
		{
			int unlockedShipsItemCount;
			unlockedShipsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_unlockedShips = new ObservableSet<int>(this);
			for (int i = 0; i < unlockedShipsItemCount; ++i)
			{
				int item;
				item = reader.ReadInt(EncodingType.EliasGamma);
				_unlockedShips.Add(item);
			}
			_defeatedEnemies = reader.ReadInt(EncodingType.EliasGamma);
			_parent = parent;
			DataChanged = false;
		}

		public ObservableSet<int> UnlockedShips => _unlockedShips;
		public int DefeatedEnemies
		{
			get => _defeatedEnemies;
			set
			{
				if (_defeatedEnemies == value) return;
				_defeatedEnemies = value;
				OnDataChanged();
			}
		}

		public void Serialize(SessionDataWriter writer)
		{
			writer.WriteInt(_unlockedShips.Count, EncodingType.EliasGamma);
			foreach (var item in _unlockedShips.Items)
			{
				writer.WriteInt(item, EncodingType.EliasGamma);
			}
			writer.WriteInt(_defeatedEnemies, EncodingType.EliasGamma);
			DataChanged = false;
		}

		public void OnDataChanged()
		{
			DataChanged = true;
			_parent?.OnDataChanged();
		}
	}
}
