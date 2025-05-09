namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    /// <summary>
    /// Справочник действий Legal Collections
    /// </summary>
    public class LegalCaseActionDto
    {
        public int Id { get; set; }
        public string ActionName { get; set; }
        public string ActionCode { get; set; }
    }
}