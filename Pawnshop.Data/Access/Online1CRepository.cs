using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.Online1C;

namespace Pawnshop.Data.Access
{
    public class Online1CRepository : RepositoryBase, IRepository
    {
        public Online1CRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>Получение начисления</summary>
        public async Task<List<Online1CReportAccural>> GetAccurals(Online1CReportData data)
        {
            var list = await UnitOfWork.Session.QueryAsync<Online1CReportAccural>(
                @"oneCIntegration.GetAccruals",
                new
                {
                    accrualDate = data.Date
                },
                commandTimeout: 300000,
                commandType: CommandType.StoredProcedure
            );

            return list.ToList();
        }

        /// <summary>Получение погашения</summary>
        public async Task<List<Online1CReportPayment>> GetPayments(Online1CReportData data)
        {
            var list = await UnitOfWork.Session.QueryAsync<Online1CReportPayment>(
                @"oneCIntegration.GetPayments",
                new
                {
                    paymentDate = data.Date
                },
                commandTimeout: 300000,
                commandType: CommandType.StoredProcedure
            );

            return list.ToList();
        }

        /// <summary>Получение выдачи</summary>
        public async Task<List<Online1CReportIssues>> GetIssues(Online1CReportData data)
        {
            var list = await UnitOfWork.Session.QueryAsync<Online1CReportIssues>(
                @"oneCIntegration.GetIssues",
                new
                {
                    issueDate = data.Date
                },
                commandTimeout: 300000,
                commandType: CommandType.StoredProcedure
            );

            return list.ToList();
        }

        /// <summary>Поступление поступление денег</summary>
        public async Task<List<Online1CReportPrepayment>> GetPrepayments(Online1CReportData data)
        {
            var list = await UnitOfWork.Session.QueryAsync<Online1CReportPrepayment>(
                @"oneCIntegration.GetPrepayments",
                new
                {
                    accrualDate = data.Date
                },
                commandTimeout: 300000,
                commandType: CommandType.StoredProcedure
            );

            return list.ToList();
        }

        /// <summary>Получить освоение аванса</summary>
        public async Task<List<Online1CReportDebitDepos>> GetDebitDepos(Online1CReportData data)
        {
            var list = await UnitOfWork.Session.QueryAsync<Online1CReportDebitDepos>(
                @"oneCIntegration.GetDebitDepos",
                new
                {
                    accrualDate = data.Date
                },
                commandTimeout: 300000,
                commandType: CommandType.StoredProcedure
            );

            return list.ToList();
        }

        /// <summary>Получить кассовые операции, не относящиеся к кредитам</summary>
        public async Task<List<Online1CReportCashFlows>> GetCashFlows(Online1CReportData data)
        {
            var list = await UnitOfWork.Session.QueryAsync<Online1CReportCashFlows>(
                @"oneCIntegration.GetCashFlows",
                new
                {
                    data.BeginDate,
                    data.EndDate
                },
                commandTimeout: 300000,
                commandType: CommandType.StoredProcedure
            );

            return list.ToList();
        }
    }
}