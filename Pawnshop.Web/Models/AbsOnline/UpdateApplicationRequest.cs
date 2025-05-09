using System;

namespace Pawnshop.Web.Models.AbsOnline
{
    public class UpdateApplicationRequest
    {
        /// <summary>
        /// Параметр шины <b><u>date_birth</u></b> (дата рождения клиента)
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_brand_id</u></b> (идентификатор CRM марки автомобиля )
        /// </summary>
        public string CarMarkId { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_model_id</u></b> (идентификатор CRM модели автомобиля )
        /// </summary>
        public string CarModelId { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_id</u></b> (гос номер автомобиля)
        /// </summary>
        public string CarNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_year</u></b> (год выпуска автомобиля)
        /// </summary>
        public string CarYear { get; set; }

        /// <summary>
        /// Параметр шины <b><u>application_id</u></b> (номер заявки)
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>firstname</u></b> (имя субъекта)
        /// </summary>
        public string Firstname { get; set; }

        /// <summary>
        /// Параметр шины <b><u>lastname</u></b> (фамилия субъекта)
        /// </summary>
        public string Lastname { get; set; }

        /// <summary>
        /// Параметр шины <b><u>middlename</u></b> (отчество субъекта)
        /// </summary>
        public string Middlename { get; set; }

        /// <summary>
        /// Параметр шины <b><u>passport_date</u></b> (дата выдачи документа)
        /// </summary>
        public DateTime? PassportIssueDate { get; set; }

        /// <summary>
        /// Параметр шины <b><u>passport_num</u></b> (номер документа)
        /// </summary>
        public string PassportNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>payment_type</u></b> (тип платежа)
        /// </summary>
        public string PaymentType { get; set; }

        /// <summary>
        /// Параметр шины <b><u>application_period</u></b> (срок заявки)
        /// </summary>
        public int? Period { get; set; }

        /// <summary>
        /// Параметр шины <b><u>product_id</u></b> (идентификатор продукта)
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Параметр шины <b><u>vehicle_passport_date</u></b> (дата выдачи тех паспорта)
        /// </summary>
        public DateTime? TechPassportIssueDate { get; set; }

        /// <summary>
        /// Параметр шины <b><u>vehicle_passport_num</u></b> (номер тех паспорта)
        /// </summary>
        public string TechPassportNumber { get; set; }
    }
}
