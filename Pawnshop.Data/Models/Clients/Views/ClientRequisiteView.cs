using Newtonsoft.Json;
using System;

namespace Pawnshop.Data.Models.Clients.Views
{
    public class ClientRequisiteView
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Note { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsDefault { get; set; }

        [JsonIgnore]
        public string Value { get; set; }

    }
}
