using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.Dictionaries
{
    public class HolidayService : IHolidayService
    {
        private readonly HolidayRepository _holidayRepository;

        public HolidayService(HolidayRepository holidayRepository)
        {
            _holidayRepository = holidayRepository;
        }

        public void Delete(int id) => _holidayRepository.Delete(id);

        public Holiday Get(DateTime date)
        {
            return _holidayRepository.Get(date);
        }

        public bool IsHoliday(DateTime date, out DateTime? nextWorkingDate)
        {
            var holiday = _holidayRepository.Get(date);
            nextWorkingDate = null; 

            if (holiday != null)
            {
                nextWorkingDate = holiday.PayDate;
                return true;
            }

            return false;
        }

        public DateTime GetFirstPreviousHolidayFromDate(DateTime date)
        {
            return _holidayRepository.GetFirstPreviousHolidayFromDate(date);
        }

        public async Task<List<Holiday>> GetRangeHolidaysAsync(DateTime dateFrom, DateTime dateUntil)
        {
            return await _holidayRepository.GetRangeHolidaysAsync(dateFrom, dateUntil);
        }

        public async Task<Holiday> GetAsync(int id)
        {
            return await Task.Run(() => _holidayRepository.Get(id));
        }

        public ListModel<Holiday> List(ListQueryModel<HolidayFilter> listQuery)
        {
            return new ListModel<Holiday>
            {
                Count = _holidayRepository.Count(listQuery, listQuery.Model),
                List = _holidayRepository.List(listQuery, listQuery.Model)
            };
        }

        public ListModel<Holiday> List(ListQuery listQuery)
        {

            return new ListModel<Holiday>
            {
                Count = _holidayRepository.Count(listQuery),
                List = _holidayRepository.List(listQuery)
            };
        }

        public Holiday Save(Holiday model)
        {
            if (model.Id > 0)
            {
                _holidayRepository.Update(model);
            }
            else _holidayRepository.Insert(model);

            return model;
        }
    }
}
