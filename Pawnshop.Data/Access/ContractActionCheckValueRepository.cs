using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class ContractActionCheckValueRepository : RepositoryBase, IRepository<ContractActionCheckValue>
    {
        public ContractActionCheckValueRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        { 
        }

        public void Insert(ContractActionCheckValue entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                INSERT INTO ContractActionCheckValues
                    (ActionId, CheckId, Value)
                VALUES 
                    (@ActionId, @CheckId, @Value)
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
        }

        public void Update(ContractActionCheckValue entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            UnitOfWork.Session.Execute(@"
                UPDATE ContractActionCheckValues
                SET 
                    ActionId = @ActionId,
                    CheckId = @CheckId, 
                    Value = @Value", entity, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            throw new NotImplementedException("Функция удаления ContractActionCheckValue недоступна");
        }

        public ContractActionCheckValue Get(int id)
        {
            return UnitOfWork.Session.Query(@"
                SELECT cacv.* FROM ContractActionCheckValues cacv
                WHERE Id = @id", new { id }, UnitOfWork.Transaction).SingleOrDefault();
        }

        public List<ContractActionCheckValue> List(ListQuery listQuery, object query)
        {
            throw new NotImplementedException("Функция получения списка ContractActionCheckValue недоступна");
        }

        public int Count(ListQuery listQuery, object query)
        {
            throw new NotImplementedException("Функция получения количества ContractActionCheckValue недоступна");
        }

        public ContractActionCheckValue Find(object query)
        {
            throw new NotImplementedException("Функция поиска ContractActionCheckValue недоступна");
        }

        public List<ContractActionCheckValue> GetByContractActionId(int contractActionId)
        {
            return UnitOfWork.Session.Query<ContractActionCheckValue>(@"
                SELECT cacv.* FROM ContractActionCheckValues cacv
                WHERE ContractActionid = @contractActionId", 
                new { contractActionId }, UnitOfWork.Transaction).ToList();
        }
    }
}
