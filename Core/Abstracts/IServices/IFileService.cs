using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Core.Abstracts.IServices
{
    public interface IFileService
    {
        /// <summary>
        /// Saves an image to the public wwwroot folder and returns its accessible relative web URL.
        /// </summary>
        Task<string> SavePublicImageAsync(IFormFile file, string folderName);

        /// <summary>
        /// Saves a sensitive document outside of public web access and returns the absolute path on the disk.
        /// </summary>
        Task<string> SaveSecureDocumentAsync(IFormFile file, string folderName);
    }
}