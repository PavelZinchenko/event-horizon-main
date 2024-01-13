using CommonComponents.Signals;

namespace Session
{
	public class MoneyValueChangedSignal : SmartWeakSignal<MoneyValueChangedSignal, long> { }
	public class StarsValueChangedSignal : SmartWeakSignal<StarsValueChangedSignal, long> { }
	public class FuelValueChangedSignal : SmartWeakSignal<FuelValueChangedSignal, int> { }
	public class TokensValueChangedSignal : SmartWeakSignal<TokensValueChangedSignal, int> { }
	public class ResourcesChangedSignal : SmartWeakSignal<ResourcesChangedSignal> { }
	public class SessionDataLoadedSignal : SmartWeakSignal<SessionDataLoadedSignal> { }
	public class SessionCreatedSignal : SmartWeakSignal<SessionCreatedSignal> { }
	public class PlayerPositionChangedSignal : SmartWeakSignal<PlayerPositionChangedSignal, int> { }
	public class NewStarSecuredSignal : SmartWeakSignal<NewStarSecuredSignal, int> { }
	public class PlayerSkillsResetSignal : SmartWeakSignal<PlayerSkillsResetSignal> { }
}
