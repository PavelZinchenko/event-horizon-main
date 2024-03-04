using System;
using System.IO;
using Ionic.Zlib;
using UnityEngine;
using CommonComponents.Serialization;

namespace GameDatabase.Storage
{
    public class FileDatabaseStorage : IDataStorage
    {
        private const uint _header = 0xDA7ABA5E;
        private readonly string _filename;

        public string Name { get; private set; }
        public string Id { get; private set; }
        public Version Version { get; private set; }
        public bool IsEditable => false;

        public FileDatabaseStorage(string filename)
        {
            _filename = filename;

            using (var file = new FileStream(_filename, FileMode.Open, FileAccess.Read))
            {
                ReadDataTillContent(file);
            }
        }

        public void UpdateItem(string id, string content)
        {
            throw new InvalidOperationException("FileDatabaseStorage.UpdateItem is not supported");
        }

        public void LoadContent(IContentLoader loader)
        {
            using var file = new FileStream(_filename, FileMode.Open, FileAccess.Read);
            var content = ReadDataTillContent(file);

            var itemCount = 0;
            while (true)
            {
                var type = content.ReadByte();
                if (type == -1) // end of file
                {
                    break;
                }
                else if (type == 1) // json
                {
                    var fileContent = content.ReadString();

                    try
                    {
                        loader.LoadJson(string.Empty, fileContent);
                        itemCount++;
                    }
                    catch (Exception e)
                    {
                        // Skip files with errors to allow loading mods created with differen database version
                        Debug.LogException(e);
                    }
                }
                else if (type == 2) // image
                {
                    var key = content.ReadString();
                    var image = content.ReadByteArray();
                    loader.LoadImage(key, new LazyImageDataLoader(image));
                }
                else if (type == 3) // localization
                {
                    var key = content.ReadString();
                    var text = content.ReadString();
                    loader.LoadLocalization(key, text);
                }
                else if (type == 4) // wav audioClip
                {
                    var key = content.ReadString();
                    var audioClip = content.ReadByteArray();
                    loader.LoadAudioClip(key, new LazyAudioDataLoader(audioClip, LazyAudioDataLoader.Format.Wav));
                }
                else if (type == 5) // ogg audioClip
                {
                    var key = content.ReadString();
                    var audioClip = content.ReadByteArray();
                    loader.LoadAudioClip(key, new LazyAudioDataLoader(audioClip, LazyAudioDataLoader.Format.Ogg));
                }
            }

            if (itemCount == 0)
                throw new FileNotFoundException("Invalid database - ", Name);
        }

        private Stream ReadDataTillContent(FileStream file)
        {
            var obsolete = !TryReadHeader(file);
            var content = UnpackContent(file);

            if (obsolete)
                LoadHeaderDataObsolete(content);
            else
                LoadHeaderData(content);

            return content;
        }

        private static bool TryReadHeader(FileStream file)
        {
            var position = file.Position;
            var header = file.ReadUInt32();
            if (header != _header)
            {
                file.Seek(position, SeekOrigin.Begin);
                return false;
            }

            return true;
        }

        private static Stream UnpackContent(FileStream file)
        {
            var encryptedStream = new Security.EncryptedReadStream(file, (int)(file.Length - file.Position));
            var zlibStream = new ZlibStream(encryptedStream, CompressionMode.Decompress);
            return zlibStream;
        }

        private void LoadHeaderData(Stream stream)
        {
            var formatId = stream.ReadInt32();
            Name = stream.ReadString();
            Id = stream.ReadString();

            var major = stream.ReadInt32();
            var minor = stream.ReadInt32();

            Version = new Version(major, minor);
        }

        private void LoadHeaderDataObsolete(Stream stream)
        {
            Name = stream.ReadString();
            Id = stream.ReadString();

            Version = new Version(1, 0);
        }
    }

    public class LazyImageDataLoader : Model.IImageData
    {
        private byte[] _rawData;
        private Model.ImageData _imageData;

        public Sprite Sprite
        {
            get
            {
                if (_imageData == null)
                {
                    _imageData = new(_rawData);
                    _rawData = null;
                }

                return _imageData.Sprite;
            }
        }

        public LazyImageDataLoader(byte[] rawData)
        {
            _rawData = rawData;
        }
    }

    public class LazyAudioDataLoader : Model.IAudioClipData
    {
        public enum Format
        {
            Wav,
            Ogg,
        }

        private readonly Format _format;
        private byte[] _rawData;
        private Model.IAudioClipData _audioClipData;

        public AudioClip AudioClip
        {
            get
            {
                if (_audioClipData == null)
                {
                    switch (_format)
                    {
                        case Format.Wav:
                            _audioClipData = new Model.AudioClipData(_rawData);
                            break;
                        case Format.Ogg:
                            _audioClipData = Model.OggAudioClip.Create(_rawData);
                            break;
                        default:
                            throw new InvalidOperationException();
                    }

                    _rawData = null;
                }

                return _audioClipData.AudioClip;
            }
        }

        public LazyAudioDataLoader(byte[] rawData, Format format)
        {
            _rawData = rawData;
            _format = format;
        }
    }
}
