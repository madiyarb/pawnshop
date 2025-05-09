using System.Collections.Generic;
using Pawnshop.Data.Models.LegalCollection.Dtos;

namespace Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseAction
{
    public class LegalCaseActionsList
    {
        public int Count { get; set; }
        public List<LegalCaseActionDto> List { get; set; }
    }
}