using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using net9PoCR2.Abstract;

namespace net9PoCR2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController(IR2Service r2Service) : ControllerBase
    {
        private readonly IR2Service _r2Service = r2Service;
        [HttpGet("buckets")]
        public async Task<IActionResult> ListBuckets()
        {
            
                var response = await _r2Service.ListBucketsAsync();
                return Ok(response.Buckets);
        }
        [HttpGet("buckets/{bucketName}/objects")]
        public async Task<IActionResult> ListObjects(string bucketName, string? prefix = null)
        {
           
              var objects=  await _r2Service.ListObjectsAsync(bucketName, prefix);
                return Ok(objects);
            
          
        }

    }
}
