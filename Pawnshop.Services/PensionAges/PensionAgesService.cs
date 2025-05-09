using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.PensionAge;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.PensionAges
{
    public class PensionAgesService : IPensionAgesService
    {
        private readonly PensionAgesRepository _pensionAgesRepository;
        private readonly IEventLog _eventLog;
        private readonly ISessionContext _sessionContext;
        public PensionAgesService(PensionAgesRepository pensionAgesRepository, IEventLog eventLog, ISessionContext sessionContext)
        {
            _pensionAgesRepository = pensionAgesRepository;
            _eventLog = eventLog;
            _sessionContext = sessionContext;
        }

        public PensionAge Save(PensionAge entity)
        {
            if(entity.CreateDate == new DateTime())
            {
                entity.CreateDate = DateTime.Now;
            }
            if(entity.AuthorId == 0)
            {
                entity.AuthorId = _sessionContext.UserId;
            }
            if (entity.Id > 0) entity = _pensionAgesRepository.Update(entity);
            else entity =  _pensionAgesRepository.Insert(entity);

            return entity;
        }

        public void Delete(int id)
        {
            _pensionAgesRepository.Delete(id);
        }

        public void Update(PensionAge entity)
        {
            _pensionAgesRepository.Update(entity);
        }

        public double GetMalePensionAge()
        {
            var pensionAge = _pensionAgesRepository.GetMaleAge();
            if (pensionAge == 0)
                throw new PawnshopApplicationException("Не найден пенсионный возраст");
            return pensionAge;
        }

        public double GetFemalePensionAge()
        {
            var pensionAge = _pensionAgesRepository.GetFemaleAge();
            if (pensionAge == 0)
                throw new PawnshopApplicationException("Не найден пенсионный возраст");
            return pensionAge;
        }

        public List<PensionAge> List()
        {
            return _pensionAgesRepository.List(null);
        }

        public PensionAge Get(int id)
        {
            return _pensionAgesRepository.Get(id);
        }
    }
}
