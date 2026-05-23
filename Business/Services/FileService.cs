using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Core.Abstracts.IServices;

namespace Business.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        
        // Whitelist safe file types to shield your server from dangerous scripts (.exe, .php, etc.)
        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private readonly string[] _allowedDocExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SavePublicImageAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("The uploaded file is empty.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedImageExtensions.Contains(extension))
                throw new InvalidDataException("Invalid image file format.");

            // Target the public wwwroot directory 
            string publicRoot = Path.Combine(_environment.WebRootPath, "uploads", folderName);
            
            if (!Directory.Exists(publicRoot))
                Directory.CreateDirectory(publicRoot);

            // Generate an unguessable unique filename to prevent overwriting files with the same name
            string uniqueFileName = $"{Guid.NewGuid()}{extension}";
            string fullPhysicalPath = Path.Combine(publicRoot, uniqueFileName);

            using (var stream = new FileStream(fullPhysicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the relative browser URL path (e.g., "/uploads/profiles/abc-123.jpg")
            return $"/uploads/{folderName}/{uniqueFileName}";
        }

        public async Task<string> SaveSecureDocumentAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("The uploaded file is empty.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedDocExtensions.Contains(extension))
                throw new InvalidDataException("Invalid document file format.");

            // CRITICAL: ContentRootPath lives outside wwwroot. It cannot be navigated to by standard browsers.
            string secureRoot = Path.Combine(_environment.ContentRootPath, "SecureStorage", folderName);

            if (!Directory.Exists(secureRoot))
                Directory.CreateDirectory(secureRoot);

            string uniqueFileName = $"{Guid.NewGuid()}{extension}";
            string fullPhysicalPath = Path.Combine(secureRoot, uniqueFileName);

            using (var stream = new FileStream(fullPhysicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the absolute disk file path. 
            // When an administrator checks the ID, your backend controller will read this path via code.
            return fullPhysicalPath;
        }
    }
}