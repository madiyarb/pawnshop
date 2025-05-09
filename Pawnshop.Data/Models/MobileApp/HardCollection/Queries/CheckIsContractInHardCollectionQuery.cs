using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Queries
{
    public class CheckIsContractInHardCollectionQuery : IRequest<bool>
    {
        public int ContractId { get; set; }
    }
}
