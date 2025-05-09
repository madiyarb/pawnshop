using System;
using System.Collections.Generic;
using System.ComponentModel;
using Pawnshop.Core;
using Pawnshop.Data.Models.Files;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Dictionaries;
using System.Linq;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Клиент
    /// </summary>
    public class Client : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Тип карты
        /// </summary>
        [DisplayName("Тип карты")]
        public CardType CardType { get; set; }

        /// <summary>
        /// Номер карты
        /// </summary>
        [DisplayName("Номер карты")]
        public string CardNumber { get; set; }

        /// <summary>
        /// ИИН/БИН
        /// </summary>
        [DisplayName("ИИН/БИН клиента")]
        [RegularExpression(Constants.IIN_REGEX, ErrorMessage = "Поле ИИН/БИН клиента должно содержать 12 цифр")]
        public string IdentityNumber { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        [DisplayName("Фамилия")]
        public string Surname { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        [DisplayName("Имя")]
        public string Name { get; set; }
        
        /// <summary>
        /// Отчество
        /// </summary>
        [DisplayName("Отчество")]
        public string Patronymic { get; set; }        

        /// <summary>
        /// Девичья фамилия
        /// </summary>
        [DisplayName("Девичья фамилия")]
        public string MaidenName { get; set; }

        /// <summary>
        /// Полное имя
        /// </summary>
        [DisplayName("Полное имя(наименование)")]
        public string FullName { get; set; }

        /// <summary>
        /// Пол: true - мужской, false - женский
        /// </summary>
        [DisplayName("Пол")]
        public bool? IsMale { get; set; }

        /// <summary>
        /// Дата рождения
        /// </summary>
        [DisplayName("Дата рождения/регистрации")]
        public DateTime? BirthDay { get; set; }

        /// <summary>
        /// Резидент
        /// </summary>
        [DisplayName("Признак резиденства")]
        public bool IsResident { get; set; }

        /// <summary>
        /// PEP(Политически важная персона)
        /// </summary>
        [DisplayName("Признак \"Политически важная персона\"(PEP)")]
        public bool IsPolitician { get; set; }

        /// <summary>
        /// Мобильный телефон
        /// </summary>
        [DisplayName("Мобильный телефон")]
        public string MobilePhone { get; set; }

        /// <summary>
        /// Все мобильные телефоны
        /// </summary>
        [DisplayName("Все мобильные телефоны")]
        public List<ClientContact> MobilePhoneList { get; set; }

        /// <summary>
        /// Городской телефон
        /// </summary>
        [DisplayName("Городской телефон")]
        public string StaticPhone { get; set; }

        /// <summary>
        /// Электронная почта
        /// </summary>
        [EmailAddress]
        [DisplayName("Электронная почта")]
        public string Email { get; set; }

        public List<ClientDocument> Documents { get; set; } = new List<ClientDocument>();

        public List<ClientAddress> Addresses { get; set; } = new List<ClientAddress>();

        /// <summary>
        /// Примечание
        /// </summary>
        [DisplayName("Примечание")]
        public string Note { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Файлы клиента
        /// </summary>
        public List<FileRow> Files { get; set; } = new List<FileRow>();
      
        /// <summary>   
        /// Идентификатор если это сотрудник
        /// </summary>
        [DisplayName("Идентификатор если это сотрудник")]
        public int? UserId { get; set; }

        /// <summary>   
        /// Идентификатор клиента/контакта в CRM
        /// </summary>
        [DisplayName("Идентификатор клиента/контакта в CRM")]
        public int? CrmId { get; set; }

        /// <summary>
        /// ИИН валиден
        /// </summary>
        public bool IdentityNumberIsValid { get; set; }

        /// <summary>
        /// Список социально уязвимых слоев населения
        /// </summary>
        public List<ClientSociallyVulnerableGroup> ClientSVG { get; set; } = new List<ClientSociallyVulnerableGroup>();

        /// <summary>
        /// Клиент сейчас СУСН
        /// </summary>
        public bool IsSVG => ClientSVG != null ? (ClientSVG.Where(x => (x.EndDate.HasValue ? x.EndDate.Value.Date : DateTime.Now.Date) >= DateTime.Now.Date).Count() > 0) : false;

        /// <summary>
        /// Банковский идентификационный код(БИК)
        /// </summary>
        [DisplayName("Банковский идентификационный код(БИК)")]
        public string BankIdentifierCode { get; set; }

        /// <summary>
        /// Банковский идентификационный код(БИК)
        /// </summary>
        [DisplayName("Банковский идентификационный код(БИК)")]
        public string BankCode { get; set; }

        /// <summary>
        /// Код Бенефициара(КБЕ)
        /// </summary>
        [DisplayName("Код Бенефициара(КБЕ)")]
        public int? BeneficiaryCode  { get; set; }

        /// <summary>
        /// Торговая марка (только для юр. лиц)
        /// </summary>
        [DisplayName("Торговая марка")]
        public string TradeName { get; set; }

        /// <summary>
        /// Сокращенное наименование (только для юр. лиц)
        /// </summary>
        [DisplayName("Аббревиатура")]
        public string Abbreviation { get; set; }

        /// <summary>
        /// Идентификатор правовой формы
        /// </summary>
        [DisplayName("Правовая форма")]
        public int LegalFormId { get; set; }

        /// <summary>
        /// Правовая форма клиента
        /// </summary>
        public ClientLegalForm LegalForm { get; set; }

        /// <summary>
        /// Идентификатор первого руководителя
        /// </summary>
        [DisplayName("Первый руководитель")]
        public int? ChiefId { get; set; }

        /// <summary>
		/// Первый руководитель текстовый
		/// </summary>
		public string ChiefName { get; set; }

		/// <summary>
		/// Первый руководитель
		/// </summary>
		public Client Chief { get; set; }

        /// <summary>
        /// Идентификатор гражданства (физ лицо)/страны регистрации (юр лицо)
        /// </summary>
        [DisplayName("Гражданство для физ.лица /Страна регистрации для юр.лица")]
        public int? CitizenshipId { get; set; }

        /// <summary>
        /// Гражданство (физ лицо)/Страна регистрации (юр лицо)
        /// </summary>
        public Country Citizenship { get; set; }

        /// <summary>
        /// Реквизиты
        /// </summary>
        public List<ClientRequisite> Requisites { get; set; } = new List<ClientRequisite>();

        /// <summary>
        /// Кодовое слово
        /// </summary>
        [DisplayName("Кодовое слово")]
        public string CodeWord { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public User Author { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Идентификатор филиала последнего привязанного договора 
        /// </summary>
        public int? LastContractBranchId { get; set; }

        /// <summary>
        /// Находится ли в черном списке клиентов 
        /// </summary>
        public bool IsInBlackList { get; set; }

        /// <summary>
        /// Клиент является получателем АСП
        /// </summary>
        public bool ReceivesASP { get; set; }
        
        /// <summary>
        /// Подтип клиента для вывода в UI
        /// </summary>
        public int SubTypeId { get; set; }

        public bool IsPensioner {get; set; } = false;
        public string DocumentNumber { get; set; }
        public DateTime? DocumentDate { get; set; }
        public string PartnerCode { get; set; }

        /// <summary>
        /// Продавец
        /// </summary>
        public bool? IsSeller { get; set; }
        
        /// <summary>
        /// связка с ContractLoanSubject
        /// </summary>
        public int SubjectId { get; set; }


        public List<string> emptyFieldsList()
        {
            List<string> emptyFieldsList = new List<string>();

            if (string.IsNullOrEmpty(Surname))
                emptyFieldsList.Add("Фамилиия");
            if (string.IsNullOrEmpty(Name))
                emptyFieldsList.Add("Имя");
            if (!BirthDay.HasValue)
                emptyFieldsList.Add("Дата рождения");
            if (!IsMale.HasValue)
                emptyFieldsList.Add("Пол");
            return emptyFieldsList;
        }
	}
}