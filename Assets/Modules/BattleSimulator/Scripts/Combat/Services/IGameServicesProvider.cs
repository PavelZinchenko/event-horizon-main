using Combat.Collision.Manager;
using Services.Audio;
using Services.ObjectPool;
using Services.Resources;

namespace Combat.Services
{
    public interface IGameServicesProvider
    {
        IResourceLocator ResourceLocator { get; }
        TrailRendererPool TrailRendererPool { get; }
        ICollisionManager CollisionManager { get; }
        ISoundPlayer SoundPlayer { get; }
        IObjectPool ObjectPool { get; }
        PrefabCache PrefabCache { get; }
    }

    public class GameServicesProvider : IGameServicesProvider
    {
        public IResourceLocator ResourceLocator { get; }
        public TrailRendererPool TrailRendererPool { get; }
        public ICollisionManager CollisionManager { get; }
        public ISoundPlayer SoundPlayer { get; }
        public IObjectPool ObjectPool { get; }
        public PrefabCache PrefabCache { get; }

        public GameServicesProvider(
            IResourceLocator resourceLocator, 
            TrailRendererPool trailRendererPool,
            ICollisionManager collisionManager,
            ISoundPlayer soundPlayer,
            IObjectPool objectPool,
            PrefabCache prefabCache)
        {
            ResourceLocator = resourceLocator;
            TrailRendererPool = trailRendererPool;
            CollisionManager = collisionManager;
            SoundPlayer = soundPlayer;
            ObjectPool = objectPool;
            PrefabCache = prefabCache;
        }        
    }
}
