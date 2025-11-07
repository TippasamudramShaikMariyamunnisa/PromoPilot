using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PromoPilot.Application.DTOs;

namespace PromoPilot.Application.DTOs
{
    public class FileUploadDto
    {
        [Required]
        public IFormFile File { get; set; }
    }
}
