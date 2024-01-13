using System.Collections.Generic;

namespace Session
{
    public interface ISerializableDataObsolete
    {
        string FileName { get; }
        bool IsChanged { get; }
        IEnumerable<byte> Serialize();
    }
}
