using System.Collections.Generic;
using Session.Utils;

namespace Session
{
	public partial class SessionLoader
	{
		partial void Upgrage_v3_0_to_v4_0(v3.SaveGameData oldData, Model.SaveGameData newData)
		{
			GameDiagnostics.Trace.LogWarning("Upgrading savegame from v3.0 to v4.0");

			var fleet = newData.Fleet;
			foreach (var ship in oldData.Fleet.Ships)
			{
				fleet.Ships.Add(new Model.ShipInfo(ship.Id, ship.Name, ship.ColorScheme, ship.Experience, ship.Components,
					ship.LayoutModifications, ship.Modifications, SatelliteSlotsToList(ship.Satellite1, ship.Satellite2, fleet), fleet));
			}

			var presets = newData.ShipPresets;
			foreach (var preset in oldData.ShipPresets.Presets)
			{
				presets.Presets.Add(new Model.ShipPresetInfo(preset.Id, preset.Name, preset.Components,
					SatelliteSlotsToList(preset.Satellite1, preset.Satellite2, presets), presets));
			}
		}

		// Satellite1 and Satellite2 become mount points 0 and 1. An empty first slot stays empty; a trailing empty slot is dropped.
		private static List<Model.SatelliteInfo> SatelliteSlotsToList(Model.SatelliteInfo first, Model.SatelliteInfo second, IDataChangedCallback callback)
		{
			var satellites = new List<Model.SatelliteInfo>(2);
			if (first.Id == 0 && second.Id == 0) return satellites;

			satellites.Add(new Model.SatelliteInfo(first.Id, first.Components, callback));
			if (second.Id != 0)
				satellites.Add(new Model.SatelliteInfo(second.Id, second.Components, callback));

			return satellites;
		}
	}
}
