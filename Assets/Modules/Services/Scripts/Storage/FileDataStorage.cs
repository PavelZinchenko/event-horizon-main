using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameModel.Serialization;
using Ionic.Zlib;
using UnityEngine;

namespace Services.Storage
{
    /// <summary>
    /// File-based save storage. Saves are written to "SavesDir" next to the game
    /// executable (or persistentDataPath on Android). One file per mod:
    ///   savegame                  -> vanilla
    ///   savegame.<modId>          -> mod-specific
    /// </summary>
    public class FileDataStorage : IDataStorage
    {
        public void Save(ISerializableGameData gameData)
        {
            try
            {
                var data = new List<byte>();

                data.AddRange(Helpers.Serialize(_formatId));
                data.AddRange(Helpers.Serialize(gameData.GameId));
                data.AddRange(Helpers.Serialize(gameData.TimePlayed));
                data.AddRange(Helpers.Serialize(gameData.DataVersion));
                data.AddRange(Helpers.Serialize(Application.version));
                data.AddRange(Helpers.Serialize(GetModId(gameData)));
                data.AddRange(gameData.Serialize()); // raw session bytes (no extra compression)

                var size = (uint)data.Count;
                byte checksumm = 0;

                uint w = 0x12345678 ^ size;
                uint z = 0x87654321 ^ size;

                for (int i = 0; i < size; ++i)
                {
                    checksumm += data[i];
                    data[i] = (byte)(data[i] ^ (byte)Random(ref w, ref z));
                }

                data.Add((byte)(checksumm ^ (byte)Random(ref w, ref z)));

                var dir = GetBasePath();
                Directory.CreateDirectory(dir);

                var modId = GetModId(gameData);
                var path = GetSavePath(dir, modId);

                File.WriteAllBytes(path, data.ToArray());
                Debug.Log($"FileDataStorage.Save: saved to {path}");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public bool TryLoad(ISerializableGameData gameData, string mod)
        {
            var dir = GetBasePath();
            var path = GetSavePath(dir, mod);
            if (!File.Exists(path))
                return false;

            try
            {
                var data = File.ReadAllBytes(path);
                return TryDeserialize(data, gameData, mod);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public bool TryImportOriginalSave(ISerializableGameData gameData, string mod)
        {
            // Load vanilla save (no mod id) and deserialize into the provided mod id.
            var dir = GetBasePath();
            var path = GetSavePath(dir, string.Empty);
            if (!File.Exists(path))
                return false;

            try
            {
                var data = File.ReadAllBytes(path);
                return TryDeserialize(data, gameData, mod);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        private static bool TryDeserialize(byte[] serializedData, ISerializableGameData gameData, string mod)
        {
            try
            {
                if (serializedData.Length < 5) return false;

                // decrypt + checksum
                var size = (uint)(serializedData.Length - 1);
                uint w = 0x12345678 ^ size;
                uint z = 0x87654321 ^ size;
                byte checksum = 0;

                for (int i = 0; i < size; ++i)
                {
                    serializedData[i] = (byte)(serializedData[i] ^ (byte)Random(ref w, ref z));
                    checksum += serializedData[i];
                }

                var expected = (byte)(checksum ^ (byte)Random(ref w, ref z));
                if (expected != serializedData[serializedData.Length - 1])
                {
                    Debug.LogWarning("FileDataStorage: checksum mismatch");
                }

                int index = 0;
                var formatId = Helpers.DeserializeInt(serializedData, ref index);
                var gameId = Helpers.DeserializeLong(serializedData, ref index);
                var time = Helpers.DeserializeLong(serializedData, ref index);
                var version = Helpers.DeserializeLong(serializedData, ref index);
                var gameVersion = Helpers.DeserializeString(serializedData, ref index);
                var modInFile = Helpers.DeserializeString(serializedData, ref index);

                if (!IsModMatch(modInFile, mod))
                {
                    Debug.LogWarning($"FileDataStorage: mod mismatch (file '{modInFile}', expected '{mod}')");
                    return false;
                }

                var payload = serializedData.Skip(index).ToArray();
                // Backward compatibility: old saves we wrote were compressed.
                if (payload.Length >= 2 && payload[0] == 0x78 && payload[1] == 0xDA)
                {
                    try
                    {
                        payload = ZlibStream.UncompressBuffer(payload);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        return false;
                    }
                }

                if (!gameData.TryDeserialize(gameId, time, version, mod, payload, 0))
                {
                    Debug.LogException(new IOException("FileDataStorage: Data deserialization failed"));
                    return false;
                }

                Debug.Log($"FileDataStorage: loaded save {gameId} (mod '{mod}')");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        private static string GetModId(ISerializableGameData gameData)
        {
            // Avoid hard reference to Session assembly; reflectively read ModId if present.
            var prop = gameData.GetType().GetProperty("ModId");
            if (prop != null && prop.PropertyType == typeof(string))
                return prop.GetValue(gameData) as string ?? string.Empty;
            return string.Empty;
        }

        private static string GetBasePath()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Path.Combine(Application.persistentDataPath, "SavesDir");
#else
            // Game directory: one level above Application.dataPath (same pattern as Mods).
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "SavesDir"));
#endif
        }

        private static string GetSavePath(string baseDir, string modId)
        {
            // Match author build: vanilla save is "savegame"; mod saves are just "<modId>" (no prefix/suffix).
            if (string.IsNullOrEmpty(modId))
                return Path.Combine(baseDir, "savegame");
            return Path.Combine(baseDir, modId);
        }

        private static bool IsModMatch(string fileMod, string currentMod)
        {
            if (string.IsNullOrEmpty(fileMod))
                return string.IsNullOrEmpty(currentMod);
            if (string.IsNullOrEmpty(currentMod))
                return string.IsNullOrEmpty(fileMod);
            return string.Equals(fileMod, currentMod, StringComparison.OrdinalIgnoreCase);
        }

        private static uint Random(ref uint w, ref uint z)
        {
            z = 36969 * (z & 65535) + (z >> 16);
            w = 18000 * (w & 65535) + (w >> 16);
            return (z << 16) + w;  /* 32-bit result */
        }

        private const int _formatId = 3;
    }
}
