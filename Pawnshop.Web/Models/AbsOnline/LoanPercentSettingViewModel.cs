namespace Pawnshop.Web.Models.AbsOnline
{
    /// <summary>
    /// Детали продукта для онлайна
    /// </summary>
    public class LoanPercentSettingViewModel
    {
        /// <summary>
        /// Параметр шины <b><u>name</u></b> (наименование продукта)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Параметр шины <b><u>name_kz</u></b> (наименование продукта на казахском)
        /// </summary>
        public string NameKz { get; set; }

        /// <summary>
        /// Параметр шины <b><u>description</u></b> (описание продукта)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Параметр шины <b><u>description_kz</u></b> (описание продукта на казахском)
        /// </summary>
        public string DescriptionKz { get; set; }

        /// <summary>
        /// Параметр шины <b><u>product_id</u></b> (идентификатор продукта)
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Параметр шины <b><u>insurance</u></b> (тип наличия страховки (0 - нету, 1 - страховка обязательна, 2 - страховка опциональна))
        /// </summary>
        public int InsuranceType { get; set; }

        /// <summary>
        /// Параметр шины <b><u>percent</u></b> (процентная ставка)
        /// </summary>
        public decimal Percent { get; set; }

        /// <summary>
        /// Параметр шины <b><u>min_value</u></b> (минимальная процентная ставка)
        /// </summary>
        public decimal MinPercent { get; set; }

        /// <summary>
        /// Параметр шины <b><u>max_percent</u></b> (максимальная процентная ставка)
        /// </summary>
        public decimal MaxPercent { get; set; }

        /// <summary>
        /// Параметр шины <b><u>is_active</u></b> (признак активности/актуальности)
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Параметр шины <b><u>percent</u></b> (минимальная сумма займа)
        /// </summary>
        public decimal MinValue { get; set; }

        /// <summary>
        /// Параметр шины <b><u>max_value</u></b> (максимальная сумма займа)
        /// </summary>
        public decimal MaxValue { get; set; }

        /// <summary>
        /// Параметр шины <b><u>min_month</u></b> (минимальный период займа)
        /// </summary>
        public int MinMonth { get; set; }

        /// <summary>
        /// Параметр шины <b><u>max_month</u></b> (максимальный период займа)
        /// </summary>
        public int MaxMonth { get; set; }

        /// <summary>
        /// Параметр шины <b><u>min_age</u></b> (минимальный возраст клиента)
        /// </summary>
        public int MinAge { get; set; }

        /// <summary>
        /// Параметр шины <b><u>max_age</u></b> (максимальный возраст клиента)
        /// </summary>
        public int MaxAge { get; set; }
    }
}
