using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries.Address;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class GroupRepository : RepositoryBase, IRepository<Group>
    {
        private readonly UserRepository _userRepository;

        public GroupRepository(IUnitOfWork unitOfWork, UserRepository userRepository) : base(unitOfWork)
        {
            _userRepository = userRepository;
        }

        public void Insert(Group entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO Members (OrganizationId, CreateDate, Locked)
     VALUES (@organizationId, @createDate, @locked);

DECLARE @memberId INT = @@IDENTITY;

INSERT INTO Groups (Id, Name, DisplayName, Type, Configuration, BitrixCategoryId, ATEId, RoadPoliceBranchId)
     VALUES (@memberId, @name, @displayName, @type, @configuration, @BitrixCategoryId, @ATEId, @RoadPoliceBranchId);

INSERT INTO MemberRelations (LeftMemberId, RightMemberId, RelationType)
     VALUES (@memberId, @memberId, 0);

SELECT @memberId", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Group entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE Members SET Locked = @locked
 WHERE Id = @id;
UPDATE Groups SET Name = @name, DisplayName = @displayName, Type = @type, Configuration = @configuration, BitrixCategoryId=@BitrixCategoryId, ATEId=@ATEId,
RoadPoliceBranchId = @RoadPoliceBranchId
 WHERE Id = @id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }

        public Group Get(int id)
        {
            return UnitOfWork.Session.Query<Group, AddressATE, DomainValue, Group>(@"
SELECT *
  FROM Groups g WITH (NOLOCK)
  JOIN Members ON Members.Id = g.Id
  JOIN AddressATEs a ON g.ATEId = a.Id
  LEFT JOIN DomainValues dv ON dv.Id = g.RoadPoliceBranchId
 WHERE g.Id = @id", (g, a, dv) =>
            {
                g.ATE = a;
                g.Signatories = GetSignatoriesForBranch(g);
                g.RoadPoliceBranch = dv;
                return g;
            }, new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }
        
        public async Task<Group> GetOnlyBranchAsync(int id)
        {
            var parameters = new { Id = id };
    
            var sqlQuery = @"
                SELECT *
                FROM Groups
                WHERE Id = @Id";

            return await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<Group>(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public async Task<Group> GetOnLineGroupAsync(int contractId)
        {
            var parameters = new { ContractId = contractId };
    
            var sqlQuery = @"
                SELECT g.*
                FROM Groups g
                    LEFT JOIN dogs.ContractAdditionalInfo ca ON g.Id = ca.SelectedBranchId
				WHERE ca.Id = @ContractId";

            return await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<Group>(sqlQuery, parameters, UnitOfWork.Transaction);
        }
        
        public async Task<Group> GetAsync(int id)
        {
            var result = await UnitOfWork.Session.QueryAsync<Group, AddressATE, DomainValue, Group>(@"
            SELECT *
            FROM Groups g WITH (NOLOCK)
            JOIN Members ON Members.Id = g.Id
            JOIN AddressATEs a ON g.ATEId = a.Id
            LEFT JOIN DomainValues dv ON dv.Id = g.RoadPoliceBranchId
            WHERE g.Id = @id", (g, a, dv) =>
            {
                g.ATE = a;
                g.RoadPoliceBranch = dv;
                return g;
            }, new { id }, UnitOfWork.Transaction);

            var group = result.FirstOrDefault();

            if (group != null)
            {
                group.Signatories = await GetSignatoriesForBranchAsync(group);
            }

            return group;
        }

        public Group Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var name = query.Val<string>("Name");
            if (name == null) throw new ArgumentNullException(nameof(name));

            var condition = "WHERE g.Name = @name";

            return UnitOfWork.Session.Query<Group, AddressATE, DomainValue, Group>($@"
            SELECT *
            FROM Groups g
              JOIN Members ON Members.Id = g.Id
              JOIN AddressATEs a ON g.ATEId = a.Id
              LEFT JOIN DomainValues dv ON dv.Id = g.RoadPoliceBranchId
            {condition}", (g, a, dv) =>
                {
                    g.ATE = a;
                    g.Signatories = GetSignatoriesForBranch(g);
                    g.RoadPoliceBranch = dv;
                    return g;
                }, new
                {
                    name
                }, UnitOfWork.Transaction).FirstOrDefault();
        }
        
        public async Task<Group> FindAsync(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var name = query.Val<string>("Name");
            if (name == null) throw new ArgumentNullException(nameof(name));

            var sqlQuery = @"
                SELECT *
                FROM Groups g
                  JOIN Members ON Members.Id = g.Id
                  JOIN AddressATEs a ON g.ATEId = a.Id
                  LEFT JOIN DomainValues dv ON dv.Id = g.RoadPoliceBranchId
                WHERE g.Name = @name";

            var result = await UnitOfWork.Session.QueryAsync<Group, AddressATE, DomainValue, Group>(
                sqlQuery, 
                (g, a, dv) =>
                {
                    g.ATE = a;
                    g.RoadPoliceBranch = dv;
                    return g;
                }, 
                new { name }, 
                UnitOfWork.Transaction);

            var group = result.FirstOrDefault();

            if (group != null)
            {
                group.Signatories = await GetSignatoriesForBranchAsync(group);
            }

            return group;
        }

        public List<Group> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var organizationId = query.Val<int?>("organizationId");
            var locked = query.Val<bool?>("locked");

            var condition = listQuery.Like(
                BuildCondition(organizationId, locked),
                "Name", "DisplayName");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "DisplayName",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Group, AddressATE, DomainValue, Group>($@"
SELECT *
  FROM Groups g
  JOIN Members ON Members.Id = g.Id
  JOIN AddressATEs a ON g.ATEId = a.Id
  LEFT JOIN DomainValues dv ON dv.Id = g.RoadPoliceBranchId
  
{condition} {order} {page}", (g, a, dv) =>
            {
                g.ATE = a;
                g.Signatories = GetSignatoriesForBranch(g);
                g.RoadPoliceBranch = dv;
                return g;
            }, new
            {
                organizationId,
                locked,
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }, UnitOfWork.Transaction).ToList();
        }

        public async Task<IEnumerable<Group>> GetGroupsWIthCashBox()
        {
            var sqlQuery = @"
                SELECT *
                FROM Groups
                WHERE IsVisible = 1 AND Name != 'TSO'";

            return await UnitOfWork.Session
                .QueryAsync<Group>(sqlQuery, UnitOfWork.Transaction);
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var organizationId = query.Val<int?>("organizationId");
            var locked = query.Val<bool?>("locked");

            var condition = listQuery.Like(
                BuildCondition(organizationId, locked),
                "Name", "DisplayName");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
  FROM Groups
  JOIN Members ON Members.Id = Groups.Id
{condition}", new
            {
                organizationId,
                locked,
                listQuery.Filter
            }, UnitOfWork.Transaction);
        }

        private string BuildCondition(int? organizationId, bool? locked)
        {
            var result = new StringBuilder();
            var wasClause = false;

            if (organizationId.HasValue)
            {
                result.Append("OrganizationId = @organizationId");
                wasClause = true;
            }
            if (locked.HasValue)
            {
                if (wasClause) result.Append(" AND ");
                result.Append("Locked = @locked");
            }

            return result.ToString();
        }

        public List<User> GetSignatoriesForBranch(Group branch)
        {
            var signatories = new List<User>();
            if (branch.Configuration is null || String.IsNullOrEmpty(branch.Configuration.Signatories))
                return signatories;
            try
            {
                string[] stringSignatories = branch.Configuration.Signatories.Split(";");
                foreach (var stringSignatory in stringSignatories)
                {
                    try
                    {
                        string[] separated = stringSignatory.Split(",");
                        int signatoryId;
                        string signatoryName;
                        int.TryParse(separated.FirstOrDefault(), out signatoryId);
                        signatoryName = separated.LastOrDefault();
                        signatories.Add(_userRepository.Get(signatoryId));
                        //signatories.Add(new Signatory(signatoryId, signatoryName));
                    }
                    catch (Exception e)
                    {
                        continue;
                    }

                }
            }
            catch (Exception e)
            {
                return signatories;
            }

            return signatories;
        }

        public async Task<List<User>> GetSignatoriesForBranchAsync(Group branch)
        {
            var signatories = new List<User>();

            if (branch.Configuration is null || string.IsNullOrEmpty(branch.Configuration.Signatories))
                return signatories;

            string[] stringSignatories = branch.Configuration.Signatories.Split(";");

            foreach (var stringSignatory in stringSignatories)
            {
                string[] separated = stringSignatory.Split(",");
                if (separated.Length != 2)
                    continue;

                if (!int.TryParse(separated[0], out int signatoryId))
                    continue;

                var signatory = await _userRepository.GetAsync(signatoryId);
                if (signatory != null)
                    signatories.Add(signatory);
            }

            return signatories;
        }

        public async Task<Group> GetByDisplayName(string displayName)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<Group>("SELECT * FROM Groups WHERE DisplayName = @displayName",
                new { displayName }, UnitOfWork.Transaction);
        }

        public List<Group> ListOnline()
        {
            return UnitOfWork.Session.Query<Group, AddressATE, DomainValue, Group>($@"
SELECT *
  FROM Groups g
  JOIN Members ON Members.Id = g.Id
  JOIN AddressATEs a ON a.Id = g.ATEId
  LEFT JOIN DomainValues dv ON dv.Id = g.RoadPoliceBranchId
 WHERE g.Name LIKE '%-TSO'",
                (g, a, dv) =>
                {
                    g.ATE = a;
                    g.Signatories = GetSignatoriesForBranch(g);
                    g.RoadPoliceBranch = dv;
                    return g;
                },
                null, UnitOfWork.Transaction)
                .ToList();
        }
    }
}