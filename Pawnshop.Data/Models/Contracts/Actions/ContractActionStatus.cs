using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Contracts.Actions
{
    public enum ContractActionStatus : byte
    {
        Await = 0,
        Approved = 10,
        AwaitForCancelApprove = 15,
        Canceled = 20
    }
}