using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pawnshop.Core.Options;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.MobileApp;
using Pawnshop.Services;
using Pawnshop.Web.Models.Membership;
using RestSharp;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;

namespace Pawnshop.Web.Engine.MobileAppApi
{
    public class MobileAppApi
    {
        private readonly string _mobileAppUrl;
        private readonly string _mobileAppUser;
        private readonly string _mobileAppPassword;
        private readonly MemberRepository _memberRepository;
        private readonly IEventLog _eventLog;

        public MobileAppApi(IOptions<EnviromentAccessOptions> options, 
            MemberRepository memberRepository, 
            IEventLog eventLog)
        {
            _mobileAppUrl = options.Value.MobileAppUrl;
            _mobileAppPassword = options.Value.MobileAppPassword;
            _mobileAppUser = options.Value.MobileAppUser;
            _memberRepository = memberRepository;
            _eventLog = eventLog;
        }

        public void Save(CardModel<User> card, string url)
        {
            var userModel = new UserModel()
            {
                FullName = card.Member.Fullname,
                Email = card.Member.Email,
                UserId = card.Member.Id,
                Locked = card.Member.Locked,
                UserName = card.Member.Login,
                GroupList = JsonConvert.SerializeObject(card.Groups.Where(x => x.Type == GroupType.Branch).Select(x => x.Id).ToList())
            };

            if (card.Roles != null && card.Roles.Any())
                SetRoleToUserModel(card.Roles, userModel);

            if (string.IsNullOrWhiteSpace(userModel.Role))
            {
                var rolesFromDb = _memberRepository.Roles(card.Member.Id, true);

                SetRoleToUserModel(rolesFromDb, userModel);
                userModel.Locked = true;
            }

            if (!string.IsNullOrWhiteSpace(userModel.Role))
                SaveUser(userModel, url);
        }

        private void SetRoleToUserModel(List<Role> roles, UserModel userModel)
        {
            userModel.Role = GetMobAppRole(roles);
        }

        public void SaveUser(UserModel model, string requestUrl)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (string.IsNullOrWhiteSpace(requestUrl))
                throw new ArgumentNullException(nameof(requestUrl));

            var fullUrl = GetUrl(requestUrl);
            var client = new RestClient(fullUrl)
            {
                Timeout = -1
            };
            var request = new RestRequest(Method.POST);
            var token = GetToken();
            request.Parameters.Clear();
            request.AlwaysMultipartFormData = true;
            request.AddHeader("Authorization", $"Bearer {token}");

            foreach (var property in model.GetType().GetProperties())
            {
                var name = property.GetCustomAttribute<JsonPropertyAttribute>().PropertyName;
                var value = property.GetValue(model);

                if (property.GetValue(model).GetType() == typeof(bool))
                    request.AddParameter(name, Convert.ToInt32(property.GetValue(model)));
                else
                    request.AddParameter(name, value);
            }

            var uri = client.BuildUri(request).AbsoluteUri;
            try
            {
                var response = client.Execute(request);
                _eventLog.Log(EventCode.MobileAppSaveUser, EventStatus.Success, requestData: uri, responseData: response.Content, uri: fullUrl, entityType: null);

            }
            catch (Exception exception)
            {
                _eventLog.Log(EventCode.MobileAppSaveUser, EventStatus.Failed, requestData: uri, responseData: exception.Message, uri: fullUrl, entityType: null);
            }
        }

        private string GetToken()
        {
            var url = GetUrl("auth/login");
            var client = new RestClient(url);

            var request = new RestRequest(Method.POST);
            request.Parameters.Clear();
            request.AlwaysMultipartFormData = true;
            request.AddParameter("username", _mobileAppUser);
            request.AddParameter("password", _mobileAppPassword);
            IRestResponse response = client.Execute(request);

            if (response.IsSuccessful)
                return JsonConvert.DeserializeObject<Token>(response.Content).AccessToken;

            throw response.ErrorException;
        }

        private string GetUrl(string requestUrl)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
                throw new ArgumentNullException(nameof(requestUrl));

            return $@"{_mobileAppUrl}{requestUrl}";
        }

        private string GetMobAppRole(List<Role> fincoreRoles)
        {
            string role = string.Empty;
            var isManager = fincoreRoles.Any(t => t.Permissions != null && t.Permissions.Permissions.Any(c => c.Name == Permissions.MobileAppAccessForManager));
            var isAppraiser = fincoreRoles.Any(t => t.Permissions != null && t.Permissions.Permissions.Any(c => c.Name == Permissions.MobileAppAccessForAppraiser));
            var isHardCollectionManager = fincoreRoles.Any(t => t.Permissions != null && t.Permissions.Permissions.Any(c => c.Name == Permissions.HardCollectionManager));
            var isHardCollectionRegionDirector = fincoreRoles.Any(t => t.Permissions != null && t.Permissions.Permissions.Any(c => c.Name == Permissions.HardCollectionRegionDirector));
            var isHardCollectionMainDirector = fincoreRoles.Any(t => t.Permissions != null && t.Permissions.Permissions.Any(c => c.Name == Permissions.HardCollectionMainDirector));

            if (isManager && isAppraiser)
                throw new PawnshopApplicationException("Нельзя назначить одновременно роль менеджера и оценщика");

            if (isManager)
                role = "Manager";
            else if (isAppraiser)
                role = "Administrator";
            else if (isHardCollectionMainDirector)
                role = "HardCollectionMainDirector";
            else if (isHardCollectionRegionDirector)
                role = "HardCollectionRegionDirector";
            else if (isHardCollectionManager)
                role = "HardCollectionManager";

            return role;
        }
    }
}