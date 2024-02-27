using System.Linq;
using Constructor.Ships;
using Session.Model;
using Session.Utils;
using System.Collections.Generic;

namespace Session.Content
{
	public interface IShipPresetsData
    {
		ObservableList<ShipPresetInfo> Presets { get; }
        void UpdatePresets(IEnumerable<IShipPreset> presets);
    }

    public class ShipPresetsData : IShipPresetsData, ISessionDataContent
	{
		private readonly SaveGameData _data;

		public ShipPresetsData(SaveGameData sessionData) => _data = sessionData;

        public ObservableList<ShipPresetInfo> Presets => _data.ShipPresets.Presets;

        public void UpdatePresets(IEnumerable<IShipPreset> presets)
        {
            _data.ShipPresets.Presets.Assign(presets.Select(item => new ShipPresetInfo(item, _data.Fleet)), new ShipPresetInfo.EqualityComparer());
        }
    }
}
