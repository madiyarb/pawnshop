namespace Pawnshop.Data.Models.LegalCollection.Dtos.LegalCase
{
    public class LegalCaseStageScenarioDto
    {
        public int? Id { get; set; }
        public int LegalCaseActionId { get; set; }
        public LegalCaseActionDto LegalCaseAction { get; set; }
        public int LegalCaseStageBeforeId { get; set; }
        public LegalCaseStageDto LegalCaseStageBefore { get; set; }
        public int LegalCaseStageAfterId { get; set; }
        public LegalCaseStageDto LegalCaseStageAfter { get; set; }
        public int LegalCaseCourseId { get; set; }
        public LegalCaseCourseDto LegalCaseCourse { get; set; }
    }
}
