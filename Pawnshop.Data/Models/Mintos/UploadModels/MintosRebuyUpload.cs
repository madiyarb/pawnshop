using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Mintos.UploadModels
{
    public class MintosRebuyUpload
    {
        public MintosRebuyUpload(string purpose)
        {
            this.purpose = purpose;
        }
        public string purpose { get; set; }
    }
}
