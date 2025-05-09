using System.Collections.Generic;
using Pawnshop.Data.Models.LegalCollection.Dtos;

namespace Pawnshop.Services.LegalCollection.HttpServices.Dtos
{
    public class LegalCaseCourseList
    {
        public int Count { get; set; }
        public List<LegalCaseCourseDto> List { get; set; }
    }
}