using System.Threading.Tasks;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Services.Subjects
{
    public interface ILoanSubjectService
    {
       Task<LoanSubject> SelectLoanSubjectAsync(string subjectCode);
    }
}