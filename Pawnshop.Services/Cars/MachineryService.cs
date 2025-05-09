using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Cars
{
    public class MachineryService : IMachineryService
    {
        private readonly MachineryRepository _repository;
        private readonly IVehcileService _vehcileService;

        public MachineryService(MachineryRepository repository, IVehcileService vehcileService)
        {
            _repository = repository;
            _vehcileService = vehcileService;
        }

        public ListModel<Machinery> ListWithCount(ListQuery listQuery)
        {
            return new ListModel<Machinery>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        public Machinery Get(int id)
        {
            var machinery = _repository.Get(id);
            if (machinery == null)
                throw new NullReferenceException($"Спецтехника с Id {id} не найден");
            return machinery;
        }

        public Machinery Save(Machinery machinery)
        {
            if (machinery.Id > 0) _repository.Update(machinery);
            else _repository.Insert(machinery);

            return machinery;
        }

        public void Delete(int id)
        {
            var count = _repository.RelationCount(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить позицию, так как она привязана к позиции договора");
            }
            _repository.Delete(id);
        }

        public List<string> Colors()
        {
            return _repository.Colors();
        }

        public void Validate(Machinery machinery)
        {
            _vehcileService.BodyNumberValidate(machinery.BodyNumber);
            _vehcileService.TechPassportNumberValidate(machinery.TechPassportNumber);
            _vehcileService.TechPassportDateValidate(machinery.TechPassportDate);
            _vehcileService.ReleaseYearValidate(machinery.ReleaseYear);
            _vehcileService.MarkValidate(machinery.VehicleMark);
            _vehcileService.ModelValidate(machinery.VehicleModel);
        }
    }
}
