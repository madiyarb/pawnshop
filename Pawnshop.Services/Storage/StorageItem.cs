using System.IO;

namespace Pawnshop.Services.Storage
{
    public class StorageItem
    {
        public string Name { get; set; }

        public Stream Content { get; set; }
    }
}