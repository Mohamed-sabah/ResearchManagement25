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
            try
            {
                // إنشاء اسم ملف فريد
                var fileExtension = Path.GetExtension(fileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                // إنشاء مسار الملف بناءً على التاريخ
                var currentDate = DateTime.Now;
                var uploadPath = Path.Combine(_settings.UploadPath,
                    currentDate.Year.ToString(),
                    currentDate.Month.ToString("D2")); // استخدام D2 للحصول على رقمين

                // التأكد من وجود المجلد
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath = Path.Combine(uploadPath, uniqueFileName);

                // حفظ الملف
                await File.WriteAllBytesAsync(filePath, fileContent);

                // إرجاع المسار النسبي
                return Path.Combine(currentDate.Year.ToString(),
                    currentDate.Month.ToString("D2"), uniqueFileName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"فشل في رفع الملف: {ex.Message}", ex);
            }
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
