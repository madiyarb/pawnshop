using System.Collections.Generic;

namespace Pawnshop.Services.MessageSenders
{
    public class MessageReceiver
    {
        public int ReceiverId { get; set; }

        public string ReceiverName { get; set; }

        public string ReceiverAddress { get; set; }

        public List<MessageReceiver> CopyAddresses { get; set; } = new List<MessageReceiver>();
    }
}