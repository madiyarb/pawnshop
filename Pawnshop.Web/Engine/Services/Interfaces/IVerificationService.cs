using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Web.Engine.Services.Interfaces
{
    public interface IVerificationService
    {
        (int, DateTime) Get(int clientId, string phoneNumber = null, bool sendToDefaultPhoneNumber = true);
        void Verify(string otp, int clientId);
        bool DoNeedVerification(int clientId, int? contractId = null);
        ClientContact GetDefaultContact(int clientId, bool throwExceptionOnNotFound = true);
        void CheckVerification(int contractId);
        void CheckClientQuestionnaireFilledStatus(int contractId, Contract contract = null);
    }
}
