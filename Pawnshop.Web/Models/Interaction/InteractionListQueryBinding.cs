using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Models.Interaction;

namespace Pawnshop.Web.Models.Interaction
{
    public class InteractionListQueryBinding : PageBinding
    {
        [FromQuery(Name = "auhorId")]
        public int? AuthorId { get; set; }

        [FromQuery(Name = "clientId")]
        public int? ClientId { get; set; }

        [FromQuery(Name = "externalPhone")]
        public string ExternalPhone { get; set; }

        [FromQuery(Name = "type")]
        public InteractionType? InteractionType { get; set; }
    }
}
