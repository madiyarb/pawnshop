using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Models.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Dictionaries
{
    public interface IHolidayService : IDictionaryWithSearchService<Holiday, HolidayFilter>
    {
        Holiday Get(DateTime date);
        bool IsHoliday(DateTime date, out DateTime? nextWorkingDate);
        DateTime GetFirstPreviousHolidayFromDate(DateTime date);
        Task<List<Holiday>> GetRangeHolidaysAsync(DateTime dateFrom, DateTime dateUntil);
    }
}
