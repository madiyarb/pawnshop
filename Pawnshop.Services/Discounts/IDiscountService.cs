using Pawnshop.Data.Models.Contracts.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Discounts
{
    public interface IDiscountService
    {
        List<Discount> GetByContractActionId(int contractActionId);
    }
}
