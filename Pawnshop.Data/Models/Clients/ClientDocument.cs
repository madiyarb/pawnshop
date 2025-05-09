using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Dictionaries.Address;
using Pawnshop.Data.Models.Files;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Clients
{
    public class ClientDocument : IEntity
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        [RequiredId(ErrorMessage = "Вид документа клиента не заполнен")]
        public int TypeId { get; set; }
        public ClientDocumentType DocumentType { get; set; }
        public string Number { get; set; }
        public string Series { get; set; }
        public int? ProviderId { get; set; }
        public string ProviderName { get; set; }
		public ClientDocumentProvider Provider { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? DateExpire { get; set; }
        public int AuthorId { get; set; }
        public User Author { get; set; }
        public string BirthPlace { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }

        public List<FileRow> Files = new List<FileRow>();


        public override bool Equals(System.Object obj)
        {
            if (obj == null)
                return false;
            ClientDocument clientDocument = obj as ClientDocument;
            if ((System.Object)clientDocument == null)
                return false;

            return Id == clientDocument.Id &&
                   ClientId == clientDocument.ClientId &&
                   TypeId == clientDocument.TypeId &&
                   Number == clientDocument.Number &&
                   Series == clientDocument.Series &&
                   ProviderId == clientDocument.ProviderId &&
                   ProviderName == clientDocument.ProviderName &&
                   Date == clientDocument.Date &&
                   DateExpire == clientDocument.DateExpire &&
                   AuthorId == clientDocument.AuthorId &&
                   BirthPlace == clientDocument.BirthPlace &&
                   CreateDate == clientDocument.CreateDate &&
                   DeleteDate == clientDocument.DeleteDate;
        }

        public bool Equals(ClientDocument clientDocument)
        {
            if ((object)clientDocument == null)
                return false;
            return Id == clientDocument.Id &&
                   ClientId == clientDocument.ClientId &&
                   TypeId == clientDocument.TypeId &&
                   Number == clientDocument.Number &&
                   Series == clientDocument.Series &&
                   ProviderId == clientDocument.ProviderId &&
                   ProviderName == clientDocument.ProviderName &&
                   Date == clientDocument.Date &&
                   DateExpire == clientDocument.DateExpire &&
                   AuthorId == clientDocument.AuthorId &&
                   BirthPlace == clientDocument.BirthPlace &&
                   DeleteDate == clientDocument.DeleteDate;
        }
    }
}
