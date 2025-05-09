using System.Collections.Generic;

namespace Pawnshop.Services.TasCore.DTO.NpckEsign
{
    public class GenerateUrlRequest
    {
        public string Iin { get; set; }
        public string Phone { get; set; }
        public string RedirectUri { get; set; }
        public string Language { get; set; }
        public List<string> DocumentIds { get; set; }
    }
}
