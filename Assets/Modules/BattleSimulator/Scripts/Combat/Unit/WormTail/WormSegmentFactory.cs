using Combat.Component.Ship;
using Combat.Component.Stats;
using Combat.Factory;
using Constructor.Model;
using UnityEngine;

namespace Combat.Component.Unit
{
    public interface IWormSegmentFactory
    {
        int Index { get; }
        int MaxLength { get; }
        IWormSegment Create(IShip ship, IWormSegment parent);
    }

    public class WormSegmentFactory : IWormSegmentFactory
    {
        private const int _unbreakableSegments = 2;

        private const float _scaleFactor = 0.95f;
        private const float _rotationAngle = 40f;
        private const float _headRotationAngle = 75f;
        private const float _firsSpawnCooldown = 0.05f;
        private const float _secondSpawnCooldown = 0.5f;

        private readonly SpaceObjectFactory _objectFactory;
        private readonly GameObject _prefab;
        private readonly ColorScheme _colorScheme;
        private readonly int _totalSegments;
        private readonly int _segmentIndex;
        private readonly float _hitPoints;
        private readonly float _parentOffset;
        private readonly float _jointOffset;
        private readonly float _respawnCooldown;
        private int _spawnCounter;

        public int Index => _segmentIndex;
        public int MaxLength => _totalSegments;

        private WormSegmentFactory(
            GameObject prefab,
            int totalSegments,
            int segmentIndex,
            float hitPoints,
            float respawnCooldown,
            float parentOffset,
            float jointOffset,
            bool isRespawn,
            ColorScheme colorScheme,
            SpaceObjectFactory objectFactory)
        {
            _objectFactory = objectFactory;
            _prefab = prefab;
            _colorScheme = colorScheme;
            _totalSegments = totalSegments;
            _hitPoints = hitPoints;
            _parentOffset = parentOffset;
            _jointOffset = jointOffset;
            _respawnCooldown = respawnCooldown;
            _segmentIndex = segmentIndex;
            _spawnCounter = isRespawn ? 1 : 0;
        }

        public static IWormSegment Create(
            SpaceObjectFactory objectFactory,
            GameObject prefab,
            IShip ship,
            int length,
            float weight,
            float hitPoints,
            float parentOffset,
            float jointOffset,
            float extraHeadOffset,
            float respawnCooldown,
            bool isRespawn,
            ColorScheme colorScheme,
            IDamageIndicator damageIndicator)
        {
            var initialCooldown = isRespawn ? _secondSpawnCooldown : _firsSpawnCooldown;
            var segmentFactory = new WormSegmentFactory(prefab, length, 1, hitPoints, respawnCooldown,
                parentOffset, jointOffset, isRespawn, colorScheme, objectFactory);
            var scale = ship.Body.Scale * _scaleFactor;
            var unitFactory = new FixedWormSegment.Factory(hitPoints, initialCooldown, respawnCooldown, segmentFactory);

            var segment = objectFactory.CreateWormTailSegment(prefab, ship, null, scale, weight, colorScheme, unitFactory);
            segment.Attach(ship, parentOffset + extraHeadOffset, jointOffset, _headRotationAngle);
            segment.DamageIndicator = damageIndicator;
            segment.AddResource(damageIndicator);
            return segment;
        }

        public IWormSegment Create(IShip ship, IWormSegment lastSegment)
        {
            var index = _segmentIndex + 1;
            if (index > _totalSegments) return null;

            var initialCooldown = _spawnCounter > 0 ? _secondSpawnCooldown : _firsSpawnCooldown;

            var segmentFactory = new WormSegmentFactory(
                _prefab, _totalSegments, index, _hitPoints, _respawnCooldown, _parentOffset, _jointOffset, _spawnCounter > 0, _colorScheme, _objectFactory);

            IUnitFactory<WormSegmentBase> unitFactory = index <= _unbreakableSegments ?
                new FixedWormSegment.Factory(_hitPoints, initialCooldown, _respawnCooldown, segmentFactory) :
                new WormSegmentDestroyable.Factory(_hitPoints, initialCooldown, _respawnCooldown, segmentFactory);

            var size = lastSegment.Body.Scale * _scaleFactor;
            var weight = lastSegment.Body.Weight * _scaleFactor;

            var segment = _objectFactory.CreateWormTailSegment(_prefab, ship, lastSegment, size, weight, _colorScheme, unitFactory);

            segment.Attach(lastSegment, _parentOffset, _jointOffset, _rotationAngle);
            segment.DamageIndicator = lastSegment.DamageIndicator;
            _objectFactory.CreateSpawnEffect(segment.Body, SpawnEffectColor(_colorScheme));

            _spawnCounter++;
            return segment;
        }

        private static Color SpawnEffectColor(in ColorScheme colorScheme)
        {
            var color = colorScheme.Color;
            color.a *= 0.5f;
            return color;
        }
    }
}
