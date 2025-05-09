namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    /// <summary>
    /// Суды
    /// </summary>
    public class CourtDto
    {
        public int Id { get; set; }
    
        /// <example> Арбитражный суд </example>
        /// <example> Районный суд (Суд первой инстанции) </example>
        public string CourtName { get; set; }
        public string CourtCode { get; set; }
    }
}
