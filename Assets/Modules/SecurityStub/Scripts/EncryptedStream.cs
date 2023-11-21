using System.IO;

namespace Security
{
    public class EncryptedReadStream : Stream
    {
        private readonly Stream _stream;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _stream.Length;

        public EncryptedReadStream(Stream stream, int length)
        {
            _stream = stream;
        }

        public override int ReadByte()
        {
            return _stream.ReadByte();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Position
        {
            get => _stream.Position;
            set => throw new System.InvalidOperationException();
        }

        public override void Flush() => throw new System.InvalidOperationException();
        public override long Seek(long offset, SeekOrigin origin) => throw new System.InvalidOperationException();
        public override void SetLength(long value) => throw new System.InvalidOperationException();
        public override void Write(byte[] buffer, int offset, int count) => throw new System.InvalidOperationException();
    }
}
