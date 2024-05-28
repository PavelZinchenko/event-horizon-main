using System;
using System.Collections.Generic;

namespace Combat.Scene
{
    public interface IUnitList<T> where T : IDisposable
    {
        IReadOnlyList<T> Items { get; }
        object LockObject { get; }
    }
}
