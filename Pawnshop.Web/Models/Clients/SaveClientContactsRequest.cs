using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Domains;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Clients
{
    public class SaveClientContactsRequest
    { 
        /// <summary>
        /// Контакты
        /// </summary>
        [Required]
        public List<ClientContactDto> Contacts { get; set; }

        /// <summary>
        /// Одноразовый пароль
        /// </summary>
        public string OTP { get; set; }
    }
}
