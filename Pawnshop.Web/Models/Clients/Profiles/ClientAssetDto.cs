using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Clients.Profiles
{
    public class ClientAssetDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле 'Тип актива' обязально для заполнения")]
        public int? AssetTypeId { get; set; }

        [Required(ErrorMessage = "Поле 'Количество актива' обязательно для заполнения"), Range(1, int.MaxValue, ErrorMessage = "Поле 'количество' в активах должно быть положительным")]
        public int? Count { get; set; }
    }
}
