using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Files;

namespace Pawnshop.Services.Models.Clients
{
    public class ClientIncomeDto
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        public IncomeType IncomeType { get; set; }
        public int ConfirmationDocumentTypeId { get; set; }
        public int? FileRowId { get; set; }
        public FileRow FileRow { get; set; }
        public decimal IncomeTurns { get; set; }
        public decimal MonthQuantity { get; set; }
        public decimal IncomeAmount { get; set; }

        public static bool HaveUpdates(ClientIncome entity, ClientIncomeDto dto) =>
            entity.IncomeType != dto.IncomeType ||
            entity.ConfirmationDocumentTypeId != dto.ConfirmationDocumentTypeId ||
            entity.FileRowId != dto.FileRowId ||
            entity.IncomeTurns != dto.IncomeTurns ||
            entity.MonthQuantity != dto.MonthQuantity ||
            entity.IncomeAmount != dto.IncomeAmount;

    }
}
