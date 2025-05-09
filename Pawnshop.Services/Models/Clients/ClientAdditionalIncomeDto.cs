using Pawnshop.Data.Models.Clients;
using System;
using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Services.Models.Clients
{
    public class ClientAdditionalIncomeDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле 'Тип дополнительного дохода' обязательно для заполнения")]
        public int? TypeId { get; set; }

        [Required(ErrorMessage = "Поле 'Количество дополнительного дохода' обязательно для заполнения")]
        [Range(1, int.MaxValue, ErrorMessage = "Поле 'Количество дополнительного дохода' должно быть положительным числом")]
        public int? Amount { get; set; }

        public static bool HaveUpdates(ClientAdditionalIncome entity, ClientAdditionalIncomeDto dto) =>
            entity.TypeId != dto.TypeId ||
            entity.Amount != dto.Amount;
    }
}
