using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ResearchManagement.Application.Interfaces;

namespace ResearchManagement.Infrastructure.Services
{

    public class FileService : IFileService
    {
        private readonly FileUploadSettings _settings;

        public FileService(IOptions<FileUploadSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<string> UploadFileAsync(byte[] fileContent, string fileName, string contentType)
        {
            // إنشاء اسم ملف فريد
            var fileExtension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

            // إنشاء مسار الملف
            var uploadPath = Path.Combine(_settings.UploadPath, DateTime.Now.Year.ToString(),
                DateTime.Now.Month.ToString());

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, uniqueFileName);

            // حفظ الملف
            await File.WriteAllBytesAsync(filePath, fileContent);

            // إرجاع المسار النسبي
            return Path.Combine(DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString(), uniqueFileName);
        }

        public async Task<byte[]> DownloadFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_settings.UploadPath, filePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("الملف غير موجود");

            return await File.ReadAllBytesAsync(fullPath);
        }

        public async Task DeleteFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_settings.UploadPath, filePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            await Task.CompletedTask;
        }

        public async Task<bool> FileExistsAsync(string filePath)
        {
            var fullPath = Path.Combine(_settings.UploadPath, filePath);
            return await Task.FromResult(File.Exists(fullPath));
        }

        public string GetFileUrl(string filePath)
        {
            return $"/files/{filePath.Replace('\\', '/')}";
        }
    }

    public class FileUploadSettings
    {
        public string UploadPath { get; set; } = "wwwroot/uploads";
        public long MaxFileSize { get; set; } = 50 * 1024 * 1024; // 50MB
        public string[] AllowedExtensions { get; set; } = { ".pdf", ".doc", ".docx" };
    }
}
