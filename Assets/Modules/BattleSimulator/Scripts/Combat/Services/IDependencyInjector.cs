using Combat.Services;

namespace Combat.Component.Helpers
{
    public interface IDependencyInjector
    {
        void Initialize(IGameServicesProvider services);
    }
}
