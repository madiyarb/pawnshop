using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Sellings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Calculation
{
    public interface ISellingRowBuilder
    {
        Selling GetSellingDuty(Contract contract, Selling selling, bool saveDiscount, int? branchId = null);
    }
}
