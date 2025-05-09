using System.Collections.Generic;

namespace Pawnshop.Services.TasCore.DTO.NpckEsign
{
    public class SaveSignedFileResponse : TasCoreBaseResponse
    {
        public List<SignedFileInfo> SignedFiles { get; set; }
    }
}
