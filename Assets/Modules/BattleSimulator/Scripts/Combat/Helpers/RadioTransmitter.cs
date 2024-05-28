using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Combat.Component.Unit;
using Combat.Factory;

namespace Combat.Helpers
{
    public class RadioTransmitter : ITickable
    {
		private const float _speed = 1.0f;
		private readonly EffectFactory _effectFactory;
		private readonly Queue<MessageData> _messages = new();
		private readonly System.Random _random = new();
		
		public RadioTransmitter(EffectFactory effectFactory)
        {
            _effectFactory = effectFactory;
        }

		public void Broadcast(IUnit unit, string message, Color color, float time = 2f)
		{
			lock (this)
			{
				var data = new MessageData
				{
					Unit = unit,
					Text = message,
					Color = color,
					Time = time
				};

				_messages.Enqueue(data);
			}
		}

		public void Tick()
		{
			lock (this)
			{
				while (_messages.TryDequeue(out var data))
				{
                    var position = data.Unit.Body.Position;
                    if (!_effectFactory.IsObjectVisible(position, data.Unit.Body.Scale)) continue;

                    var effect = _effectFactory.CreateTextEffect(data.Text);
					effect.Color = data.Color;
					effect.Position = position;
					effect.Run(data.Time, data.Unit.Body.Velocity + _speed*RotationHelpers.Direction(_random.Next(360)), 0);
				}
			}
		}

		private struct MessageData
		{
			public IUnit Unit;
			public string Text;
			public Color Color;
			public float Time;
		}
	}
}
