using System.IO;
using System.Threading.Tasks;

namespace LostAndFound.Core.Interfaces;

public interface IFileService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderName = "Images");
    Task DeleteFileAsync(string filePath);
    Task<string> UpdateFileAsync(Stream fileStream, string fileName, string oldFilePath, string folderName = "Images");
}
