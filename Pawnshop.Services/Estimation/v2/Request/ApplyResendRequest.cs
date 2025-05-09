namespace Pawnshop.Services.Estimation.v2.Request
{
    public class ApplyResendRequest
    {
        public int Apply_Id { get; set; }


        public ApplyResendRequest() { }

        public ApplyResendRequest(int applyId)
        {
            Apply_Id = applyId;
        }
    }
}
