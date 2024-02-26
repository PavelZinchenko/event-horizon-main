using System.Collections.Generic;
using System.Linq;
using GameDatabase;
using GameDatabase.Model;
using GameDatabase.DataModel;
using Constructor.Ships;
using Session.Utils;
using Constructor.Ships.Modification;

namespace Session.Model
{
	public readonly partial struct ShipInfo
	{
		public ShipInfo(IShip ship, IDataChangedCallback callback)
		{
			_id = ship.Id.Value;
			_name = ship.Name;
			_colorScheme = ship.ColorScheme.Value;
			_experience = (long)ship.Experience;
			_components = new ObservableList<ShipComponentInfo>(ship.Components.Select(item => new ShipComponentInfo(item)), callback);
			_layoutModifications = new ObservableList<byte>(ship.Model.LayoutModifications.Serialize(), callback);
			_modifications = new ObservableList<long>(ship.Model.Modifications.Select(ShipModificationExtensions.Serialize), callback);
			_satellite1 = new SatelliteInfo(ship.FirstSatellite, callback);
			_satellite2 = new SatelliteInfo(ship.SecondSatellite, callback);
		}

		public ShipInfo(
			int id, 
			string name, 
			long colorScheme, 
			long experience, 
			IEnumerable<ShipComponentInfo> components,
			IEnumerable<byte> layoutModifications,
			IEnumerable<long> modifications,
			SatelliteInfo satellite1,
			SatelliteInfo satellite2,
			IDataChangedCallback callback)
		{
			_id = id;
			_name = name;
			_colorScheme = colorScheme;
			_experience = experience;
			_components = new ObservableList<ShipComponentInfo>(components, callback);
			_layoutModifications = new ObservableList<byte>(layoutModifications, callback);
			_modifications = new ObservableList<long>(modifications, callback);
			_satellite1 = satellite1;
			_satellite2 = satellite2;
		}

		public IShip ToShip(IDatabase database)
		{
			var shipData = database.GetShip(new ItemId<Ship>(_id));
			if (shipData == null)
				return null;

			var shipModel = new ShipModel(shipData);
			var factory = new ModificationFactory(database);
			shipModel.Modifications.Assign(_modifications.Select(item => ShipModificationExtensions.Deserialize(item, factory)));
			shipModel.LayoutModifications.Deserialize(_layoutModifications.ToArray());
			var components = _components.ToIntegratedComponents(database);
			var ship = new CommonShip(shipModel, components);

			ship.FirstSatellite = _satellite1.ToSatellite(database);
			ship.SecondSatellite = _satellite2.ToSatellite(database);
			ship.Name = _name;
			ship.ColorScheme.Value = _colorScheme;
			ship.Experience = (long)_experience;
			ship.DataChanged = false;
			return ship;
		}
	}
}
