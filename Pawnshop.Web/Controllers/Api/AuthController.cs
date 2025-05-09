using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Options;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Engine.Security;
using Pawnshop.Web.Engine.MobileAppApi;
using Pawnshop.Web.Models.Auth;
using Pawnshop.Web.Models.Membership;

namespace Pawnshop.Web.Controllers.Api
{
    public class AuthController : Controller
    {
        private readonly TokenProvider _tokenProvider;
        private readonly SaltedHash _saltedHash;
        private readonly ISessionContext _sessionContext;
        private readonly UserRepository _userRepository;
        private readonly MemberRepository _memberRepository;
        private readonly OrganizationRepository _organizationRepository;
        private readonly GroupRepository _groupRepository;
        private readonly BranchContext _branchContext;
        private readonly EnviromentAccessOptions _options;
        private readonly MobileAppApi _mobileAppApi;

        public AuthController(
            TokenProvider tokenProvider, SaltedHash saltedHash, ISessionContext sessionContext,
            UserRepository userRepository, MemberRepository memberRepository,
            OrganizationRepository organizationRepository, GroupRepository groupRepository, BranchContext branchContext,
            IOptions<EnviromentAccessOptions> options,
            MobileAppApi mobileAppApi)
        {
            _tokenProvider = tokenProvider;
            _saltedHash = saltedHash;
            _sessionContext = sessionContext;
            _userRepository = userRepository;
            _memberRepository = memberRepository;
            _organizationRepository = organizationRepository;
            _groupRepository = groupRepository;
            _branchContext = branchContext;
            _options = options.Value;
            _mobileAppApi = mobileAppApi;
        }

        [HttpPost, AllowAnonymous]
        [Event(EventCode.UserAuthentication, EventMode = EventMode.Request, IncludeFails = true)]
        public string SignIn([FromBody] SignInModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            ModelState.Validate();

            var user = _userRepository.Find(new { login = model.Username });

            if (user == null)
                throw new PawnshopApplicationException("Имя пользователя указано не верно, пользователь не найден");

            if (user.Locked)
                throw new PawnshopApplicationException("Пользователь заблокирован, обратитесь к администратору");

            _userRepository.GetPasswordAndSalt(user.Id, out var password, out var salt);
            var verified = _saltedHash.VerifyHashString(model.Password, password, salt);

            if (!verified)
            {
                user.InvalidAttempts += 1;

                if (user.InvalidAttempts == Constants.INVALID_PASSWORD_ATTEMPTS)
                {
                    user.Locked = true;

                    var roles = _memberRepository.Roles(user.Id, true);
                    var card = new CardModel<User>
                    {
                        Member = user,
                        Groups = _memberRepository.Groups(user.Id, MemberRelationType.Direct),
                        Roles = roles
                    };
                    _mobileAppApi.Save(card, "user/update");
                }

                _userRepository.Update(user);

                var availableAttempts = Constants.INVALID_PASSWORD_ATTEMPTS - user.InvalidAttempts;

                if (availableAttempts == 0)
                    throw new PawnshopApplicationException(
                        $"Исчерпано количество попыток для входа, пользователь заблокирован. Для разблокировки обратитесь к администратору");

                throw new PawnshopApplicationException($"Неверный пароль. Осталось попыток для входа: {availableAttempts}");
            }

            var token = string.Empty;
            var organization = _organizationRepository.Get(user.OrganizationId);
            if (organization != null && !organization.Locked)
            {
                user.InvalidAttempts = 0;
                _userRepository.Update(user);

                var roles = _memberRepository.Roles(user.Id, true);
                token = _tokenProvider.CreateToken(user, organization, roles.ToArray());
            }

            return token;
        }

        [HttpPost("/api/auth/profile"), Authorize]
        public ProfileModel Profile()
        {
            var prof = new ProfileModel();
            prof.User = _userRepository.Get(_sessionContext.UserId);
            prof.Organization = _organizationRepository.Get(_sessionContext.OrganizationId);
            prof.Branches = _memberRepository.Groups(_sessionContext.UserId, null).Where(g => g.Type == GroupType.Branch).ToArray();

            SetBranchSignatories(prof.Branches);
            return prof;
        }

        private void SetBranchSignatories(Data.Models.Membership.Group[] branches)
        {
            foreach (var branch in branches)
            {
                if (branch.Configuration is null || String.IsNullOrEmpty(branch.Configuration.Signatories))
                    continue;
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
                            branch.Signatories.Add(_userRepository.Get(signatoryId));
                            //branch.Signatories.Add(new Signatory(signatoryId, signatoryName));
                        }
                        catch (Exception e)
                        {
                            continue;
                        }

                    }
                }
                catch (Exception e)
                {
                    continue;
                }
            }
        }

        [HttpPost("/api/auth/updateProfile"), Authorize]
        [Event(EventCode.UserProfileSaved)]
        public ProfileModel UpdateProfile([FromBody] User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            ModelState.Validate();

            var existUser = _userRepository.Find(new { login = user.Login });
            if (existUser != null && existUser.Id != _sessionContext.UserId)
            {
                throw new PawnshopApplicationException("Пользователь с выбранным именем уже существует.");
            }
            if (existUser == null)
            {
                existUser = _userRepository.Get(_sessionContext.UserId);
            }
            existUser.Login = user.Login;
            existUser.Email = user.Email;
            existUser.Fullname = user.Fullname;

            _userRepository.Update(existUser);

            return Profile();
        }

        [HttpPost("/api/auth/updateOrganizationConfiguration"), Authorize(Permissions.OrganizationConfigurationManage)]
        [Event(EventCode.OrganizationConfigSaved)]
        public void UpdateOrganizationConfiguration([FromBody] Configuration model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            ModelState.Validate();

            _branchContext.Organization.Configuration = model;

            _organizationRepository.Update(_branchContext.Organization);
        }

        [HttpPost("/api/auth/updateBranchConfiguration"), Authorize(Permissions.BranchConfigurationManage)]
        [Event(EventCode.BranchConfigSaved)]
        public void UpdateBranchConfiguration([FromBody] Configuration model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            ModelState.Validate();

            _branchContext.Branch.Configuration = model;

            _groupRepository.Update(_branchContext.Branch);
        }

        [HttpPost("/api/auth/updatePassword"), Authorize]
        [Event(EventCode.UserPasswordSaved)]
        public void UpdatePassword([FromBody] PasswordModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            ModelState.Validate();

            _userRepository.GetPasswordAndSalt(_sessionContext.UserId, out var password, out var salt);
            var verified = _saltedHash.VerifyHashString(model.OldPassword, password, salt);
            if (!verified)
            {
                throw new PawnshopApplicationException("Пароль указан не верно.");
            }

            var user = _userRepository.Get(_sessionContext.UserId);

            int expireDays = _options.ExpireDay;

            if (user.ForSupport)
            {
                expireDays = 30;
                if (model.NewPassword.Length < 12)
                    throw new PawnshopApplicationException("Пароль должен быть не меньше 12 символов");
            }

            _saltedHash.GetHashAndSaltString(model.NewPassword, out password, out salt);
            _userRepository.SetPasswordAndSalt(_sessionContext.UserId, password, salt, expireDays);
        }

        [HttpGet("/api/auth/check_token_valid"), AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult CheckTokenValid(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));

            var jwtSecurityToken = _tokenProvider.ReadToken(token);

            if (jwtSecurityToken == null)
                return NotFound();

            return Ok(DateTime.Now <= jwtSecurityToken.ValidTo.ToLocalTime());
        }

        [HttpPost("/api/auth/{userid}/applicationToken"), Authorize(Permissions.UserManage)]
        public string CreateUnexpiredToken([FromRoute]int userid)
        {
            var user = _userRepository.Get(userid);
            var token = string.Empty;
            var organization = _organizationRepository.Get(user.OrganizationId);
            if (organization != null && !organization.Locked)
            {
                user.InvalidAttempts = 0;
                _userRepository.Update(user);

                var roles = _memberRepository.Roles(user.Id, true);
                token = _tokenProvider.CreateApplicationToken(user, organization, roles.ToArray());
            }

            return token;
        }

        [HttpPost("/api/auth/check_token_valid"), AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult CheckTokenValidForMobile([FromBody] TokenModel tokenModel)
        {
            if (string.IsNullOrWhiteSpace(tokenModel?.Token))
                throw new ArgumentNullException(nameof(tokenModel));

            var jwtSecurityToken = _tokenProvider.ReadToken(tokenModel.Token);

            if (jwtSecurityToken == null)
                return NotFound();

            return Ok(DateTime.Now <= jwtSecurityToken.ValidTo.ToLocalTime());
        }
    }
}
