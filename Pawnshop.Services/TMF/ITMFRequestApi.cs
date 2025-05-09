using Pawnshop.Data.Models.TMF;
using Pawnshop.Services.Models.TMF;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.TMF
{
    public interface ITMFRequestApi
    {
        TMFBaseResponse Send(TMFBaseRequest tmfRequest, object parameters);
    }
}
