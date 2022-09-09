using Remedy.Services.Interfaces;

namespace Blog.Services
{
    public class FileService : IFileService
    {
        private readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };
        private readonly string _defaultCompanyImageSrc = "/img/DefaultCompanyImage.png";
        private readonly string _defaultProjectImageSrc = "/img/DefaultProjectImage.png";
        private readonly string _defaultProfileImageSrc = "/img/DefaultProfileImage.png";
        public string ConvertByteArrayToFile(byte[] fileData, string extension, int imageType)
        {

            if ((fileData == null || fileData.Length == 0) && imageType != null)
            {
                switch (imageType)
                {
                    case 1: return _defaultCompanyImageSrc;
                    case 2: return _defaultProjectImageSrc;
                    case 3: return _defaultProfileImageSrc;
                }
            }
            try
            {
                string ImageBase64Data = Convert.ToBase64String(fileData!);
                return string.Format($"data:{extension};base64, {ImageBase64Data}");
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                using MemoryStream memoryStream = new();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();

                return byteFile;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public string GetFileIcon(string file)
        {
            string ext = Path.GetExtension(file).Replace(".", "");
            return $"/img/contenttype/{ext}.png";
        }


        public string FormatFileSize(long bytes)
        {
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return string.Format("{0:n1}{1}", number, suffixes[counter]);
        }
    }
}
