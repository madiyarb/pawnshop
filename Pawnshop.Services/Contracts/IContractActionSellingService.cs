using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Sellings;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Models.Sellings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Contracts
{
    public interface IContractActionSellingService
    {
        ListModel<Selling> List(ListQueryModel<SellingListQueryModel> listQuery);

        ListQueryModel<SellingListQueryModel> enrichQuery(ListQueryModel<SellingListQueryModel> listQuery);

        void RegisterSellings(Contract contract, ContractAction action, int userId, int branchId);

        Selling GetSelling(int id);

        Selling GetSellingDuty(SellingDuty selling);

        Selling Sell(Selling selling, int branchId);

        void Cancel(int id, int branchId);

        void Delete(int id);

        Selling Save(Selling model, int? branchId = null);
    }
}
