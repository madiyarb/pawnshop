using System;

namespace Pawnshop.Services.TasCore.Models.NpckEsign
{
    public class TasCoreNpckEsignDocumentUploadResponse
    {
        public Guid? NpckFileId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
