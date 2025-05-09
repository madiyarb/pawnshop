using System;
using Org.BouncyCastle.Math;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Web.Models.Clients
{
    public class ClientListQueryModel
    {
        public bool? IsIndividual { get; set; }
        public bool? IsBank { get; set; }
    }
}