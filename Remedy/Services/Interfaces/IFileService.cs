namespace Remedy.Services.Interfaces
{
    public interface IFileService
    {
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);

        public string ConvertByteArrayToFile(byte[] fileData, string extension, int imageType);

        public string GetFileIcon(string file);

        public string FormatFileSize(long bytes);
    }
}
