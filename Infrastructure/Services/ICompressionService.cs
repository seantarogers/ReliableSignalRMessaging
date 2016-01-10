namespace Infrastructure.Services
{
    public interface ICompressionService
    {
        byte[] CompressWithGzip(byte[] input);

        byte[] DecompressWithGzip(byte[] data);
    }
}