namespace Pawnshop.Services.LegalCollection.HttpServices.Dtos
{
    public class CreateLegalCaseCourseCommand
    {
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public bool IsActive { get; set; }
    }
}