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
    public class SmsMessageAttributeRepository : RepositoryBase, IRepository<SmsMessageAttribute>
    {
        public SmsMessageAttributeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
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

        public SmsMessageAttribute Find(object query)
        {
            throw new NotImplementedException();
        }

        public SmsMessageAttribute Get(int id)
        {
            throw new NotImplementedException();
        }

        public void Insert(SmsMessageAttribute entity)
        {
            throw new NotImplementedException();
        }

        public List<SmsMessageAttribute> List(ListQuery listQuery, object query = null)
        {
            var pre = "WHERE sma.DeleteDate IS NULL";

            var entityType = query?.Val<string>("EntityType");

            if (!string.IsNullOrEmpty(entityType))
                pre += " AND smt.EntityType = @entityType";

            return UnitOfWork.Session.Query<SmsMessageAttribute, SmsMessageType, SmsMessageAttribute>($@"SELECT sma.*
       ,smt.*
  FROM SmsMessageAttributes sma
  LEFT JOIN SmsMessageTypes smt ON smt.Id = sma.SmsMessageTypeId
{pre}",
                (sma, smt) =>
                {
                    sma.SmsMessageType = smt;
                    return sma;
                },
                new { entityType }, UnitOfWork.Transaction).ToList();
        }

        public void Update(SmsMessageAttribute entity)
        {
            throw new NotImplementedException();
        }
    }
}
