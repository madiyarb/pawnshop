using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Pawnshop.Core.Validation;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class CBAddress : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор массива
        /// </summary>
        [RequiredId(ErrorMessage = "Поле Идентификатор массива обязательно для заполнения")]
        public long CollectionId { get; set; }

        /// <summary>
        /// Идентификатор типа
        /// </summary>
        [RequiredId(ErrorMessage = "Поле Тип адреса обязательно для заполнения")]
        public int TypeId { get; set; }

        /// <summary>
        /// Идентификатор населенного пункта
        /// </summary>
        // TODO: Добавить валидацию на одно из значений LocationId или KATOID
        public int? LocationId { get; set; }

        /// <summary>
        /// Код Като
        /// </summary>
        public string KATOID { get; set; }

        /// <summary>
        /// Улица
        /// </summary>
        [Required(ErrorMessage = "Поле Улица обязательно для заполнения")]
        public string StreetName { get; set; }

    }
}