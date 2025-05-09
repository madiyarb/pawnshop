using Pawnshop.Data.Models.Clients;
using Pawnshop.Services.Models.TMF;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.TMF
{
    public class TmfClientContractModel
    {
        public Client Client { get; set; }
        public List<TMFContract> Contracts { get; set; }
    }
}
