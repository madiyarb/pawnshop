using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Data.Models.Dictionaries.Address;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Data.Models.Membership;
using IEntity = Pawnshop.Core.IEntity;

namespace Pawnshop.Data.Models.Contracts
{
    /// <summary>
    /// Обзорная анкета займа
    /// </summary>
    public class ContractProfile : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }
        /// <summary>
        /// Вид бизнеса по проекту
        /// </summary>
        [CustomValidation(typeof(ContractProfile), "BusinessTypeValidate")]
        [Required(ErrorMessage = "Поле Вид бизнеса по проекту обязательно для заполнения")]
        public int BusinessTypeId { get; set; }
        public DomainValue BusinessType { get; set; }
        /// <summary>
        /// Количество новых рабочих мест (план)
        /// </summary>
        //[CustomValidation(typeof(ContractProfile), "NewEmploymentNumberPlannedValidate")]
        [Required(ErrorMessage = "Поле Количество новых рабочих мест (план) обязательно для заполнения")]
        public int? NewEmploymentNumberPlanned { get; set; }
        /// <summary>
        /// Признак начинающего заемщика
        /// </summary>
        public bool IsStartingBorrower { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }
        public User Author { get; set; }
        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
        /// <summary>
        /// Количество новых рабочих мест (факт)
        /// </summary>
        public int? NewEmploymentNumberActual { get; set; }
        /// <summary>
        /// Идентификатор АТЕ
        /// </summary>
        [CustomValidation(typeof(ContractProfile), "ATEValidate")]
        [Required(ErrorMessage = "Поле Место реализации проекта обязательно для заполнения")]
        public int ATEId { get; set; }
        public AddressATE ATE { get; set; }


        public static ValidationResult BusinessTypeValidate(int value)
        {
            if (value == 0)
                return new ValidationResult("Поле Вид бизнеса по проекту обязательно для заполнения");

            return ValidationResult.Success;
        }

        /*public static ValidationResult NewEmploymentNumberPlannedValidate(int? value)
        {
            if (value == null)
                return new ValidationResult("Поле Количество новых рабочих мест (план) обязательно для заполнения");

            return ValidationResult.Success;
        }*/
        
        public static ValidationResult ATEValidate(int value)
        {
            if (value == 0)
                return new ValidationResult("Поле Место реализации проекта обязательно для заполнения");

            return ValidationResult.Success;
        }
    }
}