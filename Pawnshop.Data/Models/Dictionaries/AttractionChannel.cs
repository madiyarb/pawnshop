namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Канал привлечения
    /// </summary>
    public class AttractionChannel : IDictionary
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Наименование на казахском 
        /// </summary>
        public string? NameAlt { get; set; }

        /// <summary>
        /// Требует дополнения
        /// </summary>
        public bool NeedsMore { get; set; }

        /// <summary>
        /// Отключен
        /// </summary>
        public bool Disabled { get; set; }
    }
}