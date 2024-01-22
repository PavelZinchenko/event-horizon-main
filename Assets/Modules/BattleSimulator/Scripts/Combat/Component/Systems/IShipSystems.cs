using System;
using System.Collections.Generic;

namespace Combat.Component.Systems
{
    public interface IShipSystems : IDisposable
    {
		IReadOnlyList<ISystem> All { get; }

        SystemsModifications Modifications { get; }

        void UpdatePhysics(float elapsedTime);
        void UpdateView(float elapsedTime);
        void OnEvent(SystemEventType eventType);
    }
}
