using System;

namespace Pawnshop.Services.TasCore.DTO.NpckEsign
{
    public class DocumentUploadResponse : TasCoreBaseResponse
    {
        public Guid? FileId { get; set; }
    }
}
