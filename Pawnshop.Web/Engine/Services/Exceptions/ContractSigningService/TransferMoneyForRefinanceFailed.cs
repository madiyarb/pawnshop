using System;

namespace Pawnshop.Web.Engine.Services.Exceptions.ContractSigningService
{
    public class TransferMoneyForRefinanceFailed : Exception
    {
        public TransferMoneyForRefinanceFailed(): base($"Не удалось перевести деньги с кредитной линии на договора для осуществления рефинансирования")
        { }
    }
}
