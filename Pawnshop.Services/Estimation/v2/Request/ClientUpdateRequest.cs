using System.Collections.Generic;

namespace Pawnshop.Services.Estimation.v2.Request
{
    public class ClientUpdateRequest : ClientCreationRequest
    {
        public int Client_Id { get; set; }


        public ClientUpdateRequest() { }

        public ClientUpdateRequest(
            int clientId,
            string name,
            string surname,
            string middleName,
            string birthday,
            string docType,
            string iin,
            string licenseNumber,
            string licenseDateOfIssue,
            string licenseDateOfEnd,
            string placeOfBirth,
            string licenseIssuer,
            IEnumerable<int> galleryItemIds)
            : base(name, surname, middleName, birthday, docType, iin, licenseNumber,
                  licenseDateOfIssue, licenseDateOfEnd, placeOfBirth, licenseIssuer, galleryItemIds)
        {
            Client_Id = clientId;
        }
    }
}
