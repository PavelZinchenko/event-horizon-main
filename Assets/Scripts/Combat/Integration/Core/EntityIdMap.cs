using System.Collections.Generic;
using CoreEntityId = CombatSimulation.Core.EntityId;

namespace Combat.Integration.Core
{
    // Maps 64-bit core entity IDs to the application's sequential IDs for one battle. Never truncates the hash.
    // An application ID is never reused, so a stale ID cannot reach a new ship.
    public sealed class EntityIdMap
    {
        public EntityId Assign(CoreEntityId core)
        {
            var id = new EntityId(_nextId++);
            _toApplication.Add(core, id);
            _toCore.Add(id, core);
            return id;
        }

        public bool TryGet(CoreEntityId core, out EntityId id) => _toApplication.TryGetValue(core, out id);
        public bool TryGetCore(EntityId id, out CoreEntityId core) => _toCore.TryGetValue(id, out core);

        public void Release(CoreEntityId core)
        {
            if (!_toApplication.TryGetValue(core, out var id)) return;
            _toApplication.Remove(core);
            _toCore.Remove(id);
        }

        public void Clear()
        {
            _toApplication.Clear();
            _toCore.Clear();
        }

        private int _nextId = 1;
        private readonly Dictionary<CoreEntityId, EntityId> _toApplication = new Dictionary<CoreEntityId, EntityId>();
        private readonly Dictionary<EntityId, CoreEntityId> _toCore = new Dictionary<EntityId, CoreEntityId>();
    }
}
