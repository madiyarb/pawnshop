using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Membership
{
    public class Signatory
    {
        public int SignatoryId { get; set; }
        public string SignatoryName { get; set; }

        public Signatory(int signatoryId, string signatoryName)
        {
            SignatoryId = signatoryId;
            SignatoryName = signatoryName;
        }
    }
}
