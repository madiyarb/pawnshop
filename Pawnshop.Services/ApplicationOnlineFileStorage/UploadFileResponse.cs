namespace Pawnshop.Services.ApplicationOnlineFileStorage
{
    public sealed class UploadFileResponse
    {
        public string Message { get; set; }
        public int Id { get; set; }
        public string CreateTime { get; set; }
        public int ListId { get; set; }
        public int Position { get; set; }
        public string FileGuid { get; set; }
        public string FileName { get; set; }
        public string FileComment { get; set; }
        public string FileOwner { get; set; }
        public string FileMimeType { get; set; }
        public string FileBusinessType { get; set; }
    }
}
