using System;
using System.Collections.Generic;
using GameServices.SceneManager;

namespace Services.Messenger
{
    public class MessengerContext : IMessengerContext
    {
        private readonly Dictionary<EventType, DelegateList> _events = new Dictionary<EventType, DelegateList>();

        public void Cleanup(GameScene scene)
        {
            foreach (var item in _events)
                item.Value.Remove(scene);
        }

        public void AddListener(EventType eventType, GameScene scene, Callback handler)
        {
            DelegateList list;
            if (!_events.TryGetValue(eventType, out list))
            {
                list = new DelegateList();
                _events.Add(eventType, list);
            }

            list.Add(scene, handler);
        }

        public void AddListener<T>(EventType eventType, GameScene scene, Callback<T> handler)
        {
            DelegateList list;
            if (!_events.TryGetValue(eventType, out list))
            {
                list = new DelegateList();
                _events.Add(eventType, list);
            }

            list.Add(scene, handler);
        }

        public void AddListener<T,U>(EventType eventType, GameScene scene, Callback<T,U> handler)
        {
            DelegateList list;
            if (!_events.TryGetValue(eventType, out list))
            {
                list = new DelegateList();
                _events.Add(eventType, list);
            }

            list.Add(scene, handler);
        }

        public void AddListener<T,U,V>(EventType eventType, GameScene scene, Callback<T,U,V> handler)
        {
            DelegateList list;
            if (!_events.TryGetValue(eventType, out list))
            {
                list = new DelegateList();
                _events.Add(eventType, list);
            }

            list.Add(scene, handler);
        }

        public void Broadcast(EventType eventType)
        {
            DelegateList list;
            if (!_events.TryGetValue(eventType, out list))
                return;

            list.Broadcast();
        }

        public void Broadcast<T>(EventType eventType, T arg1)
        {
            DelegateList list;
            if (!_events.TryGetValue(eventType, out list))
                return;

            list.Broadcast(arg1);
        }

        public void Broadcast<T,U>(EventType eventType, T arg1, U arg2)
        {
            DelegateList list;
            if (!_events.TryGetValue(eventType, out list))
                return;

            list.Broadcast(arg1,arg2);
        }

        public void Broadcast<T,U,V>(EventType eventType, T arg1, U arg2, V arg3)
        {
            DelegateList list;
            if (!_events.TryGetValue(eventType, out list))
                return;

            list.Broadcast(arg1,arg2,arg3);
        }

        private class DelegateList
        {
            public void Add(GameScene scene, Callback handler)
            {
                Delegate eventDelegate;
                if (_delegates.TryGetValue(scene, out eventDelegate))
                    _delegates[scene] = (Callback)eventDelegate + handler;
                else
                    _delegates[scene] = handler;
            }

            public void Add<T>(GameScene scene, Callback<T> handler)
            {
                Delegate eventDelegate;
                if (_delegates.TryGetValue(scene, out eventDelegate))
                    _delegates[scene] = (Callback<T>)eventDelegate + handler;
                else
                    _delegates[scene] = handler;
            }

            public void Add<T, U>(GameScene scene, Callback<T, U> handler)
            {
                Delegate eventDelegate;
                if (_delegates.TryGetValue(scene, out eventDelegate))
                    _delegates[scene] = (Callback<T,U>)eventDelegate + handler;
                else
                    _delegates[scene] = handler;
            }

            public void Add<T, U, V>(GameScene scene, Callback<T, U, V> handler)
            {
                Delegate eventDelegate;
                if (_delegates.TryGetValue(scene, out eventDelegate))
                    _delegates[scene] = (Callback<T,U,V>)eventDelegate + handler;
                else
                    _delegates[scene] = handler;
            }

            public void Remove(GameScene scene)
            {
                _delegates.Remove(scene);
            }

            public void Broadcast()
            {
                var items = _delegates.Values;
                var count = items.Count;
                for (var i = 0; i < count; ++i)
                    ((Callback)items[i]).Invoke();
            }

            public void Broadcast<T>(T arg1)
            {
                var items = _delegates.Values;
                var count = items.Count;
                for (var i = 0; i < count; ++i)
                    ((Callback<T>)items[i]).Invoke(arg1);
            }

            public void Broadcast<T, U>(T arg1, U arg2)
            {
                var items = _delegates.Values;
                var count = items.Count;
                for (var i = 0; i < count; ++i)
                    ((Callback<T, U>)items[i]).Invoke(arg1, arg2);
            }

            public void Broadcast<T, U, V>(T arg1, U arg2, V arg3)
            {
                var items = _delegates.Values;
                var count = items.Count;
                for (var i = 0; i < count; ++i)
                    ((Callback<T, U, V>)items[i]).Invoke(arg1, arg2, arg3);
            }

            private readonly SortedList<GameScene, Delegate> _delegates = new SortedList<GameScene, Delegate>();
        }
    }
}
