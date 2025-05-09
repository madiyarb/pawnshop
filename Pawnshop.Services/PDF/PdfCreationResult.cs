namespace Pawnshop.Services.PDF
{
    public class PdfCreationResult
    {
        public byte[] FileContent { get; set; }

        public string FileName { get; set; }

        public bool Success { get; set; }

        public string ErrorMessage { get; set; }
    }
}
