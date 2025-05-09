using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class CBCollateralRepository : RepositoryBase, IRepository<CBCollateral>
    {
        public CBCollateralRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(CBCollateral entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO CBCollaterals
(CBContractId, TypeId, LocationId, KATOID, Value, Currency, ValueTypeId)
VALUES(@CBContractId, @TypeId, @LocationId, @KATOID, @Value, @Currency, @ValueTypeId)
SELECT SCOPE_IDENTITY()", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(CBCollateral entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE CBCollaterals SET CBContractId = @CBContractId, TypeId = @TypeId, LocationId = @LocationId, KATOID = @KATOID, 
Value = @Value, Currency = @Currency, ValueTypeId = @ValueTypeId
                  WHERE Id=@id", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public CBCollateral Get(int id)
        {
            throw new NotImplementedException();
        }

        public CBCollateral Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<CBCollateral> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }
    }
}

