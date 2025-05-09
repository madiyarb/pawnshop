using System;
using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Dictionaries.Address;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Clients
{
    public class ClientAddress : IEntity
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        [RequiredId(ErrorMessage = "Вид адреса обязателен к выбору")]
        public int AddressTypeId { get; set; }
        public AddressType AddressType { get; set; }
        public int? CountryId { get; set; }
        public Country Country { get; set; }
        public int? ATEId { get; set; }
        public AddressATE ATE { get; set; }
        public int? GeonimId { get; set; }
        public AddressGeonim Geonim { get; set; }
        public string BuildingNumber { get; set; }
        public string RoomNumber { get; set; }
        public string FullPathRus { get; set; }
        public string FullPathKaz { get; set; }
        public DateTime CreateDate { get; set; }
        public int AuthorId { get; set; }
        public User Author { get; set; }
        public DateTime? DeleteDate { get; set; }
        public bool IsActual { get; set; }
        public string Note { get; set; }

        public ClientAddress()
        {
            
        }

        public ClientAddress(int clientId, int addressTypeId, int? countryId, int? ateId, int? geonimId,
            string buildingNumber, string roomNumber, int authorId, bool isActual, string note)
        {
            ClientId = clientId;
            AddressTypeId = addressTypeId;
            CountryId = countryId;
            ATEId = ateId;
            GeonimId = geonimId;
            BuildingNumber = buildingNumber ;
            RoomNumber = roomNumber;
            AuthorId = authorId;
            IsActual = isActual;
            Note = note;
            CreateDate = DateTime.Now;
        }

        public void Update(int addressTypeId, int? countryId, int? ateId, int? geonimId,
            string buildingNumber, string roomNumber, bool isActual, string note)
        {
            AddressTypeId = addressTypeId;
            CountryId = countryId;
            ATEId = ateId;
            GeonimId = geonimId;
            BuildingNumber = buildingNumber;
            RoomNumber = roomNumber;
            IsActual = isActual;
            Note = note;
        }

        public void Delete()
        {
            IsActual = false;
            DeleteDate = DateTime.Now;
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
                return false;
            ClientAddress clientAddress = obj as ClientAddress;
            if ((System.Object)clientAddress == null)
                return false;

            return Id == clientAddress.Id &&
                   ClientId == clientAddress.ClientId &&
                   AddressTypeId == clientAddress.AddressTypeId &&
                   CountryId == clientAddress.CountryId &&
                   ATEId == clientAddress.ATEId &&
                   Note == clientAddress.Note &&
                   GeonimId == clientAddress.GeonimId &&
                   //Number == clientRequisite.Number &&//TODO я х3 но в базе этого поля нету как это должно работать? 
                   BuildingNumber == clientAddress.BuildingNumber &&
                   RoomNumber == clientAddress.RoomNumber &&
                   FullPathRus == clientAddress.FullPathRus &&
                   FullPathKaz == clientAddress.FullPathKaz &&
                   CreateDate == clientAddress.CreateDate &&
                   AuthorId == clientAddress.AuthorId &&
                   DeleteDate == clientAddress.DeleteDate &&
                   IsActual == clientAddress.IsActual &&
                   Note == clientAddress.Note;
        }

        public bool Equals(ClientAddress clientAddress)
        {
            if ((object)clientAddress == null)
                return false;
            return Id == clientAddress.Id &&
                   ClientId == clientAddress.ClientId &&
                   AddressTypeId == clientAddress.AddressTypeId &&
                   CountryId == clientAddress.CountryId &&
                   ATEId == clientAddress.ATEId &&
                   Note == clientAddress.Note &&
                   GeonimId == clientAddress.GeonimId &&
                   //Number == clientRequisite.Number &&//TODO я х3 но в базе этого поля нету как это должно работать? 
                   BuildingNumber == clientAddress.BuildingNumber &&
                   RoomNumber == clientAddress.RoomNumber &&
                   FullPathRus == clientAddress.FullPathRus &&
                   FullPathKaz == clientAddress.FullPathKaz &&
                   AuthorId == clientAddress.AuthorId &&
                   DeleteDate == clientAddress.DeleteDate &&
                   IsActual == clientAddress.IsActual &&
                   Note == clientAddress.Note;
        }
    }
}
