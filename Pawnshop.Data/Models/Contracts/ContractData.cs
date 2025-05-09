using Pawnshop.Data.CustomTypes;
using Pawnshop.Data.Models.Clients;
using System.Collections.Generic;
using System;

namespace Pawnshop.Data.Models.Contracts
{
    public class ContractData : IJsonObject
    {
        public Client Client { get; set; }

        public decimal PrepaymentCost { get; set; }

        public string AttractionChannelInfo { get; set; }

        // для печатной формы экспертное заключение
        // сохранение дохода от ведения бизнеса на момент подписания (если у контракта цель кредита на бизнес, инвестиции, поплнение ОС и тд.)
        public ContractExpertOpinionData ContractExpertOpinionData { get; set; }
    }

    public class ContractExpertOpinionData
    {
        public ClientExpertOpinionData ClientExpertOpinionData { get; set; }
        public List<ClientExpertOpinionData> SubjectsExpertOpinionData { get; set; }
        public List<ContractPositionData> ContractPositionData { get; set; }
    }
    public class ClientExpertOpinionData
    {
        public ClientData ClientData { get; set; }
        public List<ClientIncome> Incomes { get; set; }
        public ClientExpense Expences { get; set; }
        // это будет использоваться только для заемщика, созаемщику эти данные не нужны
        // public ClientAdditionalIncome ClientAdditionalIncome { get; set; }
    }

    public class ClientData
    {
        public string FullName { get; set; }
        public string IIN { get; set; }
        public DateTime? BirthDate { get; set; }
        public int Age { get; set; }
    }

    public class ContractPositionData
    {
        public string OwnerFullName { get; set; }
        public string PositionDetails { get; set; }
        public int ReleaseYear { get; set; }
        public int EstimatedIncome { get; set; }
    }
}
