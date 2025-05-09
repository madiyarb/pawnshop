using System;

namespace Pawnshop.Services.TasCore.Models.NpckEsign
{
    public class SignedFile
    {
        public Guid FileStorageId { get; set; }
        public Guid EsignFileId { get; set; }
        public string Name { get; set; }


        public SignedFile() { }

        public SignedFile(Guid fileStorageId, Guid esignFileId, string name)
        {
            FileStorageId = fileStorageId;
            EsignFileId = esignFileId;
            Name = name;
        }
    }
}
