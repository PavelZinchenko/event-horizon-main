using Session.Model;

namespace Session.Content
{
	public interface IWormholeData
	{
		int GetTarget(int source);
		void SetTarget(int source, int target);
	}

	public class WormholeData : IWormholeData, ISessionDataContent
	{
		private readonly SaveGameData _data;

		public WormholeData(SaveGameData sessionData) => _data = sessionData;

		public int GetTarget(int source) => _data.Wormholes.Routes.TryGetValue(source, out var target) ? target : -1;
		public void SetTarget(int source, int target)
		{
			if (_data.Wormholes.Routes.ContainsKey(source) || _data.Wormholes.Routes.ContainsKey(target))
				throw new System.InvalidOperationException();

			_data.Wormholes.Routes.Add(source, target);
			_data.Wormholes.Routes.Add(target, source);
		}
	}
}
