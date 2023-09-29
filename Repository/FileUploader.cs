using Amazon.S3;
using Amazon.S3.Model;
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
        public async Task<APIResponse<string>> UploadVideo(IFormFile file)
        {
            var allowedFormats = new List<string>() { "video/mp4" };
            if(file == null) return _responseService.ErrorResponse<string>("File cannot be empty");
            if (!allowedFormats.Contains(file.ContentType)) return _responseService.ErrorResponse<string>("File has to be in video format");
            var fileSizeInMb = file.Length / (1024.0 * 1024.0);
            if(fileSizeInMb > 50.0) return _responseService.ErrorResponse<string>("You can only upload videos less than or equal to 50 MB");
            string accessKey = _options.Value.AccessKey;
            string secretKey = _options.Value.SecretKey;
            string bucketName = _options.Value.BucketName;
            string endPoint = _options.Value.EndPoint;
            using var s3Client = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config()
            {
                 ServiceURL = endPoint,
                 ForcePathStyle = true,
            });
            using (var fileStream = file.OpenReadStream())
            {
                var guid = Guid.NewGuid().ToString();
                var fileTransferUtility = new TransferUtility(s3Client);
                await fileTransferUtility.UploadAsync(fileStream, bucketName, (guid+file.FileName));
                Console.WriteLine("File upload completed successfully.");
            }
            var url = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = file.FileName,
                Expires = DateTime.UtcNow.AddHours(12),
            };
            string preSignedUrl = s3Client.GetPreSignedURL(url);
            return _responseService.SuccessResponse(preSignedUrl);
        }
    }
}
