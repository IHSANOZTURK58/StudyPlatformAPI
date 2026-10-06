using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace StudyPlatformAPI.Services;

public interface IFileService
{
    Task<string> UploadFileAsync(IFormFile file);

    Task<string> GetFileUrlAsync(string fileName);
}