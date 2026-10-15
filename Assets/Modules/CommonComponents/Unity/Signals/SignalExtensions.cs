using Zenject;

namespace CommonComponents.Signals
{
    public static class SignalExtensions
	{
		public static IfNotBoundBinder BindSignal<TSignal>(this DiContainer container) where TSignal : ISignal
		{
			return container.Bind<TSignal>().AsSingle();
		}

		public static IfNotBoundBinder BindSignal<TSignal>(this DiContainer container, TSignal instance) where TSignal : ISignal
		{
			return container.Bind<TSignal>().FromInstance(instance);
		}

		public static IfNotBoundBinder BindTrigger<TTrigger>(this DiContainer container) where TTrigger : ITrigger
		{
			return container.Bind<TTrigger>().AsSingle();
		}
	}
}
