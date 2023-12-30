using GameServices.SceneManager;

namespace Services.Messenger
{
    public interface IMessengerContext
    {
        void AddListener(EventType eventType, GameScene scene, Callback handler);
        void AddListener<T>(EventType eventType, GameScene scene, Callback<T> handler);
        void AddListener<T, U>(EventType eventType, GameScene scene, Callback<T, U> handler);
        void AddListener<T, U, V>(EventType eventType, GameScene scene, Callback<T, U, V> handler);

        void Broadcast(EventType eventType);
        void Broadcast<T>(EventType eventType, T arg1);
        void Broadcast<T, U>(EventType eventType, T arg1, U arg2);
        void Broadcast<T, U, V>(EventType eventType, T arg1, U arg2, V arg3);

        void Cleanup(GameScene scene);
    }
}
