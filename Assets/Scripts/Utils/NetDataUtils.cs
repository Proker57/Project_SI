using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace BOYAREngine.Net
{
    public abstract class NetDataUtils
    {
        public static byte[] Compress(byte[] data)
        {
            var output = new MemoryStream();
            using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                stream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static byte[] Decompress(byte[] data)
        {
            var input = new MemoryStream(data);
            var output = new MemoryStream();
            using (var stream = new DeflateStream(input, CompressionMode.Decompress))
            {
                stream.CopyTo(output);
            }
            return output.ToArray();
        }

        public static byte[] CompressGZip(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        public static byte[] DecompressGZip(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }

        public static IEnumerable<byte[]> SplitArrayToChunks(byte[] value, int bufferLength)
        {
            var countOfArray = value.Length / bufferLength;
            if (value.Length % bufferLength > 0)
                countOfArray++;
            for (var i = 0; i < countOfArray; i++)
            {
                yield return value.Skip(i * bufferLength).Take(bufferLength).ToArray();

            }
        }
    }
}

