using System.Threading.Tasks;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Services.Subjects
{
    public class LoanSubjectService : ILoanSubjectService
    {
        private readonly LoanSubjectRepository _loanSubjectRepository;

        public LoanSubjectService(LoanSubjectRepository loanSubjectRepository)
        {
            _loanSubjectRepository = loanSubjectRepository;
        }

        public async Task<LoanSubject> SelectLoanSubjectAsync(string subjectCode)
        {
            return await _loanSubjectRepository.GetByCodeAsync(subjectCode);
        }
    }
}