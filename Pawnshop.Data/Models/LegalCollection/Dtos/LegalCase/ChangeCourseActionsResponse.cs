using System.Collections.Generic;

namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    public class ChangeCourseActionsResponse
    {
        public List<ChangeCourseActionDto> ChangeCourseActions { get; set; }
        public int Count { get; set; }
    }
}