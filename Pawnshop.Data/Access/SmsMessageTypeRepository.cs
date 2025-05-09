using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.Sms;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pawnshop.Data.Access
{
    public class SmsMessageTypeRepository : RepositoryBase, IRepository<SmsMessageType>
    {
        public SmsMessageTypeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public SmsMessageType Find(object query)
        {
            throw new NotImplementedException();
        }

        public SmsMessageType Get(int id)
        {
            throw new NotImplementedException();
        }

        public void Insert(SmsMessageType entity)
        {
            throw new NotImplementedException();
        }

        public List<SmsMessageType> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<SmsMessageType>(@"SELECT * FROM SmsMessageTypes WHERE DeleteDate IS NULL",
                null, UnitOfWork.Transaction).ToList();
        }

        public void Update(SmsMessageType entity)
        {
            throw new NotImplementedException();
        }
    }
}
