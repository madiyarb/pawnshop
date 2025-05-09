using System;

namespace Pawnshop.Services.TasCore.DTO.NpckEsign
{
    public class SignedFileInfo
    {
        public Guid FileStorageId { get; set; }
        public Guid EsignFileId { get; set; }
        public string Name { get; set; }
    }
}
