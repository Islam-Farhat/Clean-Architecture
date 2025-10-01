using CSharpFunctionalExtensions;
using E_commerce.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace E_commerce.Infrastructure.Services
{
    public class MediaService : IMediaService
    {
        public async Task<Result<string>> UploadImage(string imageBase64, string path)
        {
            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(imageBase64))
                    throw new ArgumentException("Base64 image string is required");

                if (string.IsNullOrWhiteSpace(path))
                    throw new ArgumentException("Upload path is required");

                // Create directory if it doesn't exist
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                // Clean base64 string
                string cleanBase64 = CleanBase64String(imageBase64);

                // Get file extension from base64 string
                string extension = GetImageExtension(imageBase64) ?? "jpg";

                // Generate filename
                string fileName = $"{DateTime.Now:yyyyMMddHHmmss}.{extension}";
                string fullPath = Path.Combine(path, fileName);

                // Convert and save image
                byte[] imageBytes = Convert.FromBase64String(cleanBase64);
                await File.WriteAllBytesAsync(fullPath, imageBytes);

                // Verify file was saved successfully
                return await VerifyFileUpload(fullPath, imageBytes.Length) ? Result.Success<string>(fileName) : Result.Failure<string>("Error upload");

            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Invalid base64 format: {ex.Message}");
                return Result.Failure<string>("");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Upload failed: {ex.Message}");
                return Result.Failure<string>("");
            }
        }

        private string CleanBase64String(string base64String)
        {
            if (base64String.Contains("base64,"))
            {
                return base64String.Split(',')[1];
            }
            return base64String.Trim();
        }

        private string GetImageExtension(string base64String)
        {
            if (base64String.StartsWith("data:image/"))
            {
                int start = base64String.IndexOf('/') + 1;
                int end = base64String.IndexOf(';');
                if (end > start)
                {
                    return base64String.Substring(start, end - start);
                }
            }
            return null;
        }

        private async Task<bool> VerifyFileUpload(string filePath, long expectedSize)
        {
            // Wait a moment for file system to catch up
            await Task.Delay(100);

            if (!File.Exists(filePath))
                return false;

            var fileInfo = new FileInfo(filePath);
            return fileInfo.Length > 0 && fileInfo.Length == expectedSize;
        }
    }
}
