using System.IO;

namespace Pawnshop.Services.ApplicationOnlineFileStorage
{
    public sealed class FileWithContentType
    {
        public string? ContentType { get; set; }
        public Stream Stream { get; set; }
    }
}
