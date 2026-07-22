using System.Collections.Generic;
using System.Linq;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;
using UnityEngine;
using Zenject;

namespace GameServices.Multiplayer
{
    /// <summary>
    /// Lightweight host-authoritative synchronizer: client sends controls; host sends
    /// authoritative ship transforms and resources at 15 Hz.
    /// </summary>
    public sealed class MultiplayerBattleSync : ITickable
    {
        public MultiplayerBattleSync(IScene scene) => _scene = scene;

        public void Tick()
        {
            var session = MultiplayerSession.Instance;
            if (session == null || !session.IsActive) return;
            _elapsed += Time.deltaTime;
            if (session.IsHost)
            {
                if (_elapsed < 1f / 15f) return;
                _elapsed = 0;
                SendHostSnapshot(session);
            }
            else
            {
                if (_elapsed >= 1f / 20f)
                {
                    _elapsed = 0;
                    SendClientInput(session);
                }
                ApplySnapshot(session.LatestSnapshot);
            }
        }

        private void SendClientInput(MultiplayerSession session)
        {
            var ship = _scene.PlayerShip;
            if (ship == null || !ship.IsActive()) return;
            var controls = ship.Controls;
            session.SendInput(new NetInput
            {
                sequence = ++_inputSequence,
                throttle = controls.Throttle,
                hasCourse = controls.Course.HasValue,
                course = controls.Course ?? 0,
                systems = EncodeSystems(controls.Systems),
            });
        }

        private void SendHostSnapshot(MultiplayerSession session)
        {
            var hostShips = StableShips(UnitSide.Player);
            var clientShips = StableShips(UnitSide.Enemy);
            var snapshots = new List<ShipSnapshot>(hostShips.Count + clientShips.Count);
            AddSnapshots(snapshots, hostShips, 0);
            AddSnapshots(snapshots, clientShips, 1);
            session.SendSnapshot(new SnapshotBatch { sequence = ++_snapshotSequence, ships = snapshots.ToArray() });
        }

        private void ApplySnapshot(SnapshotBatch batch)
        {
            if (batch == null || batch.sequence <= _lastAppliedSnapshot || batch.ships == null) return;
            _lastAppliedSnapshot = batch.sequence;
            var localShips = StableShips(UnitSide.Player);
            var remoteShips = StableShips(UnitSide.Enemy);
            foreach (var snapshot in batch.ships)
            {
                var list = snapshot.owner == 1 ? localShips : remoteShips;
                if (snapshot.slot < 0 || snapshot.slot >= list.Count) continue;
                var ship = list[snapshot.slot];
                ship.Body.Move(Vector2.Lerp(ship.Body.Position, new Vector2(snapshot.x, snapshot.y), 0.65f));
                ship.Body.Turn(Mathf.LerpAngle(ship.Body.Rotation, snapshot.rotation, 0.65f));
                Correct(ship.Stats.Armor, snapshot.armor);
                Correct(ship.Stats.Shield, snapshot.shield);
                Correct(ship.Stats.Energy, snapshot.energy);
                if (snapshot.owner == 0)
                    MultiplayerController.Apply(ship, snapshot.throttle, snapshot.hasCourse, snapshot.course, snapshot.systems);
            }
        }

        private List<IShip> StableShips(UnitSide side)
        {
            var stable = side == UnitSide.Player ? _playerShips : _enemyShips;
            foreach (var ship in _scene.Ships.Items)
            {
                if (ship == null || ship.Type.Side != side || !ship.IsActive() || stable.Contains(ship)) continue;
                stable.Add(ship);
            }
            stable.RemoveAll(ship => ship == null || !ship.IsActive());
            return stable;
        }

        private static void AddSnapshots(ICollection<ShipSnapshot> output, IReadOnlyList<IShip> ships, int owner)
        {
            for (var slot = 0; slot < ships.Count; slot++)
            {
                var ship = ships[slot];
                output.Add(new ShipSnapshot
                {
                    owner = owner, slot = slot,
                    x = ship.Body.Position.x, y = ship.Body.Position.y, rotation = ship.Body.Rotation,
                    armor = ship.Stats.Armor.Value, shield = ship.Stats.Shield.Value, energy = ship.Stats.Energy.Value,
                    throttle = ship.Controls.Throttle, hasCourse = ship.Controls.Course.HasValue,
                    course = ship.Controls.Course ?? 0, systems = EncodeSystems(ship.Controls.Systems),
                });
            }
        }

        private static string EncodeSystems(Combat.Component.Controls.SystemsState systems)
        {
            var chars = new char[systems.Count];
            for (var i = 0; i < chars.Length; i++) chars[i] = systems.GetState(i) ? '1' : '0';
            return new string(chars);
        }

        private static void Correct(Combat.Unit.HitPoints.IResourcePoints resource, float target)
        {
            if (resource == null || Mathf.Abs(resource.Value - target) < 0.01f) return;
            resource.Get(resource.Value - target);
        }

        private readonly IScene _scene;
        private float _elapsed;
        private int _inputSequence;
        private int _snapshotSequence;
        private int _lastAppliedSnapshot;
        private readonly List<IShip> _playerShips = new();
        private readonly List<IShip> _enemyShips = new();
    }
}
