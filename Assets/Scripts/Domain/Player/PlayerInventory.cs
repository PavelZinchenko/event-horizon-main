using System.Linq;
using Constructor;
using Diagnostics;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Model;
using GameServices.GameManager;
using Session;
using CommonComponents.Utils;
using Zenject;
using Domain.Quests;

namespace GameServices.Player
{
    public sealed class PlayerInventory : GameServiceBase
    {
        [Inject] private readonly IDatabase _database;

        [Inject]
        public PlayerInventory(ISessionData session, SessionDataLoadedSignal dataLoadedSignal, SessionCreatedSignal sessionCreatedSignal, 
            SessionAboutToSaveSignal sessionAboutToSaveSignal)
            : base(dataLoadedSignal, sessionCreatedSignal)
        {
            _session = session;
            _sessionAboutToSaveSignal = sessionAboutToSaveSignal;
            _sessionAboutToSaveSignal.Event += OnSessionAboutToSave;
        }

        public IGameItemCollection<ComponentInfo> Components { get { return _components; } }
        public IGameItemCollection<Satellite> Satellites { get { return _satellites; } }

        protected override void OnSessionDataLoaded()
        {
            Load();
        }

        protected override void OnSessionCreated() { }

        private void Load()
        {
            Clear();

            var random = new System.Random(_session.Game.Seed);
            foreach (var item in _session.Inventory.Components.Items)
            {
                var componentInfo = item.Key.ToComponentInfo(_database);
                if (!componentInfo)
                {
                    GameDiagnostics.Trace.LogError($"Unknown module in inventory - {item.Key}");
                    continue;
                }

                if (!componentInfo.IsValidModification)
                {
                    GameDiagnostics.Trace.LogError($"Invalid modification {componentInfo.ModificationType} for module {componentInfo.Data.Id}");
                    componentInfo = ComponentInfo.CreateRandomModification(componentInfo.Data, random, componentInfo.ModificationQuality, componentInfo.ModificationQuality);
                }

                _components.Add(componentInfo, item.Value);
            }

            foreach (var item in _session.Inventory.Satellites.Items)
                _satellites.Add(_database.GetSatellite(new ItemId<Satellite>(item.Key)), item.Value);

            _components.IsDirty = false;
            _satellites.IsDirty = false;
        }

        private void SaveComponents()
        {
            if (_components.IsDirty)
            {
                _session.Inventory.Components.Clear();
                foreach (var item in _components.Items)
                    _session.Inventory.Components.Add(new Session.Model.InventoryComponentInfo(item.Key), item.Value);

                _components.IsDirty = false;
            }

            if (_satellites.IsDirty)
            {
                _session.Inventory.Satellites.Clear();
                foreach (var item in _satellites.Items)
                    _session.Inventory.Satellites.Add(item.Key.Id.Value, item.Value);

                _satellites.IsDirty = false;
            }
        }

        private void OnSessionAboutToSave()
        {
            if (_satellites.IsDirty || _components.IsDirty)
                SaveComponents();
        }

        private void Clear()
        {
            _components.Clear();
            _satellites.Clear();
        }

        private readonly GameItemCollection<ComponentInfo> _components = new GameItemCollection<ComponentInfo>();
        private readonly GameItemCollection<Satellite> _satellites = new GameItemCollection<Satellite>();
        private readonly SessionAboutToSaveSignal _sessionAboutToSaveSignal;

        private readonly ISessionData _session;
    }
}