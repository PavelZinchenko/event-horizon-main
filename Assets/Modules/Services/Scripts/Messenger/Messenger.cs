using System;
using GameServices.SceneManager;
using Zenject;

namespace Services.Messenger
{
    public class Messenger : IMessenger
    {
        [Inject]
        public Messenger(GameScene scene, SceneBeforeUnloadSignal sceneBeforeUnloadSignal, IMessengerContext context)
        {
            _scene = scene;
            _context = context;
            _sceneBeforeUnloadSignal = sceneBeforeUnloadSignal;
            _sceneBeforeUnloadSignal.Event += Cleanup;
        }

        public void AddListener(EventType eventType, Callback handler)
        {
            if (!_isEnabled)
                throw new InvalidOperationException();

            _context.AddListener(eventType, _scene, handler);
        }

        public void AddListener<T>(EventType eventType, Callback<T> handler)
        {
            if (!_isEnabled)
                throw new InvalidOperationException();

            _context.AddListener(eventType, _scene, handler);
        }

        public void AddListener<T, U>(EventType eventType, Callback<T, U> handler)
        {
            if (!_isEnabled)
                throw new InvalidOperationException();

            _context.AddListener(eventType, _scene, handler);
        }

        public void AddListener<T, U, V>(EventType eventType, Callback<T, U, V> handler)
        {
            if (!_isEnabled)
                throw new InvalidOperationException();

            _context.AddListener(eventType, _scene, handler);
        }

        public void Broadcast(EventType eventType)
        {
            _context.Broadcast(eventType);
        }

        public void Broadcast<T>(EventType eventType, T arg1)
        {
            _context.Broadcast(eventType, arg1);
        }

        public void Broadcast<T, U>(EventType eventType, T arg1, U arg2)
        {
            _context.Broadcast(eventType, arg1, arg2);
        }

        public void Broadcast<T, U, V>(EventType eventType, T arg1, U arg2, V arg3)
        {
            _context.Broadcast(eventType, arg1, arg2, arg3);
        }

        private void Cleanup(GameScene scene)
        {
            if (!_isEnabled || scene != _scene)
                return;

            _isEnabled = false;
            _context.Cleanup(_scene);
            UnityEngine.Debug.Log("Messenger.Cleanup - " + _scene);
        }

		public void RemoveListener(EventType eventType, Callback handler)
		{
			throw new NotImplementedException();
		}

		public void RemoveListener<T>(EventType eventType, Callback<T> handler)
		{
			throw new NotImplementedException();
		}

		public void RemoveListener<T, U>(EventType eventType, Callback<T, U> handler)
		{
			throw new NotImplementedException();
		}

		public void RemoveListener<T, U, V>(EventType eventType, Callback<T, U, V> handler)
		{
			throw new NotImplementedException();
		}

		private bool _isEnabled = true;
        private readonly GameScene _scene;
        private readonly IMessengerContext _context;
        private readonly SceneBeforeUnloadSignal _sceneBeforeUnloadSignal;
    }
}
