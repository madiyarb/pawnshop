using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Clients
{
    public class SaveClientBlackListRequest
    {
        public List<ClientsBlackListDto> BlackList { get; set; }
    }
}
