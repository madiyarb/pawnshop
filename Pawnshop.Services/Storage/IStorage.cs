using System.IO;
using System.Threading.Tasks;

namespace Pawnshop.Services.Storage
{
    public interface IStorage
    {
        Task<string> Save(Stream stream);

        Task<string> Save(Stream stream, ContainerName containerName, string fileName);
        Task<string> SaveToFolder(Stream stream, ContainerName containerName, string fileName, string folderName);

        Task<Stream> Load(string name);

        Task<Stream> Load(string name, ContainerName containerName);
    }
}