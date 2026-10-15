using Session;
using UnityEngine;
using Zenject;

namespace Services.Unity
{
    public class SessionPlayTimeTicker : ITickable
    {
        private readonly SessionData _session;

        public SessionPlayTimeTicker(SessionData session)
        {
            _session = session;
        }

        public void Tick()
        {
            _session.AddPlayTime(Time.deltaTime);
        }
    }
}
