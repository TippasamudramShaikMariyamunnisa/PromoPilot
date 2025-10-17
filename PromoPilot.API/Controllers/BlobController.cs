using Microsoft.AspNetCore.Mvc;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;

namespace PromoPilot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlobController : ControllerBase
    {
        private readonly IBlobStorageService _blobStorageService;

        public BlobController(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        [HttpPost("Upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCampaignReport([FromForm] FileUploadDto dto)
        {
            var file = dto.File;

            if (file == null || file.Length == 0)
                return BadRequest("File is empty or missing.");

            var fileUrl = await _blobStorageService.UploadFileAsync(file);

            return Ok(new
            {
                FileName = file.FileName,
                DownloadUrl = $"{Request.Scheme}://{Request.Host}/api/blob/download/{file.FileName}"
            });
        }

        [HttpGet("Download/{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            var stream = await _blobStorageService.DownloadFileAsync(fileName);

            if (stream == null)
                return NotFound("File not found.");

            return File(stream, "application/octet-stream", fileName);
        }

        [HttpGet("List-Reports")]
        public async Task<IActionResult> ListCampaignReports()
        {
            var files = await _blobStorageService.ListFilesAsync();
            return Ok(files);
        }
    }
}