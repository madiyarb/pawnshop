namespace Pawnshop.Services.ApplicationOnlineFileStorage
{
    public sealed class FileStorageOptions
    {
        public string BaseUrl { get; set; }

        public string UserName { get; set; }

        public string Secret { get; set; }
        public string SignableLink { get; set; }
    }
}
