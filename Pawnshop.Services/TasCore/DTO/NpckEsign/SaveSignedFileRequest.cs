namespace Pawnshop.Services.TasCore.DTO.NpckEsign
{
    public class SaveSignedFileRequest
    {
        public string Code { get; set; }
        public string RedirectUri { get; set; }
        public int ListId { get; set; }
    }
}
