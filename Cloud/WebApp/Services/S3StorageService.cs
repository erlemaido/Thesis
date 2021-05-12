using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;

namespace WebApp.Services
{
    public class S3StorageService
    {
        private readonly AmazonS3Client _s3Client;
        private const string BucketName = "cloud-photo-storage";
        private const string FolderName = "assets";
        private const double Duration = 24;

        public S3StorageService()
        {
            _s3Client = new AmazonS3Client(new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.USEast1,
                UseAccelerateEndpoint = true
            });
        }

        public async Task<string> AddItem(IFormFile file, string subFolderName)
        {
            var fileName = file.FileName;
            var objectKey = $"{FolderName}/{subFolderName}/{fileName}";
            
            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = BucketName,
                Key = objectKey,
                InputStream = ms,
                ContentType = file.ContentType
            };

            await _s3Client.PutObjectAsync(putObjectRequest);

            return GeneratePreSignedUrl(objectKey);
        }

        private string GeneratePreSignedUrl(string objectKey)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = BucketName,
                Key = objectKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddHours(Duration)
            };

            var url = _s3Client.GetPreSignedURL(request);
            return url;
        }
    }
}