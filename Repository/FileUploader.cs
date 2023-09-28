using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
using System.Net.Mime;
using VideoManagerAPI.Models;

namespace VideoManagerAPI.Repository
{
    public class FileUploader
    {
        private readonly IOptions<WasabiCredentials> _options;
        private readonly IResponseService _responseService;

        public FileUploader(IOptions<WasabiCredentials> options, IResponseService responseService)
        {
            _options = options;
            _responseService = responseService;

        }
        public async Task<APIResponse<bool>> UploadVideo(IFormFile file)
        {
            if(file == null) return _responseService.ErrorResponse<bool>("File cannot be empty");
           // if(file.ContentType == ContentType)
            string accessKey = _options.Value.AccessKey;
            string secretKey = _options.Value.SecretKey;
            string bucketName = _options.Value.BucketName;
            string endPoint = _options.Value.EndPoint;
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "/Waiting.jpg";
            if (!File.Exists(filePath))
                return _responseService.ErrorResponse<bool>("File Not Found");
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
            return _responseService.SuccessResponse(true);
        }
    }
}
