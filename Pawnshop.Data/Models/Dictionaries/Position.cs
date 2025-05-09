using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts;
using System.ComponentModel.DataAnnotations;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Clients;
using System.Collections.Generic;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Позиция
    /// </summary>
    public class Position : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        private string _name;

        /// <summary>
        /// Наименование
        /// </summary>
        [Required(ErrorMessage = "Поле наименование позиции обязательно для заполнения")]
        public string Name
        {
            get { return _name?.ToUpper(); }
            set { _name = value.ToUpper(); }
        }

        /// <summary>
        /// Вид залога
        /// </summary>
        public CollateralType CollateralType { get; set; }

        /// <summary>
        /// Владелец залога
        /// </summary>
        public int? ClientId { get; set; }
        public Client Client { get; set; }

        //Субъекты для позиции
        public List<PositionSubject> PositionSubjects { get; set; } = new List<PositionSubject>();
        public bool? HasUsedPledge { get; set; }
    }
}
