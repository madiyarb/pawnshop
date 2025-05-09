using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.Calls;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Notifications;
using System;

namespace Pawnshop.Data.Models.Interaction
{
    public class Interaction : IEntity
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int AuthorId { get; set; }
        public User Author { get; set; }
        public int? UpdateAuthorId { get; set; }
        public User UpdateAuthor { get; set; }
        public InteractionType InteractionType { get; set; }
        public int? ClientId { get; set; }
        public Client Client { get; set; }
        public Guid? ApplicationOnlineId { get; set; }
        public ApplicationOnline ApplicationOnline { get; set; }
        public string InternalPhone { get; set; }
        public string ExternalPhone { get; set; }
        public string Result { get; set; }
        public int? CallPurposeId { get; set; }
        public int? AttractionChannelId { get; set; }
        public int? CallId { get; set; }
        public Call Call { get; set; }
        public int? SmsNotificationId { get; set; }
        public Notification SmsNotification { get; set; }
        public string CarYear { get; set; }
        public string Firstname { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string PreferredLanguage { get; set; }


        public Interaction() { }

        public Interaction(int authorId, InteractionType interactionType, string internalPhone, string externalPhone, string result, string carYear,
            string firstname, string surname, string patronymic, string preferredLanguage, int? clientId = null, Guid? applicationOnlineId = null,
            int? callPurposeId = null, int? attractionChannelId = null, int? callId = null, int? smsNotificationId = null)// Почему carYear обязательный 
        {
            ApplicationOnlineId = applicationOnlineId;
            AttractionChannelId = attractionChannelId;
            AuthorId = authorId;
            CallId = callId;
            CallPurposeId = callPurposeId;
            CarYear = carYear;
            ClientId = clientId;
            CreateDate = DateTime.Now;
            ExternalPhone = externalPhone;
            Firstname = firstname;
            InteractionType = interactionType;
            InternalPhone = internalPhone;
            Patronymic = patronymic;
            Result = result;
            SmsNotificationId = smsNotificationId;
            Surname = surname;
            PreferredLanguage = preferredLanguage;
        }

        public void Update(int updateAuthorId, string result, string carYear, string firstname, string surname, string patronymic, string preferredLanguage,
            int? callPurposeId, int? attractionChannelId)
        {
            UpdateDate = DateTime.Now;
            UpdateAuthorId = updateAuthorId;

            if (!string.IsNullOrEmpty(result))
                Result = result;

            if (!string.IsNullOrEmpty(carYear))
                CarYear = carYear;

            if (!string.IsNullOrEmpty(firstname))
                Firstname = firstname;

            if (!string.IsNullOrEmpty(patronymic))
                Patronymic = patronymic;

            if (!string.IsNullOrEmpty(firstname))
                Surname = surname;

            if (!string.IsNullOrEmpty(preferredLanguage))
                PreferredLanguage = preferredLanguage;

            if (callPurposeId.HasValue)
                CallPurposeId = callPurposeId;

            if (attractionChannelId.HasValue)
                AttractionChannelId = attractionChannelId;
        }
    }
}
