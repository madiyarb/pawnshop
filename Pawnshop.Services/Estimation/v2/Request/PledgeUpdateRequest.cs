using System.Collections.Generic;

namespace Pawnshop.Services.Estimation.v2.Request
{
    public class PledgeUpdateRequest : PledgeCreationRequest
    {
        public int Pledge_Id { get; set; }


        public PledgeUpdateRequest() { }

        public PledgeUpdateRequest(
            int pledgeId,
            string brand,
            string registrationNumber,
            string vinCode,
            string licensePlate,
            string registrationIssueDate,
            string model,
            int modelId,
            string color,
            string holderName,
            int prodYear,
            IEnumerable<int> galleryItemIds)
            : base(brand, registrationNumber, vinCode, licensePlate, registrationIssueDate,
                  model, modelId, color, holderName, prodYear, galleryItemIds)
        {
            Pledge_Id = pledgeId;
        }
    }
}
