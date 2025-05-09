using System;
using Pawnshop.Data.Models.Clients;

namespace Pawnshop.Services.Clients
{
    /// <summary>
    /// Модель для создания клиента по упрощённой схеме(не полными данными)
    /// </summary>
    public class CreateSimpleClientCommand
    {
        /// <summary>
        /// Id контракта
        /// </summary>
        public int ContractId { get; set; }
        
        /// <summary>
        /// Тип карты клиента
        /// </summary>
        public CardType? CardType { get; set; }
        // public int CardTypeValue { get; set; }
        
        /// <summary>
        /// Id автора
        /// </summary>
        public int AuthorId { get; set; }
        
        /// <summary>
        /// Id легальной формы
        /// </summary>
        public string LegalFormCode { get; set; }
        
        /// <summary>
        /// Code с таблицы LoanSubject
        /// </summary>
        public string LoanSubjectCode { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        
        /// <summary>
        /// Отчество
        /// </summary>
        public string? Patronymic { get; set; }
        public string IIN { get; set; }
        public bool IsResident { get; set; }
        public bool IsSeller { get; set; }
        public int CitizenshipId { get; set; }
        public int BeneficiaryCode { get; set; }
        public DateTime BirthDay { get; set; }
        public bool IsMale { get; set; }

        /// <summary>
        /// Дата выдачи документа
        /// </summary>
        public DateTime DocumentIssueDate { get; set; }
        
        /// <summary>
        /// Дата истечения срока документа
        /// </summary>
        public DateTime DocumentExpireDate { get; set; }
        public string DocumentNumber { get; set; }
        
        /// <summary>
        /// Место рождения
        /// </summary>
        public string BirthPlace { get; set; }
        public string DocumentProviderCode { get; set; }
        public string ClientDocumentTypeCode { get; set; }
        public ClientContact Contact { get; set; }
    }
}