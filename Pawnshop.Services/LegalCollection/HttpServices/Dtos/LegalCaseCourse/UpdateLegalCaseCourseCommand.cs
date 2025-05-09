namespace Pawnshop.Services.LegalCollection.HttpServices.Dtos
{
    public class UpdateLegalCaseCourseCommand
    {
        public int Id { get; set; }
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public bool IsActive { get; set; }
    }
}