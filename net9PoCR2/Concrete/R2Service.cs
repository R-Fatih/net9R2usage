using Amazon.S3;
using Amazon.S3.Model;
using net9PoCR2.Abstract;
using System.Text;

namespace net9PoCR2.Concrete
{

    public class R2Service : IR2Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<R2Service> _logger;

        public R2Service(IAmazonS3 s3Client, ILogger<R2Service> logger)
        {
            _s3Client = s3Client;
            _logger = logger;
        }

        public async Task<ListBucketsResponse> ListBucketsAsync()
        {
            try
            {
                _logger.LogInformation("Bucket listesi alınıyor...");

                var response = await _s3Client.ListBucketsAsync();

                _logger.LogInformation($"Toplam {response.Buckets.Count} bucket bulundu:");
               
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bucket listesi alınırken hata oluştu");
                throw;
            }
        }

        public async Task<string> UploadFileAsync(string bucketName, string key, string content)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            return await UploadFileAsync(bucketName, key, stream, "text/plain");
        }

        public async Task<string> UploadFileAsync(string bucketName, string key, Stream fileStream, string contentType = "application/octet-stream")
        {
            try
            {
                _logger.LogInformation($"Dosya yükleniyor: {key} -> {bucketName}");

                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    InputStream = fileStream,
                    ContentType = contentType,
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                };

                var response = await _s3Client.PutObjectAsync(request);

                _logger.LogInformation($"Dosya başarıyla yüklendi. ETag: {response.ETag}");
                return response.ETag;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya yüklenirken hata oluştu: {key}");
                throw;
            }
        }

        public async Task<string?> DownloadFileAsync(string bucketName, string key)
        {
            try
            {
                _logger.LogInformation($"Dosya indiriliyor: {key} <- {bucketName}");

                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = key
                };

                using var response = await _s3Client.GetObjectAsync(request);
                using var reader = new StreamReader(response.ResponseStream);

                var content = await reader.ReadToEndAsync();
                _logger.LogInformation($"Dosya başarıyla indirildi. Boyut: {content.Length} karakter");
                _logger.LogInformation($"İçerik: {content}");

                return content;
            }
            catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchKey")
            {
                _logger.LogWarning($"Dosya bulunamadı: {key}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya indirilirken hata oluştu: {key}");
                throw;
            }
        }

        public async Task<Stream?> DownloadFileStreamAsync(string bucketName, string key)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = key
                };

                var response = await _s3Client.GetObjectAsync(request);
                return response.ResponseStream;
            }
            catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchKey")
            {
                _logger.LogWarning($"Dosya bulunamadı: {key}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya stream alınırken hata oluştu: {key}");
                throw;
            }
        }

        public async Task<ListObjectsV2Response> ListObjectsAsync(string bucketName, string? prefix = null)
        {
            try
            {
                _logger.LogInformation($"Dosyalar listeleniyor: {bucketName}" + (prefix != null ? $" (prefix: {prefix})" : ""));

                var request = new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    Prefix = prefix,
                    MaxKeys = 100
                };

                var response = await _s3Client.ListObjectsV2Async(request);

                _logger.LogInformation($"Toplam {response.S3Objects.Count} dosya bulundu:");
              return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosyalar listelenirken hata oluştu: {bucketName}");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string bucketName, string key)
        {
            try
            {
                _logger.LogInformation($"Dosya siliniyor: {key} <- {bucketName}");

                var request = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(request);
                _logger.LogInformation($"Dosya başarıyla silindi: {key}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya silinirken hata oluştu: {key}");
                return false;
            }
        }

        public string GeneratePresignedUrl(string bucketName, string key, TimeSpan expiration)
        {
            try
            {
                _logger.LogInformation($"Presigned URL oluşturuluyor: {key} (süre: {expiration.TotalMinutes} dakika)");

                var request = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    Verb = HttpVerb.GET,
                    Expires = DateTime.UtcNow.Add(expiration)
                };

                var url =  _s3Client.GetPreSignedURL(request);
                _logger.LogInformation($"Presigned URL oluşturuldu: {url}");

                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Presigned URL oluşturulurken hata oluştu: {key}");
                throw;
            }
        }

        public async Task<bool> CheckIfObjectExistsAsync(string bucketName, string key)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = bucketName,
                    Key = key
                };

                await _s3Client.GetObjectMetadataAsync(request);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.ErrorCode == "NotFound")
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya varlığı kontrol edilirken hata oluştu: {key}");
                throw;
            }
        }
    }
}
