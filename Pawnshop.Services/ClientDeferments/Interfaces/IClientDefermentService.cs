using Pawnshop.Data.Models.ClientDeferments;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.TasLabRecruit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pawnshop.Services.ClientDeferments.Interfaces
{
    public interface IClientDefermentService
    {
        Task RegisterRecruitDeferment(Recruit recruit);
        IEnumerable<ClientDeferment> GetActiveDeferments(int clientId);
        Task CreateDefermentForMilitary(Contract contract, Recruit recruit);
        ClientDefermentModel MapDefermentToModel(ClientDeferment deferment, Client client = null);
        Task CreateDeferment(Contract contract, DateTime startDefermentDate, DateTime endDefermentDate, string type);
        void CancelClientDeferment(ClientDeferment clientDeferment);
        ContractDefermentInformation GetDefermentInformation(int contractId, DateTime? dateTime = null);
        List<ContractDefermentInformation> GetCreditLineDefermentInformation(int creditLineId, DateTime? date = null);
        ClientDeferment GetActiveDeferment(int contractId);
        int GetRestructuredMonthCount(int contractId, List<ContractPaymentSchedule> contractPaymentSchedule);
    }
}
