using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Clients.Profiles
{
    public class SaveClientAssetsRequest
    {
        [Required]
        public List<ClientAssetDto> Assets { get; set; }
    }
}
