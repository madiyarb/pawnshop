using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Clients.Profiles
{
    public class SaveClientAdditionalContactsRequest
    {
        [Required]
        public List<ClientAdditionalContactDto> AdditionalContacts { get; set; }
    }
}
