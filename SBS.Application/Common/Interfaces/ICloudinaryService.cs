using System.IO;
using System.Threading.Tasks;

namespace SBS.Application.Common.Interfaces;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string folderName = "pools");
}
