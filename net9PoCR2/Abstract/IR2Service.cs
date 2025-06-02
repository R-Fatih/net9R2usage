using Amazon.S3.Model;

namespace net9PoCR2.Abstract
{
    public interface IR2Service
    {
        Task<ListBucketsResponse> ListBucketsAsync();
        Task<string> UploadFileAsync(string bucketName, string key, string content);
        Task<string> UploadFileAsync(string bucketName, string key, Stream fileStream, string contentType = "application/octet-stream");
        Task<string?> DownloadFileAsync(string bucketName, string key);
        Task<Stream?> DownloadFileStreamAsync(string bucketName, string key);
        Task<ListObjectsV2Response> ListObjectsAsync(string bucketName, string? prefix = null);
        Task<bool> DeleteFileAsync(string bucketName, string key);
        string GeneratePresignedUrl(string bucketName, string key, TimeSpan expiration);
        Task<bool> CheckIfObjectExistsAsync(string bucketName, string key);
    }
}
