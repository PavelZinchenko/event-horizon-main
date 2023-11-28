using System;
using System.Collections.Generic;
using System.Text;
using GameDatabase;
using GameDatabase.DataModel;
using Helpers = GameModel.Serialization.Helpers;

namespace Constructor
{
	public class IntegratedComponent
	{
		public IntegratedComponent(ComponentInfo component, int x, int y, int barrelId, int keyBinding, int behaviour, bool locked)
		{
			Info = component;
			X = x;
			Y = y;
			BarrelId = barrelId;
		    KeyBinding = keyBinding;
		    Behaviour = behaviour;
		    Locked = locked;
		}

		public override string ToString() { throw new NotSupportedException(); }

		public int KeyBinding { get; set; }
		public bool Locked { get; set; }
        public int Behaviour { get; set; }

		public readonly ComponentInfo Info;
		public readonly int X;
		public readonly int Y;
		public readonly int BarrelId;
	}

    public static class ComponentExtensions
    {
#region Obsolete

        public static IEnumerable<byte> Serialize(this IntegratedComponent component)
        {
            foreach (var value in Helpers.Serialize(component.Info.SerializeToInt64()))
                yield return value;

            yield return (byte)component.X;
            yield return (byte)component.Y;
            yield return (byte)component.BarrelId;
            yield return (byte)component.KeyBinding;
            yield return (byte)component.Behaviour;
            yield return component.Locked ? (byte)1 : (byte)0;
        }

        public static IntegratedComponent Deserialize(IDatabase database, byte[] data/*, ref int index*/)
        {
            var index = 0;
            var component = ComponentInfo.FromInt64(database, Helpers.DeserializeLong(data, ref index));
            var x = (sbyte)data[index++];
            var y = (sbyte)data[index++];
            var barrelId = (sbyte)data[index++];
            var keyBinding = (sbyte)data[index++];
            var mode = (sbyte)data[index++];
            var locked = data[index++] > 0;

            return new IntegratedComponent(component, x, y, barrelId, keyBinding, mode, locked);
        }

        public static IEnumerable<byte> SerializeObsolete(this IntegratedComponent component)
        {
            foreach (var value in Helpers.Serialize(component.Info.SerializeToInt32Obsolete()))
                yield return value;

            yield return (byte)component.X;
            yield return (byte)component.Y;
            yield return (byte)component.BarrelId;
            yield return (byte)component.KeyBinding;
            yield return (byte)component.Behaviour;
            yield return component.Locked ? (byte)1 : (byte)0;
        }

        public static IntegratedComponent DeserializeObsolete(IDatabase database, byte[] data/*, ref int index*/)
        {
            var index = 0;
            var component = ComponentInfo.FromInt32Obsolete(database, Helpers.DeserializeInt(data, ref index));
            var x = (sbyte)data[index++];
            var y = (sbyte)data[index++];
            var barrelId = (sbyte)data[index++];
            var keyBinding = (sbyte)data[index++];
            var mode = (sbyte)data[index++];
            var locked = data[index++] > 0;

            return new IntegratedComponent(component, x, y, barrelId, keyBinding, mode, locked);
        }

        public static long SerializeToInt64Obsolete(this IntegratedComponent component) // deprecated
        {
            long value = component.Info.SerializeToInt32Obsolete();
            value <<= 8;
            value += (byte)component.X;
            value <<= 8;
            value += (byte)component.Y;
            value <<= 8;
            value += (byte)component.BarrelId;
            value <<= 8;
            value += (byte)component.KeyBinding;
            value <<= 1;
            value += component.Locked ? 1 : 0;
            return value;
        }

        public static IntegratedComponent DeserializeFromInt64Obsolete(IDatabase database, long data)
        {
            var locked = (data & 1) != 0;
            data >>= 1;
            var keyBinding = (sbyte)data;
            data >>= 8;
            var barrelId = (sbyte)data;
            data >>= 8;
            var y = (sbyte)data;
            data >>= 8;
            var x = (sbyte)data;
            data >>= 8;
            var component = ComponentInfo.FromInt32Obsolete(database, (int)data);

            return new IntegratedComponent(component, x, y, barrelId, keyBinding, 0, locked);
        }

#endregion

        public static IntegratedComponent FromDatabase(InstalledComponent serializable)
        {
            var info = new ComponentInfo(serializable.Component, serializable.Modification, serializable.Quality);
            var component = new IntegratedComponent(info, serializable.X, serializable.Y, serializable.BarrelId, serializable.KeyBinding, serializable.Behaviour, true);

            return component;
        }

        private const char _separator = '/';
    }
}
