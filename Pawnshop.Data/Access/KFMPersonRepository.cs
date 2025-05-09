using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.KFM;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Data.Access
{
    public class KFMPersonRepository : RepositoryBase
    {
        public KFMPersonRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<bool> FindByClientIdAsync(int clientId) =>
            await UnitOfWork.Session.QueryFirstOrDefaultAsync<bool>(@"SELECT 1
  FROM KFMPersons k 
  JOIN Clients c ON c.IdentityNumber = k.IdentityNumber
   AND c.Name = k.Name
   AND c.Surname = k.Surname
   AND c.Patronymic = k.Patronymic
 WHERE c.Id = @clientId",
                new { clientId }, UnitOfWork.Transaction);

        public async Task<bool> FindByIdentityNumberAsync(string identityNumber) =>
                    await UnitOfWork.Session.QueryFirstOrDefaultAsync<bool>(@"SELECT 1 FROM KFMPersons WHERE IdentityNumber = @identityNumber", new { identityNumber }, UnitOfWork.Transaction);

        public async Task<IEnumerable<KFMPerson>> FindListAsync(object query)
        {
            var surname = query?.Val<string>("Surname");
            var name = query?.Val<string>("Name");
            var patronymic = query?.Val<string>("Patronymic");
            var identityNumber = query?.Val<string>("IdentityNumber");
            var birthDay = query?.Val<DateTime?>("BirthDay");

            var predicateParams = new List<string>();

            if (!string.IsNullOrEmpty(surname))
                predicateParams.Add("Surname = @surname");

            if (!string.IsNullOrEmpty(name))
                predicateParams.Add("Name = @name");

            if (!string.IsNullOrEmpty(patronymic))
                predicateParams.Add("Patronymic = @patronymic");

            if (!predicateParams.Any())
                return null;

            if (!string.IsNullOrEmpty(identityNumber))
                predicateParams.Add("IdentityNumber = @identityNumber");

            if (birthDay.HasValue)
                predicateParams.Add("BirthDate = CAST(@birthDay AS DATE)");

            var predicate = "WHERE ";
            predicate += string.Join(" AND ", predicateParams.ToArray());

            return await UnitOfWork.Session.QueryAsync<KFMPerson>($@"SELECT *
  FROM KFMPersons
 {predicate}",
                new { surname, name, patronymic, identityNumber, birthDay }, UnitOfWork.Transaction);
        }

        public async Task Insert(List<KFMPerson> persons)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.ExecuteAsync(@"INSERT INTO KFMPersons
VALUES ( @Num, @Surname, @Name, @Patronymic, @Birthdate, @IdentityNumber, @Note, @Correction, @UploadDate )",
                    persons, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void TruncateTable()
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("TRUNCATE TABLE KFMPersons", transaction: UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}
