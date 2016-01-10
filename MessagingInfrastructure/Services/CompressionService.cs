namespace MessagingInfrastructure.Services
{
    using System.IO;
    using System.IO.Compression;

    public class CompressionService : ICompressionService
    {
        public byte[] CompressWithGzip(byte[] input)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(input, 0, input.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        public byte[] DecompressWithGzip(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }
    }
}
