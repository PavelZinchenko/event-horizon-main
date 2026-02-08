using System;
using System.IO;
using UnityEngine;

namespace Security
{
    /// <summary>
    /// Decrypts mod files produced by ModBuilder (see EventHorizon-Editor/GameDatabase/ModBuilder.cs).
    /// Layout: [magic 0xDA7ABA5E][zlib-compressed bytes XOR'ed with PRNG][checksum XOR last PRNG byte].
    /// </summary>
    public class EncryptedReadStream : Stream
    {
        private readonly byte[] _buffer;
        private readonly int _length;
        private int _position;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _length;

        public EncryptedReadStream(Stream stream, int length)
        {
            // length is (file.Length - file.Position) when called from FileDatabaseStorage
            // and includes the trailing checksum byte.
            if (length <= 1) throw new InvalidDataException("Encrypted stream too small");

            _length = length - 1; // exclude checksum byte
            _buffer = new byte[_length];

            var read = 0;
            while (read < _length)
            {
                var n = stream.Read(_buffer, read, _length - read);
                if (n == 0) throw new EndOfStreamException();
                read += n;
            }

            var checksumByte = stream.ReadByte();
            if (checksumByte < 0) throw new EndOfStreamException();

            DecryptInPlace(_buffer, (byte)checksumByte);
            _position = 0;
        }

        public override int ReadByte()
        {
            if (_position >= _length) return -1;
            return _buffer[_position++];
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || offset + count > buffer.Length) throw new ArgumentOutOfRangeException();
            if (_position >= _length) return 0;

            var toCopy = Math.Min(count, _length - _position);
            Buffer.BlockCopy(_buffer, _position, buffer, offset, toCopy);
            _position += toCopy;
            return toCopy;
        }

        public override long Position
        {
            get => _position;
            set
            {
                if (value < 0 || value > _length) throw new ArgumentOutOfRangeException(nameof(value));
                _position = (int)value;
            }
        }

        public override void Flush() { /* read-only */ }

        public override long Seek(long offset, SeekOrigin origin)
        {
            int newPos = origin switch
            {
                SeekOrigin.Begin => (int)offset,
                SeekOrigin.Current => _position + (int)offset,
                SeekOrigin.End => _length + (int)offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };

            if (newPos < 0 || newPos > _length) throw new IOException("Seek out of range");
            _position = newPos;
            return _position;
        }

        public override void SetLength(long value) => throw new InvalidOperationException();
        public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException();

        private static void DecryptInPlace(byte[] data, byte checksumByte)
        {
            uint size = (uint)data.Length;
            uint w = 0x12345678 ^ size;
            uint z = 0x87654321 ^ size;

            byte checksum = 0;
            for (int i = 0; i < data.Length; ++i)
            {
                checksum += data[i];
                data[i] ^= (byte)Next(ref w, ref z);
            }

            var expected = (byte)(checksumByte ^ (byte)Next(ref w, ref z));
            if (checksum != expected)
                Debug.LogWarning($"Mod decrypt checksum mismatch (got {checksum}, expected {expected})");
        }

        private static uint Next(ref uint w, ref uint z)
        {
            z = 36969 * (z & 65535) + (z >> 16);
            w = 18000 * (w & 65535) + (w >> 16);
            return (z << 16) + w; // 32-bit result
        }
    }
}
