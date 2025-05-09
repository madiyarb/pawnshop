using System;

namespace Pawnshop.Web.Models.AbsOnline
{
    /// <summary>
    /// Параметры создания заявки
    /// </summary>
    public class CreateApplicationRequest
    {
        /// <summary>
        /// Параметр шины <b><u>application_id</u></b> (номер заявки)
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>uin</u></b> (ИИН субъекта)
        /// </summary>
        public string IIN { get; set; }

        /// <summary>
        /// Параметр шины <b><u>product_id</u></b> (идентификатор продукта)
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Параметр шины <b><u>insurance</u></b> (признак использования страховки)
        /// </summary>
        public bool Insurance { get; set; }

        /// <summary>
        /// Параметр шины <b><u>lastname</u></b> (фамилия субъекта)
        /// </summary>
        public string Lastname { get; set; }

        /// <summary>
        /// Параметр шины <b><u>firstname</u></b> (имя субъекта)
        /// </summary>
        public string Firstname { get; set; }

        /// <summary>
        /// Параметр шины <b><u>middlename</u></b> (отчество субъекта)
        /// </summary>
        public string Middlename { get; set; }

        /// <summary>
        /// Параметр шины <b><u>birth_date</u></b> (дата рождения клиента)
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Параметр шины <b><u>tel</u></b> (номер мобильного телефона)
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// Параметр шины <b><u>address</u></b> (адрес субъекта)
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_brand</u></b> (марка автомобиля)
        /// </summary>
        public string CarMark { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_model</u></b> (модель автомобиля)
        /// </summary>
        public string CarModel { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_id</u></b> (гос номер автомобиля)
        /// </summary>
        public string CarId { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_year</u></b> (год выпуска автомобиля)
        /// </summary>
        public string CarYear { get; set; }

        /// <summary>
        /// Параметр шины <b><u>application_period</u></b> (срок заявки)
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Параметр шины <b><u>application_value</u></b> (сумма заявки)
        /// </summary>
        public decimal LoanCost { get; set; }

        /// <summary>
        /// Параметр шины <b><u>application_source</u></b> (Система инициинирования заявки)
        /// </summary>
        public string ApplicationSource { get; set; }

        /// <summary>
        /// Параметр шины <b><u>ref</u></b> (UTM метки)
        /// </summary>
        public string UtmTags { get; set; }

        /// <summary>
        /// Параметр шины <b><u>application_insurance</u></b> дублирует insurance (признак использования страховки)
        /// </summary>
        public bool ApplicationInsurance { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_brand_id</u></b> (идентификатор CRM марки автомобиля)
        /// </summary>
        public int? CarMarkId { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_brand_id</u></b> (идентификатор CRM модели автомобиля)
        /// </summary>
        public int? CarModelId { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_number</u></b> (гос номер автомобиля)
        /// </summary>
        public string CarNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>email</u></b> (электронная почта субъекта)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Параметр шины <b><u>expense_credit</u></b>
        /// </summary>
        public decimal? ExpenseCredit { get; set; }

        /// <summary>
        /// Параметр шины <b><u>expense_life</u></b>
        /// </summary>
        public decimal? ExpenseLife { get; set; }

        /// <summary>
        /// Параметр шины <b><u>partner_code</u></b> (код партнера)
        /// </summary>
        public string PartnerCode { get; set; }

        /// <summary>
        /// Параметр шины <b><u>summ_expense</u></b>
        /// </summary>
        public decimal? SumExpense { get; set; }

        /// <summary>
        /// Параметр шины <b><u>summ_revenue</u></b>
        /// </summary>
        public decimal? SumRevenue { get; set; }

        /// <summary>
        /// Параметр шины <b><u>tel2</u></b> (дополнительный номер телефона)
        /// </summary>
        public string AdditionalPhone { get; set; }
    }
}
