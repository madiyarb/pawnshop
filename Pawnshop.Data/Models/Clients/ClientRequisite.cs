using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.JetPay;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pawnshop.Data.Models.Clients
{
    public class ClientRequisite : IEntity, ILoggableToEntity
    {
        public int Id { get; set; }
        public int ClientId { get; set; }

        //[RequiredId(ErrorMessage = "Банк не выбран")]
        public int? BankId { get; set; }
        public Client Bank { get; set; }
        public bool IsDefault { get; set; }
        [Required(ErrorMessage = "Значение реквизита не может быть пустым")]
        public string Value { get; set; }
        public string Note { get; set; }
        [RequiredId(ErrorMessage = "Вид реквизита не выбран")]
        public int RequisiteTypeId { get; set; }
        public string Number { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string CardExpiryDate { get; set; }
        public string CardHolderName { get; set; }
        public ClientRequisiteCardType? CardType { get; set; }
        public JetPayCardPayoutInformation JetPayCardInfo { get; set; }


        public ClientRequisite()
        {

        }

        public int GetLinkedEntityId()
        {
            return ClientId;
        }

        public bool IsMatchesTheMask(string mask)
        {
            Regex regex = new Regex(mask);
            var res = regex.IsMatch(Value);
            return res;
        }

        public bool IsCorrectValue()
        {
            var errors = new List<string>();

            if (RequisiteTypeId == 1 && (!BankId.HasValue || BankId.Value == 0))
                errors.Add("Банк не выбран");

            if (RequisiteTypeId == 2)
            {
                if (string.IsNullOrEmpty(CardHolderName))
                    errors.Add("Значение владельца карты не может быть пустым");

                if (string.IsNullOrEmpty(CardExpiryDate))
                    errors.Add("Значение срок действия карты не может быть пустым");
            }

            if (errors.Any())
                throw new PawnshopApplicationException(string.Join("\r\n", errors.ToArray()));

            return true;
        }

        public ClientRequisite(int clientId, bool isDefault, string value, string note,
            int authorId, string cardExpiryDate, string cardHolderName, ClientRequisiteCardType? cardType = null)
        {
            ClientId = clientId;
            IsDefault = isDefault;
            Value = value;
            Note = note;
            RequisiteTypeId = 2;
            AuthorId = authorId;
            CardExpiryDate = cardExpiryDate;
            CardHolderName = cardHolderName;
            CreateDate = DateTime.Now;
            CardType = cardType;
        }

        public void UpdateCard(bool isDefault, string value, string note, int authorId, string cardExpiryDate, string cardHolderName)
        {
            IsDefault = isDefault;
            Value = value;
            Note = note;
            AuthorId = authorId;
            CardExpiryDate = cardExpiryDate;
            CardHolderName = cardHolderName;
        }

        public void UpdateBill(int? bankId, bool isDefault, string value, string note, int authorId)
        {
            BankId = bankId;
            IsDefault = isDefault;
            Value = value;
            Note = note;
            AuthorId = authorId;
        }

        public ClientRequisite(int clientId, int? bankId, bool isDefault, string value, string note, int authorId)
        {
            ClientId = clientId;
            BankId = bankId;
            IsDefault = isDefault;
            Value = value;
            Note = note;
            RequisiteTypeId = 1;
            AuthorId = authorId;
            CreateDate = DateTime.Now;
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
                return false;
            ClientRequisite clientRequisite = obj as ClientRequisite;
            if ((System.Object)clientRequisite == null)
                return false;

            return Id == clientRequisite.Id &&
                   ClientId == clientRequisite.ClientId &&
                   BankId == clientRequisite.BankId &&
                   IsDefault == clientRequisite.IsDefault &&
                   Value == clientRequisite.Value &&
                   Note == clientRequisite.Note &&
                   RequisiteTypeId == clientRequisite.RequisiteTypeId &&
                   //Number == clientRequisite.Number &&//TODO я х3 но в базе этого поля нету как это должно работать? 
                   AuthorId == clientRequisite.AuthorId &&
                   //CreateDate == clientRequisite.CreateDate &&
                   DeleteDate == clientRequisite.DeleteDate &&
                   CardExpiryDate == clientRequisite.CardExpiryDate &&
                   CardHolderName == clientRequisite.CardHolderName;
        }

        public bool Equals(ClientRequisite clientRequisite)
        {
            if ((object)clientRequisite == null)
                return false;
            return Id == clientRequisite.Id &&
                   ClientId == clientRequisite.ClientId &&
                   BankId == clientRequisite.BankId &&
                   IsDefault == clientRequisite.IsDefault &&
                   Value == clientRequisite.Value &&
                   Note == clientRequisite.Note &&
                   RequisiteTypeId == clientRequisite.RequisiteTypeId &&
                   //Number == clientRequisite.Number &&//TODO я х3 но в базе этого поля нету как это должно работать? 
                   AuthorId == clientRequisite.AuthorId &&
                   DeleteDate == clientRequisite.DeleteDate &&
                   CardExpiryDate == clientRequisite.CardExpiryDate &&
                   CardHolderName == clientRequisite.CardHolderName;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ ClientId.GetHashCode() ^ BankId.GetHashCode() ^ IsDefault.GetHashCode() ^
                   Value.GetHashCode() ^ Note.GetHashCode() ^ RequisiteTypeId.GetHashCode() ^ AuthorId.GetHashCode() ^
                   CreateDate.GetHashCode() ^ DeleteDate.GetHashCode() ^ CardExpiryDate.GetHashCode() ^ CardHolderName.GetHashCode();
        }
    }
}
