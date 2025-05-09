namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    /// <summary>
    /// Справочник смены направлений Legal collection
    /// </summary>
    public class ChangeCourseActionDto
    {
        public int Id { get; set; }
        public string ActionName { get; set; }
        public string CourseFromCode { get; set; }
        public string CourseToCode { get; set; }
        public string ActionCode { get; set; }
    }
}