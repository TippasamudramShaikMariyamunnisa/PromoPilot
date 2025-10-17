using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PromoPilot.Application.Interfaces
{
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(IFormFile file);
        Task<Stream> DownloadFileAsync(string fileName);
        Task<List<string>> ListFilesAsync();
    }

}
