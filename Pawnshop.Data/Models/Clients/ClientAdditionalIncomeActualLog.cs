using Pawnshop.Core;
using System;

namespace Pawnshop.Data.Models.Clients
{
    public class ClientAdditionalIncomeActualLog : ClientAdditionalIncome, ILoggerEntity
    {
        public int ClientAdditionalIncomeId { get; set; }
        public int OperationAuthorId { get; set; }
        public OperationType OperationType { get; set; }
        public DateTime LogDateTime { get; set; } = DateTime.Now;
        public string LogReason { get; set; }

        public static ClientAdditionalIncomeActualLog MapFromBaseEntity(ClientAdditionalIncome clientAdditionalIncome)
        {
            var clientAdditionalIncomeLog = new ClientAdditionalIncomeActualLog
            {
                ClientAdditionalIncomeId = clientAdditionalIncome.Id,
                ContractId = clientAdditionalIncome.ContractId,
                ClientId = clientAdditionalIncome.ClientId,
                TypeId = clientAdditionalIncome.TypeId,
                Amount = clientAdditionalIncome.Amount,
                AuthorId = clientAdditionalIncome.AuthorId,
                CreateDate = clientAdditionalIncome.CreateDate,
                DeleteDate = clientAdditionalIncome.DeleteDate
            };

            return clientAdditionalIncomeLog;
        }
    }
}
