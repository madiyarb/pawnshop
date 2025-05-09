using Pawnshop.Core;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Профиль клиента
    /// </summary>
    public class ClientProfile
    {
        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Тип образования
        /// </summary>
        public int? EducationTypeId { get; set; }

        /// <summary>
        /// Общий стаж работы
        /// </summary>
        public int? TotalWorkExperienceId { get; set; }

        /// <summary>
        /// Семейный статус
        /// </summary>
        public int? MaritalStatusId { get; set; }

        /// <summary>
        /// ФИО супруга/супруги
        /// </summary>
        public string SpouseFullname { get; set; }

        /// <summary>
        /// Доход супруга
        /// </summary>
        public int? SpouseIncome { get; set; }

        /// <summary>
        /// Количество детей
        /// </summary>
        public int? ChildrenCount { get; set; }

        /// <summary>
        /// Количество взрослых иждивенцев
        /// </summary>
        public int? AdultDependentsCount { get; set; }

        /// <summary>
        /// Количество несовершеннолетних иждивенцев
        /// </summary>
        public int? UnderageDependentsCount { get; set; }

        /// <summary>
        /// Где живет клиент
        /// </summary>
        public int? ResidenceAddressTypeId { get; set; }

        /// <summary>
        /// Работает ли сейчас
        /// </summary>
        public bool? IsWorkingNow { get; set; }

        /// <summary>
        /// Имеются ли активы
        /// </summary>
        public bool? HasAssets { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        public ClientProfile() { }


        public void SetResidenceAddressTypeId(int? residenceTypeId)
        {
            ResidenceAddressTypeId = residenceTypeId;
        }

        public void SetFamilyStatus(int? martialStatusId, string? spouseFullname,
            int? spouseIncome, int? childrenCount, int? adultDependentsCount, int? underageDependentsCount)
        {
            MaritalStatusId = martialStatusId;
            SpouseFullname = spouseFullname;
            SpouseIncome = spouseIncome;
            ChildrenCount = childrenCount;
            AdultDependentsCount = adultDependentsCount;
            UnderageDependentsCount = underageDependentsCount;
        }

        public void SetWorkAndEducation(int? educationTypeId, int? totalWorkExperienceId, bool? isWorkingNow)
        {
            EducationTypeId = educationTypeId;
            TotalWorkExperienceId = totalWorkExperienceId;
            IsWorkingNow = isWorkingNow;
        }
        public List<string> EmptyFields()
        {
            List<string> emptyFields = new List<string>();
            if (!UnderageDependentsCount.HasValue)
                emptyFields.Add("Кол-во иждивенцев до 18 лет");
            if (!ChildrenCount.HasValue)
                emptyFields.Add("Кол-во детей");

            return emptyFields;
        }
    }
}
