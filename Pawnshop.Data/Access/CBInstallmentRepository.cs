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
    public class CBInstallmentRepository : RepositoryBase, IRepository<CBInstallment>
    {
        public CBInstallmentRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(CBInstallment entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO CBInstallments
(CBContractId, PaymentMethodId, PaymentPeriodId, TotalAmount, Currency, InstallmentAmount, InstallmentCount)
VALUES
(@CBContractId, @PaymentMethodId, @PaymentPeriodId, @TotalAmount, @Currency, @InstallmentAmount, @InstallmentCount)
SELECT SCOPE_IDENTITY()", entity,UnitOfWork.Transaction);

                foreach (var record in entity.Records)
                {
                    record.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO CBInstallmentRecords
(CBInstallmentId, AccountingDate, OutstandingInstallmentCount, OutstandingAmount, OverdueInstallmentCount, OverdueAmount, Fine, Penalty, ProlongationStartDate, ProlongationEndDate, LastPaymentDate)
VALUES(@CBInstallmentId, @AccountingDate, @OutstandingInstallmentCount, @OutstandingAmount, @OverdueInstallmentCount, @OverdueAmount, @Fine, @Penalty, @ProlongationStartDate, @ProlongationEndDate, @LastPaymentDate)
SELECT SCOPE_IDENTITY()", record, UnitOfWork.Transaction);
                }

                transaction.Commit();
            }
        }

        public void Update(CBInstallment entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE CBInstallments SET CBContractId = @CBContractId, PaymentMethodId = @PaymentMethodId, PaymentPeriodId = @PaymentPeriodId,
TotalAmount = @TotalAmount, Currency = @Currency, InstallmentAmount = @InstallmentAmount, InstallmentCount = @InstallmentCount
WHERE Id=@id", entity,UnitOfWork.Transaction);

                foreach (var record in entity.Records)
                {
                    UnitOfWork.Session.Execute(@"
UPDATE CBInstallmentRecords SET CBInstallmentId = @CBInstallmentId, AccountingDate = @AccountingDate, OutstandingInstallmentCount = @OutstandingInstallmentCount,
OutstandingAmount = @OutstandingAmount, OverdueInstallmentCount = @OverdueInstallmentCount, OverdueAmount = @OverdueAmount, Fine = @Fine, 
Penalty = @Penalty, ProlongationStartDate = @ProlongationStartDate, ProlongationEndDate = @ProlongationEndDate, LastPaymentDate=@LastPaymentDate
WHERE Id=@id", record, UnitOfWork.Transaction);
                }

                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public CBInstallment Get(int id)
        {
            throw new NotImplementedException();
        }

        public CBInstallment Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<CBInstallment> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }
    }
}

