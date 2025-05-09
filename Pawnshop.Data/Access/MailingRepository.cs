using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Mail;

namespace Pawnshop.Data.Access
{
    public class MailingRepository : RepositoryBase, IRepository<Mailing>
    {
        public MailingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Mailing entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                INSERT INTO Mailings (MailingType, Description, Subject, MailingText, CreateDate, AuthorId )
                    VALUES ( @MailingType, @Description, @Subject, @MailingText, @CreateDate, @AuthorId )
                        SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(Mailing entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE Mailings
                        SET MailingType = @MailingType, Description = @Description, Subject = @Subject, MailingText = @MailingText
                            WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Mailings SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public Mailing Find(object query)
        {
            var mailingType = query?.Val<MailingType?>("MailingType");

            var condition = "WHERE DeleteDate IS NULL";

            condition += mailingType.HasValue ? " AND MailingType = @mailingType" : string.Empty;

            var mailing =  UnitOfWork.Session.QuerySingleOrDefault<Mailing>($@"
                SELECT *
                  FROM Mailings
                    {condition}", 
                new { mailingType }
            );

            if (mailing != null)
            {
                mailing.MailingAddresses = UnitOfWork.Session.Query<MailingAddress>($@"
                SELECT *
                  FROM MailingAddresses
                    WHERE MailingId = @Id",
                    new { mailing.Id }
                ).ToList();
            }

            return mailing;
        }

        public Mailing Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<Mailing>(@"
                SELECT *
                    FROM Mailings
                        WHERE Id = @id", new { id });
        }

        public List<Mailing> List(ListQuery listQuery, object query = null)
        {
            if (listQuery is null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = "WHERE DeleteDate IS NULL";

            return UnitOfWork.Session.Query<Mailing>($@"
                SELECT *
                  FROM Mailings
                    {condition}"
            ).ToList();
        }
        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery is null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = "WHERE DeleteDate IS NULL";

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                  FROM Mailings
                    {condition}");
        }
    }
}