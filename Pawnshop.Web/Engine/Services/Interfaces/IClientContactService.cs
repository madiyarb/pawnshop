using Pawnshop.Data.Models.Clients;
using Pawnshop.Web.Models.Clients;
using System.Collections.Generic;

namespace Pawnshop.Web.Engine.Services.Interfaces
{
    public interface IClientContactService
    {
        List<ClientContact> GetList(int clientId, string code = null);
        List<ClientContact> Save(int clientId, List<ClientContactDto> clientContacts, string otp);
        List<ClientContact> SaveMerchant(int clientId, List<ClientContactDto> clientContacts);
        void ValidateDtoContacts(int clientId, List<ClientContactDto> contacts);
        List<ClientContact> GetMobilePhoneContacts(int clientId);
        List<ClientContact> GetWorkPhoneContacts(int clientId);
        List<ClientContact> GetHomePhoneContacts(int clientId);
        List<ClientContact> GetEmailContacts(int clientId);
        void UpdateUkassaCheckReceive(int contactId, bool receive);
        void SaveWithoutChecks(ClientContact clientContact);
        ClientContact Find(object query);
        void Delete(int id);
        void Update(ClientContact entity);
        int? GetClientIdByDefaultPhone(string phoneNumber);
    }
}
