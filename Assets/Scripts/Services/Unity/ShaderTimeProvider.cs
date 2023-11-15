using UnityEngine;
using Zenject;

namespace Services.Unity
{
    public class ShaderTimeProvider : ITickable
    {
        public ShaderTimeProvider()
        {
            _propertyId = Shader.PropertyToID("_UnscaledTime");
        }

        public void Tick()
        {
            Shader.SetGlobalFloat(_propertyId, Time.unscaledTime);
        }

        private int _propertyId;
    }
}
