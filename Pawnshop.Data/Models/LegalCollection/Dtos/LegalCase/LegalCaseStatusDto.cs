namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    /// <summary>
    /// Статусы дел Legal Collection
    /// </summary>
    public class LegalCaseStatusDto
    {
        public int Id { get; set; }
    
        /// <example> Не начато</example>
        /// <example> Активно</example>
        /// <example> Завершено</example>
        public string StatusName { get; set; }
        public string StatusCode { get; set; }
    }
}