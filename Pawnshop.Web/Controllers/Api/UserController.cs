using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Options;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Engine.Security;
using Pawnshop.Web.Engine.MobileAppApi;
using Pawnshop.Web.Models.List;
using Pawnshop.Web.Models.Membership;
using Pawnshop.Web.Models.Users;
using Stimulsoft.Data.Expressions.Antlr.Runtime;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserRepository _repository;
        private readonly MemberRepository _memberRepository;
        private readonly ISessionContext _sessionContext;
        private readonly SaltedHash _saltedHash;
        private readonly MobileAppApi _mobileAppApi;
        private readonly EnviromentAccessOptions _options;

        public UserController(
            UserRepository repository,
            MemberRepository memberRepository,
            ISessionContext sessionContext, SaltedHash saltedHash,
            MobileAppApi mobileAppApi,
            IOptions<EnviromentAccessOptions> options)
        {
            _repository = repository;
            _memberRepository = memberRepository;
            _sessionContext = sessionContext;
            _saltedHash = saltedHash;
            _mobileAppApi = mobileAppApi;
            _options = options.Value;
        }

        [HttpPost]
        public ListModel<User> List([FromBody] ListQueryModel<UserListQueryModel> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<UserListQueryModel>();
            if (listQuery.Model == null) listQuery.Model = new UserListQueryModel();

            if (!_sessionContext.ForSupport)
            {
                listQuery.Model.OrganizationId = _sessionContext.OrganizationId;
            }

            return new ListModel<User>
            {
                List = _repository.List(listQuery, listQuery.Model),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }

        [HttpPost, Authorize(Permissions.UserView)]
        public CardModel<User> Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var user = _repository.Get(id);
            if (user == null) throw new InvalidOperationException();

            return new CardModel<User>
            {
                Member = user,
                Groups = _memberRepository.Groups(user.Id, MemberRelationType.Direct),
                Roles = _memberRepository.Roles(user.Id, false)
            };
        }

        [HttpPost, Authorize(Permissions.UserManage)]
        [Event(EventCode.UserSaved, EventMode = EventMode.Response)]
        public CardModel<User> Save([FromBody] CardModel<User> card)
        {
            if (card?.Member == null) throw new ArgumentNullException(nameof(card));

            var existUser = _repository.Find(new { login = card.Member.Login });
            if (existUser != null && existUser.Id != card.Member.Id)
                throw new PawnshopApplicationException("Пользователь с выбранным именем уже существует.");

            var existIdentityNumber = _repository.Find(new { login = card.Member.IdentityNumber });
            if (existIdentityNumber != null && existIdentityNumber.Id != card.Member.Id)
            {
                var error = string.Concat("Пользователь с таким ИИН уже существует: ", existIdentityNumber.Fullname);
                throw new PawnshopApplicationException(error);
            }

            using (var transaction = _repository.BeginTransaction())
            {
                if (card.Member.Id > 0)
                {
                    if (existUser.Locked && !card.Member.Locked)
                    {
                        card.Member.InvalidAttempts = 0;
                        card.Member.ExpireDate = DateTime.Now.Date.AddDays(_options.ExpireDay);
                    }

                    _repository.Update(card.Member);
                    _mobileAppApi.Save(card, "user/update");
                }
                else
                {
                    card.Member.InvalidAttempts = 0;
                    card.Member.OrganizationId = _sessionContext.OrganizationId;
                    card.Member.CreateDate = DateTime.Now;
                    card.Member.ExpireDate = DateTime.Now.Date;
                    _repository.Insert(card.Member);

                    _mobileAppApi.Save(card, "user/store");

                    /*** TODO Нормальный механизм активации пользователя через электронную почту ***/
                    _saltedHash.GetHashAndSaltString("123456", out var hash, out var salt);
                    _repository.SetPasswordAndSalt(card.Member.Id, hash, salt, 0);
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

        [HttpPost, Authorize(Permissions.UserManage)]
        [Event(EventCode.UserSaved, EventMode = EventMode.Response)]
        public User Reset([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var user = _repository.Get(id);
            if (user == null) throw new InvalidOperationException();

            using (var transaction = _repository.BeginTransaction())
            {
                /*** TODO Нормальный механизм активации пользователя через электронную почту ***/
                _saltedHash.GetHashAndSaltString("123456", out var hash, out var salt);
                _repository.SetPasswordAndSalt(user.Id, hash, salt, 0);

                transaction.Commit();
            }

            return user;
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleModel model)
        {
            try
            {
                var takenUserBranches = _memberRepository.Groups(model.TakeFromUserId, MemberRelationType.Direct).Select(x => x.Id);
                var givenUserBranches = _memberRepository.Groups(model.GiveToUserId, MemberRelationType.Direct).Select(x => x.Id);
                if (takenUserBranches.Intersect(givenUserBranches).Count() == 0)
                {
                    throw new PawnshopApplicationException("Выбранные пользователи из разных филиалов.");
                }
                using (var transaction = _repository.BeginTransaction())
                {
                    _memberRepository.DeleteRoles(model.TakeFromUserId, new List<Role> { model.Role });
                    _memberRepository.InsertRoles(model.GiveToUserId, new List<Role> { model.Role });
                    var history = new MemberRoleChangesHistory()
                    {
                        TakenFromUserId = model.TakeFromUserId,
                        GivenToUserId = model.GiveToUserId,
                        RoleId = model.Role.Id,
                        CreateDate = DateTime.Now,
                        AuthorId = _sessionContext.UserId,
                        Note = model.Note
                    };
                    await _memberRepository.AddChangeRoleHistory(history); 
                    transaction.Commit();
                }
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("api/users/tasonlinemanagerslist")]
        public async Task<IActionResult> GetTasOnlineManagers()
        {
            var users = _repository.GetAllTasOnlineManagers();

            if (users == null)
                return NotFound();
             
            return Ok(users.Select(user => new TasOnlineManagerView
            {
                Id = user.Id,
                Name = user.Fullname
            }));
        }
    }
}