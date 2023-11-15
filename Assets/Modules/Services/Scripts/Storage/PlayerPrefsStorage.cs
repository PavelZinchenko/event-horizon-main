using Ionic.Zlib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using UnityEngine;
using GameModel.Serialization;

namespace Services.Storage
{
    public class PlayerPrefsStorage : IDataStorage
    {
        public bool TryLoad(ISerializableGameData gameData, string mod)
        {
            return TryLoad(mod, out var data) && TryDeserialize(data, gameData, mod);
        }

        public void Save(ISerializableGameData gameData)
        {
            try
            {
                if (_currentGameId == gameData.GameId && _currentVersion == gameData.DataVersion)
                {
                    UnityEngine.Debug.Log("PlayerPrefsStorage.Save: Game data not changed: " + gameData.GameId + "/" + gameData.DataVersion);
                    return;
                }

                UnityEngine.Debug.Log("PlayerPrefsStorage.Save: saving data " + gameData.GameId + "/" + gameData.DataVersion);

                var data = new List<byte>();

                data.AddRange(Helpers.Serialize(_formatId));
                data.AddRange(Helpers.Serialize(gameData.GameId));
                data.AddRange(Helpers.Serialize(gameData.TimePlayed));
                data.AddRange(Helpers.Serialize(gameData.DataVersion));
                data.AddRange(Helpers.Serialize(AppConfig.version));
                data.AddRange(ZlibStream.CompressBuffer(gameData.Serialize().ToArray()));

                var serializedData = Convert.ToBase64String(data.ToArray(), Base64FormattingOptions.None);
                PlayerPrefs.SetString(_key, serializedData);
                PlayerPrefs.Save();

                _currentGameId = gameData.GameId;
                _currentVersion = gameData.DataVersion;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
            }
        }

        public bool TryImportOriginalSave(ISerializableGameData gameData, string mod)
        {
            if (string.IsNullOrEmpty(mod))
                return false;

            return TryLoad(null, out var data) && TryDeserialize(data, gameData, mod);
        }

        private bool TryLoad(string mod, out byte[] data)
        {
            try
            {
                var key = string.IsNullOrEmpty(mod) ? _key : _key + "." + mod;
                var dataString = PlayerPrefs.GetString(key);

                if (string.IsNullOrEmpty(dataString))
                {
                    data = null;
                    return false;
                }

                data = System.Convert.FromBase64String(dataString);
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
                data = null;
                return false;
            }
        }

        private bool TryDeserialize(byte[] serializedData, ISerializableGameData gameData, string mod)
        {
            try
            {
                _currentGameId = -1;

                var size = (uint)(serializedData.Length - 1);

                int index = 0;
                var formatId = Helpers.DeserializeInt(serializedData, ref index);
                var gameId = Helpers.DeserializeLong(serializedData, ref index);
                var time = Helpers.DeserializeLong(serializedData, ref index);
                var version = Helpers.DeserializeLong(serializedData, ref index);
                var gameVersion = Helpers.DeserializeString(serializedData, ref index);

                if (!gameData.TryDeserialize(gameId, time, version, mod, ZlibStream.UncompressBuffer(serializedData.Skip(index).ToArray()), 0))
                {
                    Debug.LogException(new IOException("PlayerPrefsStorage.Load: Data deserialization failed"));
                    return false;
                }

                _currentGameId = gameId;
                _currentVersion = version;

                UnityEngine.Debug.Log("PlayerPrefsStorage.TryDeserializeData: done - " + gameData.GameId);

                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
                return false;
            }
        }

        private long _currentGameId;
        private long _currentVersion;
        
        private readonly string _key = "savegame";

        private const int _formatId = 3;
    }
}
