using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EasyConsume.Infrastructure.Messaging
{
    public class DecompressingHttpContent : HttpContent
    {
        protected readonly Stream DecompressedStream;
        protected readonly StreamContent StreamContent;

        public DecompressingHttpContent(Stream content, HttpResponseMessage response)
        {
            foreach (var (key, value) in response.Content.Headers) // preserve response.Content.Headers
                Headers.Add(key, value);

            var compression = response.Content.Headers.ContentEncoding.FirstOrDefault() ?? string.Empty;
            switch (compression.ToLower())
            {
                case "gzip":
                    DecompressedStream = new GZipStream(content, CompressionMode.Decompress);
                    break;
                case "deflate":
                    DecompressedStream = new DeflateStream(new HeaderlessStream(content, 2), CompressionMode.Decompress);
                    break;
                case "br":
                    DecompressedStream = new BrotliStream(content, CompressionMode.Decompress);
                    break;
                case "":
                case "none":
                    DecompressedStream = content;
                    break;
                default:
                    throw new NotImplementedException($"Unsupported data encoding: {compression}");
            }
            StreamContent = new StreamContent(DecompressedStream);
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context) => StreamContent.CopyToAsync(stream, context);

        protected override Task<Stream> CreateContentReadStreamAsync() => StreamContent.ReadAsStreamAsync();

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposing) return;
                StreamContent.Dispose();
                DecompressedStream.Dispose();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
