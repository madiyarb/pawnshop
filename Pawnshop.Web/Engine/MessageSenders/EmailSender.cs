using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Extensions;
using MimeKit;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Options;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Mail;
using Pawnshop.Services.MessageSenders;
using Pawnshop.Services.Storage;

namespace Pawnshop.Web.Engine.MessageSenders
{
    public class EmailSender : IMessageSender
    {
        private readonly EnviromentAccessOptions _options;
        private readonly MailingRepository _mailingRepository;
        private readonly IStorage _storage;

        public EmailSender(
            IOptions<EnviromentAccessOptions> options,
            MailingRepository mailingRepository,
            IStorage storage)
        {
            _options = options.Value;
            _mailingRepository = mailingRepository;
            _storage = storage;
        }

        public void Send(string subject, string message, List<MessageReceiver> receivers, Action<SendResult> callback)
        {
            if (subject == null)
                throw new ArgumentNullException(nameof(subject));

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArithmeticException($"{nameof(subject)} не должен быть пустым или содержать только пробельные символы");

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException($"{nameof(message)} не должен быть пустым или содержать только пробелы");

            if (receivers == null)
                throw new ArgumentNullException(nameof(receivers));

            if (receivers.Count == 0)
                throw new ArgumentException($"{nameof(receivers)} не должен быть пустым");

            if (receivers.Any(r => r == null))
                throw new ArgumentException($"{nameof(receivers)} не должен содержать пустые элементы");

            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            foreach (var receiver in receivers)
            {
                try
                {
                    SendEmail(subject, message, receiver);
                    callback(new SendResult { ReceiverId = receiver.ReceiverId, Success = true, SendAddress = receiver.ReceiverAddress });
                }
                catch (Exception e)
                {
                    callback(new SendResult { ReceiverId = receiver.ReceiverId, StatusMessage = e.Message, Success = false, SendAddress = receiver.ReceiverAddress });
                }
            }
        }

        public void SendEmail(string subject, string message, MessageReceiver receiver)
        {
            if (string.IsNullOrWhiteSpace(receiver.ReceiverAddress)) throw new PawnshopApplicationException("Не заполнен адрес электронной почты");
            if (!_options.SendEmailNotifications)
            {
                return;
            }

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("ТАС КРЕДИТ Компания", "no-reply@tascredit.kz"));
            emailMessage.To.Add(new MailboxAddress(receiver.ReceiverName, receiver.ReceiverAddress));
            emailMessage.Cc.AddRange(receiver.CopyAddresses.Select(a => new MailboxAddress(a.ReceiverName, a.ReceiverAddress)).ToList());

            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect(_options.SmtpServerName, _options.SmtpServerPort, true);
                client.Authenticate(_options.NoReplyEmailName, _options.NoReplyEmailPassword);
                client.Send(emailMessage);
                client.Disconnect(true);
            }
        }

        public string SendEmail(MailingType mailingType, string folderName = null, Stream stream = null, string fileName = null, ContainerName? container = null)
        {
            var mailing = _mailingRepository.Find(new { MailingType = mailingType });

            if (mailing is null)
                throw new PawnshopApplicationException($"Для типа {mailingType.GetDisplayName()} не найдено рассылок");

            if (mailing.MailingAddresses is null)
                throw new PawnshopApplicationException($"Для типа {mailingType.GetDisplayName()} не найдены адресаты");

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("ТАС КРЕДИТ Компания", "no-reply@tascredit.kz"));

            var mainAddress = mailing.MailingAddresses.FirstOrDefault(t => t.IsDefault);

            emailMessage.To.Add(new MailboxAddress(mainAddress.Name, mainAddress.Address));

            var copyAddresses = mailing.MailingAddresses.Where(t => !t.IsDefault).Select(a => new MailboxAddress(a.Name, a.Address));

            if (copyAddresses.Any())
                emailMessage.Cc.AddRange(copyAddresses);

            emailMessage.Subject = mailing.Subject;

            var builder = new BodyBuilder
            {
                TextBody = mailing.MailingText
            };

            var fileUrl = string.Empty;

            if (stream != null)
            {
                if (fileName is null)
                    throw new PawnshopApplicationException($"Имя файла не задано");

                if (container != null)
                    fileUrl = _storage.Save(stream, container.Value, fileName).Result;
                else
                {
                    if (string.IsNullOrWhiteSpace(folderName))
                        throw new PawnshopApplicationException($"Не задана папка для сохранения в TechnicalDocuments");

                    fileUrl = _storage.SaveToFolder(stream, ContainerName.TechnicalDocuments, fileName, folderName).Result;
                }

                new FileExtensionContentTypeProvider().TryGetContentType(fileUrl, out var contentType);

                var attachment = new MimePart(contentType)
                {
                    Content = new MimeContent(stream),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = fileName
                };

                builder.Attachments.Add(attachment);
            }

            emailMessage.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect(_options.SmtpServerName, _options.SmtpServerPort, true);
                client.Authenticate(_options.NoReplyEmailName, _options.NoReplyEmailPassword);
                client.Send(emailMessage);
                client.Disconnect(true);
            }

            return fileUrl;
        }
    }
}