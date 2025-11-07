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

        [HttpPost("Trigger-LogicApp")]
        public async Task<IActionResult> TriggerLogicApp([FromBody] ReportTriggerDto dto)
        {
            var logicAppUrl = "https://prod-27.eastasia.logic.azure.com:443/workflows/0a17c0d856984d87b88ac280fa704a07/triggers/When_an_HTTP_request_is_received/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2FWhen_an_HTTP_request_is_received%2Frun&sv=1.0&sig=3LYrdmRZUt2gY5JyiS07ZQeCFbiNEwjbrd7l1qxK7b8";

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsJsonAsync(logicAppUrl, dto);

            if (response.IsSuccessStatusCode)
                return Ok("Logic App triggered successfully.");
            else
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

    }
}