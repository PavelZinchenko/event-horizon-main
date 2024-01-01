using System;
using SmartWeakEvent;

namespace CommonComponents.Signals
{
	public class SmartWeakSignal<TSignal> : ISignal
		where TSignal : SmartWeakSignal<TSignal>
	{
		private readonly SmartWeakEvent<Action> _event = new();

		public event Action Event
		{
			add { _event.Add(value); }
			remove { _event.Remove(value); }
		}

		public class Trigger : ITrigger
		{
			private readonly TSignal _signal;
			public Trigger(TSignal signal) => _signal = signal;
			public void Fire() => _signal._event.Raise();
		}
	}

	public class SmartWeakSignal<TSignal, TParam> : ISignal
		where TSignal : SmartWeakSignal<TSignal, TParam>
	{
		private readonly SmartWeakEvent<Action<TParam>> _event = new();

		public event Action<TParam> Event
		{
			add { _event.Add(value); }
			remove { _event.Remove(value); }
		}

		public class Trigger : ITrigger
		{
			private readonly TSignal _signal;
			public Trigger(TSignal signal) => _signal = signal;
			public void Fire(TParam param) => _signal._event.Raise(param);
		}
	}

	public class SmartWeakSignal<TSignal, TParam1, TParam2> : ISignal
	where TSignal : SmartWeakSignal<TSignal, TParam1, TParam2>
	{
		private readonly SmartWeakEvent<Action<TParam1, TParam2>> _event = new();

		public event Action<TParam1, TParam2> Event
		{
			add { _event.Add(value); }
			remove { _event.Remove(value); }
		}

		public class Trigger : ITrigger
		{
			private readonly TSignal _signal;
			public Trigger(TSignal signal) => _signal = signal;
			public void Fire(TParam1 param1, TParam2 param2) => _signal._event.Raise(param1, param2);
		}
	}
}
