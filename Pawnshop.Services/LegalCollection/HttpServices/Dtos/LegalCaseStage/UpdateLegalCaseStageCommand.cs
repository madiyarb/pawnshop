namespace Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseStage
{
    public class UpdateLegalCaseStageCommand
    {
        public int Id { get; set; }
        public string StageName { get; set; }
        public string StageCode { get; set; }
    }
}