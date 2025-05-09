using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models._1c
{
    public enum QueryStatus : short
    {
        Queued = 0,
        Success = 10,
        Canceled = 20,
        Failed = 30
    }
}
