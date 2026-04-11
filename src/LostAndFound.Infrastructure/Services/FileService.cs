using System;
using System.IO;
using System.Threading.Tasks;
using LostAndFound.Core.Interfaces;

namespace LostAndFound.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly string _basePath;

    public FileService()
    {
        // Resolving the base wwwroot path using standard built-in classes
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderName = "Images")
    {
        if (fileStream == null || fileStream.Length == 0)
            throw new ArgumentException("File stream cannot be null or empty.");

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var uploadsFolder = Path.Combine(_basePath, folderName);

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var destinationStream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(destinationStream);
        }

        // Returns the relative path to save in db
        return Path.Combine(folderName, uniqueFileName).Replace("\\", "/");
    }

    public Task DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Task.CompletedTask;

        var fullPath = Path.Combine(_basePath, filePath.Replace("/", "\\"));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public async Task<string> UpdateFileAsync(Stream fileStream, string fileName, string oldFilePath, string folderName = "Images")
    {
        await DeleteFileAsync(oldFilePath);
        return await UploadFileAsync(fileStream, fileName, folderName);
    }
}
