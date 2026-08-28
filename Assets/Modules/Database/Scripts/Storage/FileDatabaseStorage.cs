using System;
using System.IO;
using Ionic.Zlib;
using UnityEngine;
using CommonComponents.Serialization;

namespace GameDatabase.Storage
{
    public class FileDatabaseStorage : IDataStorage
    {
        private const uint _legacyHeader = 0xDA7ABA5E;
        private const uint _authenticatedHeader = 0xDADABA5E;
        private const int _authenticatedFormatVersion = 1;
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
                using (ReadDataTillContent(file)) { }
            }
        }

        public void UpdateItem(string id, string content)
        {
            throw new InvalidOperationException("FileDatabaseStorage.UpdateItem is not supported");
        }

        public void LoadContent(IContentLoader loader)
        {
            using var file = new FileStream(_filename, FileMode.Open, FileAccess.Read);
            using var content = ReadDataTillContent(file);

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
            if (file.Length - file.Position < sizeof(uint))
                throw new InvalidDataException("Invalid packed mod header.");

            var header = file.ReadUInt32();
            Stream content = null;
            try
            {
                var obsolete = false;
                if (header == _authenticatedHeader)
                {
                    content = UnpackAuthenticatedContent(file);
                }
                else if (header == _legacyHeader)
                {
                    content = UnpackLegacyContent(file);
                }
                else
                {
                    file.Seek(-sizeof(uint), SeekOrigin.Current);
                    content = UnpackLegacyContent(file);
                    obsolete = true;
                }

                if (obsolete)
                    LoadHeaderDataObsolete(content);
                else
                    LoadHeaderData(content);

                return content;
            }
            catch
            {
                content?.Dispose();
                throw;
            }
        }

        private static Stream UnpackLegacyContent(FileStream file)
        {
            var encryptedStream = new Security.EncryptedReadStream(file, checked((int)(file.Length - file.Position)));
            return new ZlibStream(encryptedStream, CompressionMode.Decompress);
        }

        private static Stream UnpackAuthenticatedContent(FileStream file)
        {
            const int fixedContentSize = sizeof(int)
                + Security.ModEncryption.WrappedKeySize
                + Security.ModEncryption.NonceSize
                + Security.ModEncryption.AuthenticationTagSize;
            if (file.Length - file.Position <= fixedContentSize)
                throw new InvalidDataException("The authenticated mod envelope is truncated.");

            var formatVersion = file.ReadInt32();
            if (formatVersion != _authenticatedFormatVersion)
                throw new InvalidDataException($"Unsupported packed mod format version: {formatVersion}.");

            var wrappedKey = new byte[Security.ModEncryption.WrappedKeySize];
            var nonce = new byte[Security.ModEncryption.NonceSize];
            var authenticationTag = new byte[Security.ModEncryption.AuthenticationTagSize];
            byte[] encryptedData = null;
            byte[] associatedData = null;

            try
            {
                ReadExactly(file, wrappedKey);
                ReadExactly(file, nonce);
                ReadExactly(file, authenticationTag);

                var encryptedLength = file.Length - file.Position;
                if (encryptedLength <= 0 || encryptedLength > int.MaxValue)
                    throw new InvalidDataException("The encrypted mod content has an invalid length.");
                encryptedData = new byte[(int)encryptedLength];
                ReadExactly(file, encryptedData);

                associatedData = CreateAssociatedData(formatVersion, wrappedKey, nonce);
                var compressedData = Security.ModEncryption.Decrypt(
                    wrappedKey,
                    nonce,
                    authenticationTag,
                    encryptedData,
                    associatedData);
                var compressedStream = new ClearingMemoryStream(compressedData);
                try
                {
                    return new ZlibStream(compressedStream, CompressionMode.Decompress);
                }
                catch
                {
                    compressedStream.Dispose();
                    throw;
                }
            }
            finally
            {
                Clear(wrappedKey);
                Clear(nonce);
                Clear(authenticationTag);
                Clear(encryptedData);
                Clear(associatedData);
            }
        }

        private static byte[] CreateAssociatedData(int formatVersion, byte[] wrappedKey, byte[] nonce)
        {
            var data = new byte[sizeof(uint) + sizeof(int) + wrappedKey.Length + nonce.Length];
            var offset = 0;
            WriteUInt32(data, ref offset, _authenticatedHeader);
            WriteUInt32(data, ref offset, unchecked((uint)formatVersion));
            Buffer.BlockCopy(wrappedKey, 0, data, offset, wrappedKey.Length);
            offset += wrappedKey.Length;
            Buffer.BlockCopy(nonce, 0, data, offset, nonce.Length);
            return data;
        }

        private static void WriteUInt32(byte[] destination, ref int offset, uint value)
        {
            destination[offset++] = (byte)value;
            destination[offset++] = (byte)(value >> 8);
            destination[offset++] = (byte)(value >> 16);
            destination[offset++] = (byte)(value >> 24);
        }

        private static void ReadExactly(Stream stream, byte[] buffer)
        {
            var offset = 0;
            while (offset < buffer.Length)
            {
                var count = stream.Read(buffer, offset, buffer.Length - offset);
                if (count <= 0)
                    throw new EndOfStreamException("The packed mod envelope is truncated.");
                offset += count;
            }
        }

        private static void Clear(byte[] data)
        {
            if (data != null)
                Array.Clear(data, 0, data.Length);
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

        private sealed class ClearingMemoryStream : MemoryStream
        {
            private readonly byte[] _buffer;

            public ClearingMemoryStream(byte[] buffer)
                : base(buffer, false)
            {
                _buffer = buffer;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    Clear(_buffer);
                base.Dispose(disposing);
            }
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
