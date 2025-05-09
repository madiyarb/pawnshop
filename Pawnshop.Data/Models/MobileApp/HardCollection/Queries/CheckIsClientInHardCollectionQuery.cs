using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Queries
{
    public class CheckIsClientInHardCollectionQuery : IRequest<bool>
    {
        public int ClientId { get; set; }
    }
}
