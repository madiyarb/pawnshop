using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Auction.Dtos.Mapping
{
    public class CreateClientDto
    {
        /// <summary>
        /// Идентификатор клиента, предоставленный внешним микросервисом.
        /// </summary>
        public int ExternalClientId { get; set; }
        public string IIN { get; set; }

        /// <summary>
        /// Имя 
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        public string? MiddleName { get; set; }
    }
}
