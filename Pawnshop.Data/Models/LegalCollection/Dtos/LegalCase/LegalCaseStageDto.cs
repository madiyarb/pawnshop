namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    /// <summary>
    /// Стадия дела Legal Collection
    /// </summary>
    public class LegalCaseStageDto
    {
        public int Id { get; set; }
    
        public string StageName { get; set; }
        public string StageCode { get; set; }
    }
}
