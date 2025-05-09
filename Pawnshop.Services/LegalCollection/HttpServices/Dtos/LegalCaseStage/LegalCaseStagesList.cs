using System.Collections.Generic;
using Pawnshop.Data.Models.LegalCollection.Dtos;

namespace Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseStage
{
    public class LegalCaseStagesList
    {
        public int Count { get; set; }
        public List<LegalCaseStageDto> List { get; set; }
    }
}