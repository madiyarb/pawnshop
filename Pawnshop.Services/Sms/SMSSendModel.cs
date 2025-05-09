using Pawnshop.Services.MessageSenders;

namespace Pawnshop.Services.Sms
{
    public class SMSSendModel
    {
        public SMSSendModel(string mes, MessageReceiver rec)
        {
            Message = mes;
            Receiver = rec;
        }
        public string Message { get; set; }
        public MessageReceiver Receiver { get; set; }
    }
}
