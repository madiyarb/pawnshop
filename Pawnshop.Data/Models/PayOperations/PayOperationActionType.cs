using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.PayOperations
{
    public enum PayOperationActionType : short
    {
        Check = 10,
        ReturnIfWrong = 20,
        ChangeOrRepair = 30,
        Execute = 40,
        Cancel = 50
    }
}
