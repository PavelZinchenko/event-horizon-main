using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Constructor.Ships;
using GameDatabase;
using Session.ContentObsolete;
using Ionic.Zlib;
using Helpers = GameModel.Serialization.Helpers;

namespace GameServices.Multiplayer
{
    public static class FleetWebSerializer
    {
        // The arena upload. It keeps version 4 (two satellites per ship), so older clients can read it.
        public static string SerializeFleet(IEnumerable<IShip> ships)
        {
            var data = ZlibStream.CompressBuffer(SerializeFleetInternal(ships, ArenaVersion).ToArray());
            var base64 = EncodeUrlBase64(Convert.ToBase64String(data, Base64FormattingOptions.None));

            if (base64.Length > 16384)
            {
                UnityEngine.Debug.LogException(new OverflowException("SerializeFleet: Size is too big"));
                return string.Empty;
            }

            UnityEngine.Debug.LogWarning("SerializeFleet: " + base64.Length + " bytes");

            return base64;
        }

        // The compressed fleet without the URL text encoding (online duel handshake). Version 5 keeps every satellite.
        public static byte[] SerializeFleetData(IEnumerable<IShip> ships) =>
            ZlibStream.CompressBuffer(SerializeFleetInternal(ships, Version).ToArray());

        public static IEnumerable<IShip> DeserializeFleet(IDatabase database, string base64) =>
            DeserializeFleetData(database, System.Convert.FromBase64String(DecodeUrlBase64(base64)));

        public static IEnumerable<IShip> DeserializeFleetData(IDatabase database, byte[] data)
        {
            var index = 0;

            try
            {
                data = ZlibStream.UncompressBuffer(data);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("DeserializeFleet: " + e.Message);
            }

            var version = Helpers.DeserializeInt(data, ref index);
            if (version == 1)
                return DeserializeV1(database, data, index);
            if (version == 2)
                return DeserializeV2(database, data, index);
            if (version == 3)
                return DeserializeV3(database, data, index);
            if (version == 4 || version == 5)
                return DeserializeV4(database, data, index);

            return Enumerable.Empty<IShip>();
        }

        public static string EncodeUrlBase64(string base64)
        {
            var builder = new StringBuilder(base64);
            builder.Replace('+', '-');
            builder.Replace('/', '_');
            while (builder.Length > 0 && builder[builder.Length - 1] == '=')
                builder.Remove(builder.Length - 1, 1);

            return builder.ToString();
        }

        public static string DecodeUrlBase64(string urlBase64)
        {
            var builder = new StringBuilder(urlBase64);
            builder.Replace('-', '+');
            builder.Replace('_', '/');
            while (builder.Length % 4 != 0)
                builder.Append('=');

            return builder.ToString();
        }

        private static IEnumerable<IShip> DeserializeV1(IDatabase database, byte[] data, int index)
        {
            var count = Helpers.DeserializeInt(data, ref index);

            for (var i = 0; i < count; ++i)
            {
                var shipInfo = FleetData.ShipInfoV1.Deserialize(data, ref index);
                yield return ShipDataExtensions.FromShipInfoObsolete(database, shipInfo.ToShipInfo(database));
            }
        }

        private static IEnumerable<IShip> DeserializeV2(IDatabase database, byte[] data, int index)
        {
            var count = Helpers.DeserializeInt(data, ref index);

            for (var i = 0; i < count; ++i)
            {
                var shipInfo = FleetData.ShipInfoObsolete.DeserializeObsolete(data, ref index, database);
                yield return ShipDataExtensions.FromShipInfoObsolete(database, shipInfo);
            }
        }

        private static IEnumerable<IShip> DeserializeV3(IDatabase database, byte[] data, int index)
        {
            var count = Helpers.DeserializeInt(data, ref index);

            for (var i = 0; i < count; ++i)
            {
                var shipInfo = FleetData.ShipInfoObsolete.Deserialize(data, ref index);
                yield return ShipDataExtensions.FromShipInfoObsolete(database, shipInfo);
            }
        }

        private static IEnumerable<IShip> DeserializeV4(IDatabase database, byte[] data, int index)
        {
            var count = Helpers.DeserializeInt(data, ref index);

            for (var i = 0; i < count; ++i)
            {
                var shipInfo = ShipData.Deserialize(data, ref index);
                yield return ShipDataExtensions.FromShipData(database, shipInfo);
            }
        }

        // Version 4 writes ShipData version 1 (two satellites); version 5 writes ShipData version 2 (the satellite list).
        // Both read the same way, because ShipData stores its own version.
        private static IEnumerable<byte> SerializeFleetInternal(IEnumerable<IShip> ships, int version)
        {
            var shipVersion = version == ArenaVersion ? ShipData.TwoSatellitesVersion : ShipData.Version;

            foreach (var value in Helpers.Serialize(version))
                yield return value;
            foreach (var value in Helpers.Serialize(ships.Count()))
                yield return value;

            foreach (var ship in ships)
                foreach (var value in ship.ToShipData().Serialize(shipVersion))
                    yield return value;
        }

        private const int Version = 5;
        private const int ArenaVersion = 4;
    }
}
