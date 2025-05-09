using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Dictionaries.Address;
using Pawnshop.Data.Models.Domains;
using System.Collections.Generic;

namespace Pawnshop.Data.Models.Membership
{
    /// <summary>
    /// Группа
    /// </summary>
    public class Group : Member
    {
        /// <summary>
        /// Системное имя группы
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Отображаемое имя группы
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Флаг филиала
        /// </summary>
        public GroupType Type { get; set; }

        /// <summary>
        /// Конфигурация
        /// </summary>
        public Configuration Configuration { get; set; }

        /// <summary>
        /// Идентификатор категории сделок в Bitrix24
        /// </summary>
        public int BitrixCategoryId { get; set; }

        /// <summary>
        /// Месторасположение филиала
        /// </summary>
        [RequiredId(ErrorMessage = "Поле Месторасположение филиала обязательно для заполнения")]
        public int ATEId { get; set; }

        /// <summary>
        /// АТЕ
        /// </summary>
        public AddressATE ATE { get; set; }

        public List<User> Signatories { get; set; } = new List<User>();

        /// <summary>
        /// Управление дорожной полиции
        /// </summary>
        public int? RoadPoliceBranchId { get; set; }
        public DomainValue? RoadPoliceBranch { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public byte IsVisible { get; set; }

        /// <summary>
        /// Проверка онлайн филиала
        /// </summary>IsTasOnlineBrancheForApplicationOnline
        public bool IsTasOnlineBranch()
        {
            if (Name.Contains("-TSO") || Name.Contains("TSO"))
                return true;
            return false;
        }

        /// <summary>
        /// Проверка онлайн филиала для онлайн заявок
        /// </summary>
        public bool IsTasOnlineBranchForApplicationOnline()
        {
            if (Name.Contains("-TSO"))
                return true;
            return false;
        }

        /// <summary>
        /// Регион
        /// </summary>
        public int? RegionId { get; set; }
    }
}