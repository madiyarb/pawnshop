using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts;
using System;
using Pawnshop.Data.Models.Mintos;
using Pawnshop.Data.Models.Mintos.UploadModels;

namespace Pawnshop.Data.Access
{
    public class MintosContractActionRepository : RepositoryBase, IRepository<MintosContractAction>
    {
        public MintosContractActionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(MintosContractAction entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>($@"
INSERT INTO MintosContractActions
       ( ContractId, MintosContractId, ContractActionId, InvestorScheduleId, Status)
VALUES ( @ContractId, @MintosContractId, @ContractActionId, @InvestorScheduleId, @Status )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }

        }

        public void Update(MintosContractAction entity)
        {
                UnitOfWork.Session.Execute(@"
UPDATE MintosContractActions SET InvestorScheduleId = @InvestorScheduleId, UploadDate = @UploadDate, Status = @Status WHERE Id=@Id",
                    entity, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            UnitOfWork.Session.Execute(@"UPDATE MintosContractActions SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public MintosContractAction Get(int id)
        {
            return UnitOfWork.Session.Query<MintosContractAction, ContractAction, MintosContractAction>($@"
SELECT t.*, u.*
  FROM MintosContractActions t
  JOIN ContractActions u ON u.Id = t.ContractActionId
WHERE t.Id = @id",
                (t, u) =>
                {
                    t.ContractAction = u;
                    return t;
                }, new
                {
                    id
                }).FirstOrDefault();
        }

        public MintosContractAction GetByContractActionId(int id)
        {
            return UnitOfWork.Session.Query<MintosContractAction, ContractAction, MintosContractAction>($@"
                SELECT t.*, u.*
                  FROM MintosContractActions t
                  JOIN ContractActions u ON u.Id = t.ContractActionId
                WHERE t.ContractActionId = @id",
                (t, u) =>
                {
                    t.ContractAction = u;
                    return t;
                }, new
                {
                    id
                }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public MintosContractAction Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<MintosContractAction> List(ListQuery listQuery, object query = null)
        {
            var status = query?.Val<MintosUploadStatus?>("Status");

            var pre = "t.DeleteDate IS NULL AND u.DeleteDate IS NULL";
            pre += status!=null ? " AND t.Status = @status" : string.Empty;

            var condition = listQuery.Like(pre);
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<MintosContractAction, ContractAction, MintosContractAction>($@"
SELECT t.*, u.*
  FROM MintosContractActions t
  JOIN ContractActions u ON u.Id = t.ContractActionId
{condition} {page}",
                (t, u) =>
                {
                    t.ContractAction = u;
                    return t;
                }, new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter,
                    status
                }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }
    }
}