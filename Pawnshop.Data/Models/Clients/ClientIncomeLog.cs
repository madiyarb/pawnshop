using Pawnshop.Core;
using System;

namespace Pawnshop.Data.Models.Clients
{
    public class ClientIncomeLog : ClientIncome, ILoggerEntity
    {
        public int ClientIncomeId { get; set; }
        public int OperationAuthorId { get; set; }
        public OperationType OperationType { get; set; }
        public DateTime LogDateTime { get; set; } = DateTime.Now;
        public string LogReason { get; set; }

        public static ClientIncomeLog MapFromBaseEntity(ClientIncome clientIncome)
        {
            var clientIncomeLogs = new ClientIncomeLog();

            clientIncomeLogs.ClientIncomeId = clientIncome.Id;
            clientIncomeLogs.ContractId = clientIncome.ContractId;
            clientIncomeLogs.ClientId = clientIncome.ClientId;
            clientIncomeLogs.IncomeType = clientIncome.IncomeType;
            clientIncomeLogs.ConfirmationDocumentTypeId = clientIncome.ConfirmationDocumentTypeId;
            clientIncomeLogs.FileRowId = clientIncome.FileRowId;
            clientIncomeLogs.IncomeTurns = clientIncome.IncomeTurns;
            clientIncomeLogs.MonthQuantity = clientIncome.MonthQuantity;
            clientIncomeLogs.IncomeAmount = clientIncome.IncomeAmount;
            clientIncomeLogs.CreateDate = clientIncome.CreateDate;
            clientIncomeLogs.DeleteDate = clientIncome.DeleteDate;
            clientIncomeLogs.AuthorId = clientIncome.AuthorId;

            return clientIncomeLogs;
        }
    }
}
