using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Queries
{
    public class CheckInHardCollectionContractsQuery : IRequest<bool>
    {
        public int ContractId { get; set; }
        public int ClientId { get; set; }
    }
}
