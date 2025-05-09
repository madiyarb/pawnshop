using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Audit;

namespace Pawnshop.Data.Access
{
    public class JobLogRepository : RepositoryBase, IRepository<JobLogItem>
    {
        public JobLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
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

        public JobLogItem Find(object query)
        {
            throw new NotImplementedException();
        }

        public JobLogItem Get(int id)
        {
            throw new NotImplementedException();
        }

        public void Insert(JobLogItem entity)
        {
            UnitOfWork.Session.Execute(@"
                INSERT INTO JobLogItems ( JobName, JobCode, JobStatus, EntityType, EntityId, RequestData, ResponseData, CreateDate )
                VALUES ( @JobName, @JobCode, @JobStatus, @EntityType, @EntityId, @RequestData, @ResponseData, @CreateDate )
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
        }

        public List<JobLogItem> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(JobLogItem entity)
        {
            throw new NotImplementedException();
        }
    }
}
