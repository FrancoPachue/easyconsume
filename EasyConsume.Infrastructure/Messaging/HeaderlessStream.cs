using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyConsume.Infrastructure.Messaging
{
    public class HeaderlessStream : Stream
    {
        private int _headerBytesRead = 0;
        private readonly byte[] _buffer;
        private readonly Stream _stream;
        private readonly int _headerBytes;

        public HeaderlessStream(Stream stream, int skipBytes)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            if (skipBytes < 0) throw new ArgumentOutOfRangeException(nameof(skipBytes), "headerLength must be positive.");
            _headerBytes = skipBytes;
            _buffer = new byte[skipBytes];
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // Strip header from the stream
            while (_headerBytesRead < _headerBytes)
            {
                var headerBytesToRead = _headerBytes - _headerBytesRead;
                var bytesRead = _stream.Read(_buffer, 0, headerBytesToRead);
                _headerBytesRead += bytesRead;
            }

            return _stream.Read(buffer, offset, count);
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => _stream.CanSeek;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length => _stream.Length;
        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }
    }
}
