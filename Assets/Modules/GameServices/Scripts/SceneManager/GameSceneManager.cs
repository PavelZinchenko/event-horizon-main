using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Zenject;

namespace GameServices.SceneManager
{
    public class GameSceneManager : MonoBehaviour, IGameSceneManager, IInitializable, IDisposable
    {
        [Inject] private readonly SceneBeforeUnloadSignal.Trigger _sceneBeforeUnloadTrigger;
        [Inject] private readonly SceneLoadedSignal.Trigger _sceneLoadedTrigger;
        [Inject] private readonly SceneManagerStateChangedSignal.Trigger _stateChangedTrigger;

        public void Initialize()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;

            UpdateLoadedScenes();
        }

        public void Dispose()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        public State State
        {
            get
            {
                return _state;
            }
            private set
            {
                if (_state == value)
                    return;

                _state = value;
                _stateChangedTrigger.Fire(value);
            }
        }

        public void Load(IEnumerable<GameScene> scenes)
        {
            State = State.Loading;

            _requiredScenes.Clear();
            foreach (var id in scenes)
            {
                if (id == GameScene.Undefined)
                    continue;
                if (!_requiredScenes.Add(id))
                    continue;
                if (_activeScenes.Contains(id) || _loadingScenes.Contains(id))
                    continue;

                _loadingScenes.Add(id);
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(id.ToSceneName(), LoadSceneMode.Additive);
            }

            foreach (var id in _activeScenes)
            {
                if (_requiredScenes.Contains(id))
                    continue;

                _sceneBeforeUnloadTrigger.Fire(id);
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(id.ToSceneName());
                foreach (var gameobject in scene.GetRootGameObjects())
                    gameobject.SetActive(false);

                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
            }

            _activeScenes.IntersectWith(_requiredScenes);

            if (_loadingScenes.Count == 0)
                State = State.Ready;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var sceneId = scene.name.ToGameScene();
            if (sceneId == GameScene.Undefined)
            {
                UnityEngine.Debug.Log("Undefined scene loaded - " + scene.name);
                Debug.Break();
                return;
            }

            if (!_requiredScenes.Contains(sceneId))
            {
                UnityEngine.Debug.Log("Unexpected scene loaded - " + scene.name);
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
                Debug.Break();
                return;
            }

            _loadingScenes.Remove(sceneId);
            _activeScenes.Add(sceneId);

            _sceneLoadedTrigger.Fire(sceneId);

            if (_loadingScenes.Count == 0)
                State = State.Ready;
        }

        private void OnSceneUnloaded(Scene scene)
        {
        }

        private void UpdateLoadedScenes()
        {
            _activeScenes.Clear();
            var count = UnityEngine.SceneManagement.SceneManager.sceneCount;
            for (var i = 0; i < count; ++i)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (!scene.IsValid())
                    continue;
                var id = scene.name.ToGameScene();
                if (id == GameScene.Undefined)
                    continue;
                
                _activeScenes.Add(id);
            }
        }

        private State _state = State.Ready;
        private readonly HashSet<GameScene> _requiredScenes = new HashSet<GameScene>();
        private readonly HashSet<GameScene> _activeScenes = new HashSet<GameScene>();
        private readonly HashSet<GameScene> _loadingScenes = new HashSet<GameScene>();
    }
}
