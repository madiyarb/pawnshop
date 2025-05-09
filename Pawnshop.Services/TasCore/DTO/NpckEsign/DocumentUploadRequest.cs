using System;

namespace Pawnshop.Services.TasCore.DTO.NpckEsign
{
    public class DocumentUploadRequest
    {
        public string Link { get; set; }
        public Guid? SignedFileStorageId { get; set; }
        public string Name { get; set; }


        public DocumentUploadRequest() { }

        public DocumentUploadRequest(string link, Guid? signedFileStorageId, string name)
        {
            Link = link;
            SignedFileStorageId = signedFileStorageId;
            Name = name;
        }
    }
}
