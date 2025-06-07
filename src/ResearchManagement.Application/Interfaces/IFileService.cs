using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchManagement.Application.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(byte[] fileContent, string fileName, string contentType);
        Task<byte[]> DownloadFileAsync(string filePath);
        Task DeleteFileAsync(string filePath);
        Task<bool> FileExistsAsync(string filePath);
        string GetFileUrl(string filePath);
    }
}