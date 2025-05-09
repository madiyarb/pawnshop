using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Positions
{
    public interface IPositionSubjectService
    {
        PositionSubject Save(PositionSubject positionSubject);
        PositionSubject Get(int id);
        List<PositionSubject> GetSubjectsForPosition(int positionId);
        List<PositionSubject> SaveSubjectsForPosition(List<PositionSubject> positionSubjects, int positionId);
        Task<List<PositionSubject>> GetPositionSubjectsForPositionAndDate(int positionId, DateTime? beginDate = null);
        Task SavePositionSubjectsToHistoryForContract(Contract contract);
        Task MigratePositionSubjectsToHistoryIfNecessary(int positionId);
    }
}
