namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    /// <summary>
    /// Направления Дел LegalCollection
    /// </summary>
    public class LegalCaseCourseDto
    {
        public int Id { get; set; }

        /// <example> Исковое производство </example>
        /// <example> Исполнительная надпись (IsActive = 0) </example>
        /// <example> Работа по умершим </example>
        /// <example> Аукцион </example>
        public string CourseName { get; set; }
        public string CourseCode { get; set; }

        /// <summary>
        /// активно ли данное направление или нет, н.: Исп.надпись не активна = 0
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
