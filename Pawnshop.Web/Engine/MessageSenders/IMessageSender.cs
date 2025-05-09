using System;
using System.Collections.Generic;
using Pawnshop.Services.MessageSenders;

namespace Pawnshop.Web.Engine.MessageSenders
{
    public interface IMessageSender
    {
        void Send(string subject, string message, List<MessageReceiver> receivers, Action<SendResult> callback);
    }
}