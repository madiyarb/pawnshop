using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class ContractLoanSubjectRepository : RepositoryBase, IRepository<ContractLoanSubject>
    {
        public ContractLoanSubjectRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        { 
        }

        public void Insert(ContractLoanSubject entity)
        {
            entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                INSERT INTO ContractLoanSubjects
                    (SubjectId, ContractId, ClientId, AuthorId, CreateDate, DeleteDate) 
                VALUES 
                    (@SubjectId, @ContractId, @ClientId, @AuthorId, @CreateDate, @DeleteDate)
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
        }
        
        public async Task<int> InsertAsync(ContractLoanSubject entity)
        {
            entity.Id = await UnitOfWork.Session.ExecuteScalarAsync<int>(@"
                INSERT INTO ContractLoanSubjects
                    (SubjectId, ContractId, ClientId, AuthorId, CreateDate, DeleteDate) 
                VALUES 
                    (@SubjectId, @ContractId, @ClientId, @AuthorId, @CreateDate, @DeleteDate)
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

            return entity.Id;
        }

        public void Update(ContractLoanSubject entity)
        {
            UnitOfWork.Session.Execute(@"
                UPDATE ContractLoanSubjects 
                    SET
                        SubjectId = @SubjectId, 
                        ContractId = @ContractId, 
                        ClientId = @ClientId, 
                        AuthorId = @AuthorId, 
                        CreateDate = @CreateDate, 
                        DeleteDate = @DeleteDate
                WHERE Id = @Id", entity, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            DateTime deleteDate = DateTime.Now;
            UnitOfWork.Session.Execute(@"
                UPDATE ContractLoanSubjects 
                    SET
                        DeleteDate = @deleteDate
                WHERE Id = @id", new { deleteDate, id }, UnitOfWork.Transaction);
        }

        public ContractLoanSubject Get(int id)
        {
            ContractLoanSubject contractLoanSubject = UnitOfWork.Session.Query<ContractLoanSubject>(@"
                SELECT cls.* FROM ContractLoanSubjects cls
                WHERE cls.Id = @id AND cls.DeleteDate IS NULL", new { id }, UnitOfWork.Transaction).FirstOrDefault();

            return contractLoanSubject;
        }

        public ContractLoanSubject Find(object query)
        {
            throw new NotImplementedException("Функция Find не имеет реализации");
        }

        public List<ContractLoanSubject> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException("Функция List не имеет реализации");
        }

        public List<ContractLoanSubject> GetListByContractIdAndLoanSubjectId(int contractId, int loanSubjectId)
        {
            List<ContractLoanSubject> contractLoanSubjects = UnitOfWork.Session.Query<ContractLoanSubject>(@"
                SELECT cls.* FROM ContractLoanSubjects cls
                WHERE
                    cls.SubjectId = @loanSubjectId
                    AND cls.ContractId = @contractId
                    AND cls.DeleteDate IS NULL", 
                    new { contractId, loanSubjectId }, UnitOfWork.Transaction).ToList();

            return contractLoanSubjects;
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException("Функция Count не имеет реализации");
        }
    }
}
