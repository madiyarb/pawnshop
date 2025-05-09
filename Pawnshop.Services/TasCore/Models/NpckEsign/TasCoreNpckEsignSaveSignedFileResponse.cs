using System.Collections.Generic;

namespace Pawnshop.Services.TasCore.Models.NpckEsign
{
    public class TasCoreNpckEsignSaveSignedFileResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<SignedFile> SignedFiles { get; set; }
    }
}
