using Pawnshop.Data.Models.Contracts.Discounts;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Contracts
{
    public interface IContractDiscountService
    {
        void Create(ContractDiscount contractDiscount, int authorId);
        void Delete(int id);
        ContractDiscount Get(int id);
        PersonalDiscount GetPersonalDiscountById(int personalDiscountId);
    }
}
