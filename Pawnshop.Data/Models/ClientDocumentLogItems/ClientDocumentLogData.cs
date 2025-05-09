using System;
using Pawnshop.Data.Models.Clients;

namespace Pawnshop.Data.Models.ClientDocumentLogItems
{
    public class ClientDocumentLogData
    {
        public int DocumentId { get; set; }
        public int ClientId { get; set; }
        public string Number { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? DateExpire { get; set; }
        public int? ProviderId { get; set; }
        public string BirthPlace { get; set; }
        public DateTime? DeleteDate { get; set; }

        public ClientDocumentLogData()
        {
            
        }

        public ClientDocumentLogData(ClientDocument document)
        {
            DocumentId = document.Id;
            ClientId = document.ClientId;
            Number = document.Number;
            Date = document.Date;
            DateExpire = document.DateExpire;
            ProviderId = document.ProviderId;
            BirthPlace = document.BirthPlace;
            DeleteDate = document.DeleteDate;
        }
    }
}
