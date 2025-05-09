using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Models.List;
using Pawnshop.Web.Models.Membership;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.GroupView)]
    public class GroupController : Controller
    {
        private readonly GroupRepository _repository;
        private readonly MemberRepository _memberRepository;
        private readonly ISessionContext _sessionContext;

        public GroupController(
            GroupRepository repository,
            MemberRepository memberRepository,
            ISessionContext sessionContext)
        {
            _repository = repository;
            _memberRepository = memberRepository;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        public ListModel<Group> List([FromBody] ListQuery listQuery)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var organizationId = _sessionContext.OrganizationId;

            return new ListModel<Group>
            {
                List = _repository.List(listQuery, new { organizationId }),
                Count = _repository.Count(listQuery, new { organizationId })
            };
        }

        [AllowAnonymous]
        [HttpGet("api/groups/withcashbox")]
        public async Task<IActionResult> GetCashGroups()
        {
            var groups = await _repository.GetGroupsWIthCashBox();
            return Ok(groups);
        }

        [HttpPost]
        public CardModel<Group> Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var user = _repository.Get(id);
            if (user == null) throw new InvalidOperationException();

            return new CardModel<Group>
            {
                Member = user,
                Groups = _memberRepository.Groups(user.Id, MemberRelationType.Direct),
                Roles = _memberRepository.Roles(user.Id, false)
            };
        }

        [HttpPost, Authorize(Permissions.GroupManage)]
        [Event(EventCode.GroupSaved, EventMode = EventMode.Response)]
        public CardModel<Group> Save([FromBody] CardModel<Group> card)
        {
            if (card?.Member == null) throw new ArgumentNullException(nameof(card));

            using (var transaction = _repository.BeginTransaction())
            {
                if (card.Member.Id > 0)
                    _repository.Update(card.Member);
                else
                {
                    card.Member.OrganizationId = _sessionContext.OrganizationId;
                    card.Member.CreateDate = DateTime.Now;
                    _repository.Insert(card.Member);
                }

                var dbRoles = _memberRepository.Roles(card.Member.Id, false);
                var rolesAdd = card.Roles.Diff(dbRoles);
                var rolesDel = dbRoles.Diff(card.Roles);

                var dbGroups = _memberRepository.Groups(card.Member.Id, MemberRelationType.Direct);
                var groupsAdd = card.Groups.Diff(dbGroups);
                var groupsDel = dbGroups.Diff(card.Groups);

                _memberRepository.InsertRoles(card.Member.Id, rolesAdd);
                _memberRepository.DeleteRoles(card.Member.Id, rolesDel);
                _memberRepository.InsertGroups(card.Member.Id, groupsAdd);
                _memberRepository.DeleteGroups(card.Member.Id, groupsDel);

                transaction.Commit();
            }

            return card;
        }
    }
}