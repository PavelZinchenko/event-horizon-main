using System;

namespace CommonComponents.Signals
{
	public class Signal<TSignal> : ISignal
		where TSignal : Signal<TSignal>
	{
		private event Action _event;

		public event Action Event
		{
			add { _event += value; }
			remove { _event -= value; }
		}

		public class Trigger : ITrigger
		{
			private readonly TSignal _signal;
			public Trigger(TSignal signal) => _signal = signal;
			public void Fire() => _signal._event?.Invoke();
		}
	}

	public class Signal<TSignal, TParam> : ISignal
		where TSignal : Signal<TSignal, TParam>
	{
		private event Action<TParam> _event;

		public event Action<TParam> Event
		{
			add { _event += value; }
			remove { _event -= value; }
		}

		public class Trigger : ITrigger
		{
			private readonly TSignal _signal;
			public Trigger(TSignal signal) => _signal = signal;
			public void Fire(TParam param) => _signal._event?.Invoke(param);
		}
	}

	public class Signal<TSignal, TParam1, TParam2> : ISignal
		where TSignal : Signal<TSignal, TParam1, TParam2>
	{
		private event Action<TParam1, TParam2> _event;

		public event Action<TParam1, TParam2> Event
		{
			add { _event += value; }
			remove { _event -= value; }
		}

		public class Trigger : ITrigger
		{
			private readonly TSignal _signal;
			public Trigger(TSignal signal) => _signal = signal;
			public void Fire(TParam1 param1, TParam2 param2) => _signal._event?.Invoke(param1, param2);
		}
	}
}
