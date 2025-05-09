
namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    public class LegalCollectionDocumentTypeDto
    {
        public int Id { get; set; }
    
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }
    
        /// <summary>
        /// Наименование на другом языке. Перевод
        /// </summary>
        public string? AlternativeName { get; set; }
        public string Code { get; set; }
    }
}