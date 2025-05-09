using System.Collections.Generic;
using Pawnshop.Services.Models.TasOnline;

namespace Pawnshop.Services.TasOnline
{
    public interface ITasOnlinePaymentApi
    {
        public TasOnlineResponse Send(string url, object parameters);
    }
}