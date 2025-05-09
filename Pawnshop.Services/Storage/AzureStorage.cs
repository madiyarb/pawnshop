using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Pawnshop.Core;

namespace Pawnshop.Services.Storage
{
    public class AzureStorage : IStorage
    {
        private readonly CloudBlobClient _client;
        private readonly ISessionContext _sessionContext;

        public AzureStorage(CloudBlobClient client, ISessionContext sessionContext)
        {
            _client = client;
            _sessionContext = sessionContext;
        }

        public async Task<string> Save(Stream stream)
        {
            var container = await GetContainer(_sessionContext.OrganizationUid);
            var name = Guid.NewGuid().ToString("D");
            var blob = container.GetBlockBlobReference(name);
            await blob.UploadFromStreamAsync(stream);

            return name;
        }

        public async Task<string> Save(Stream stream, ContainerName containerName, string fileName)
        {
            var container = await GetContainer(containerName.ToString().ToLower());
            var name = $"{Guid.NewGuid().ToString("D")}-{fileName}";
            var blob = container.GetBlockBlobReference(name);
            await blob.UploadFromStreamAsync(stream);

            return name;
        }

        public async Task<string> SaveToFolder(Stream stream, ContainerName containerName, string fileName, string folderName)
        {
            var container = await GetContainer(containerName.ToString().ToLower());
            var name = string.Concat(folderName, $"{Guid.NewGuid().ToString("D")}-{fileName}");
            var blob = container.GetBlockBlobReference(name);
            await blob.UploadFromStreamAsync(stream);

            return name;
        }

        public async Task<Stream> Load(string name)
        {
            var container = await GetContainer(_sessionContext.OrganizationUid);
            var blob = container.GetBlobReference(name);
            return await blob.OpenReadAsync();            
        }

        public async Task<Stream> Load(string name, ContainerName containerName)
        {
            var container = await GetContainer(containerName.ToString().ToLower());
            var blob = container.GetBlobReference(name);
            return await blob.OpenReadAsync();            
        }

        private async Task<CloudBlobContainer> GetContainer(string containerName)
        {
            var container = _client.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();

            return container;
        }
    }
}