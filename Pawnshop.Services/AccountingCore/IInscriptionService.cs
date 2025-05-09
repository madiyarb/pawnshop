using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.AccountingCore
{
    public interface IInscriptionService : IService<Inscription>
    {
        public IEnumerable<CashOrder> RestoreOnBalance(IContract contract, DateTime date, List<InscriptionRow> rows);
        public void RestoreOnBalance(IContract contract, DateTime date);
        public void RestoreOnBalanceOnBuyout(IContract contract, DateTime date);
        public IEnumerable<CashOrder> WriteOffBalance(int contractId, List<InscriptionRow> rows, DateTime date);
        void WriteOffOnBuyout(Contract contract, ContractAction action);
        List<InscriptionRow> GetInscriptionRows(Inscription inscription, DateTime date);
        Task<List<InscriptionRow>> GetInscriptionRowsAsync(Inscription inscription, Contract contract, DateTime date);
        InscriptionAction RestoreOrders(Inscription inscription, Contract contract, InscriptionActionType actiontype);
        IDictionary<int, (int, DateTime)> DeleteOrders(Inscription inscription, Contract contract, InscriptionActionType actiontype, int authorId, int branchId);
        Inscription Get(int id);
        Task<Inscription> GetAsync(int? inscriptionId);
        Task<Inscription> GetOnlyInscriptionAsync(int inscriptionId);
        Inscription Save(Inscription model);
        void Delete(int id);
        IDbTransaction BeginInscriptionTransaction();
        void DeleteAction(int id);
        void InsertAction(InscriptionAction action);
        void SaveActionRows(InscriptionAction action);
        ListModel<Inscription> List(ListQuery listQuery);
        public Task<Inscription> StopAccruals(UpdateLegalCaseCommand request);
        public Task ResumeAccruals(UpdateLegalCaseCommand request);
        public Task AddInscriptionRow(int inscriptionId, Inscription? inscription, InscriptionRow row);
        public Task ExecuteInscription(UpdateLegalCaseCommand request);
        public Task<bool> ApproveInscriptionAsync(int inscriptionId, Inscription inscription = null);
    }
}