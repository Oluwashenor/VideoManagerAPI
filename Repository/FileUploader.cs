using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
using VideoManagerAPI.Models;

namespace VideoManagerAPI.Repository
{
    public class FileUploader
    {
        private readonly IOptions<WasabiCredentials> _options;

        public FileUploader(IOptions<WasabiCredentials> options)
        {
            _options = options;
        }
        public async Task<bool> UploadVideo()
        {
            string accessKey = _options.Value.AccessKey;
            string secretKey = _options.Value.SecretKey;
            string bucketName = _options.Value.BucketName;
            string endPoint = _options.Value.EndPoint;
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "/Waiting.jpg";
            if (!File.Exists(filePath))
                return false;
            using var s3Client = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config()
            {
                 ServiceURL = endPoint,
                 ForcePathStyle = true,
            });
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                // Create a transfer utility
                var fileTransferUtility = new TransferUtility(s3Client);
                // Upload the file to Wasabi
                await fileTransferUtility.UploadAsync(fileStream, bucketName, Path.GetFileName(filePath));

                Console.WriteLine("File upload completed successfully.");
            }
            return true;
        }
    }
}
