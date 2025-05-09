using System;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Pawnshop.Data.Models.Contracts;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Services.Storage;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using System.Text.RegularExpressions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.Clients;
using static Pawnshop.Core.Constants;

namespace Pawnshop.Web.Engine.Export
{
    public class ContractWordBuilder
    {
        private readonly IStorage _storage;
        private readonly BranchContext _branchContext;
        private readonly ISessionContext _sessionContext;
        private readonly UserRepository _userRepository;
        private readonly IClientModelValidateService _clientModelValidateService;

        public ContractWordBuilder(IStorage storage, 
            BranchContext branchContext, 
            ISessionContext sessionContext, 
            UserRepository userRepository, 
            IClientModelValidateService clientModelValidateService)
        {
            _storage = storage;
            _branchContext = branchContext;
            _sessionContext = sessionContext;
            _userRepository = userRepository;
            _clientModelValidateService = clientModelValidateService;
        }

        public async Task<Stream> Build(Contract model)
        {
            var template = Template();
            var stream = await _storage.Load(template, ContainerName.Templates);
            var user = _userRepository.Get(_sessionContext.UserId);
            
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using (var document = WordprocessingDocument.Open(memoryStream, true))
            {
                var paragraphs = document.MainDocumentPart.Document.Body.Descendants<Paragraph>();
                foreach (var paragraph in paragraphs)
                {
                    var text = SearchParts(paragraph.InnerText, model, user);
                    if (!paragraph.InnerText.Equals(text))
                    {
                        var properties = paragraph.Descendants<RunProperties>().FirstOrDefault();
                        
                        paragraph.RemoveAllChildren<Run>();
                        var run = paragraph.AppendChild(new Run());
                        if (properties != null)
                        {
                            run.RunProperties = (RunProperties)properties.Clone();
                        }
                        run.AppendChild(new Text(text));
                    }
                }

                return memoryStream;
            }
        }

        private string Template()
        {
            switch (_branchContext.Branch.Name)
            {
                case AKU:
                    return "contract-aktau.docx";
                case AKT:
                    return "contract-aktobe.docx";
                case AST:
                    return "contract-astana.docx";
                case ATY:
                    return "contract-atyrau.docx";
                case ABA:
                    return "contract-almaty.docx";
                case BZK:
                    return "contract-bzk.docx";
                case KRG:
                    return "contract-karaganda.docx";
                case KSK:
                    return "contract-kaskelen.docx";
                case KKS:
                    return "contract-kokshetau.docx";
                case KZO:
                    return "contract-kyzylorda.docx";
                case OSK:
                    return "contract-oskemen.docx";
                case PAV:
                    return "contract-pavlodar.docx";
                case SEM:
                    return "contract-semey.docx";
                case TAL:
                    return "contract-taldyk.docx";
                case TRZ:
                    return "contract-taraz.docx";
                case TKS:
                    return "contract-turkestan.docx";
                case SHM:
                    return "contract-shymkent.docx";
                case SAR:
                    return "contract-saryagash.docx";
                case ALA:
                    return "contract-ala.docx";
                case SRM:
                    return "contract-sayram.docx";
                case ZHK:
                    return "contract-zhk.docx";
                case URL:
                    return "contract-uralsk.docx";
                case NUR:
                    return "contract-nursultan.docx";
                case SHY:
                    return "contract-shy.docx";
                case DOS:
                    return "contract-dos.docx";
                case KSN:
                    return "contract-kostanay.docx";
                case PET:
                    return "contract-petropavl.docx";
                case TLG:
                    return "contract-talgar.docx";
                case ZHA:
                    return "contract-zhanaozen.docx";
                case ZHN:
                    return "contract-zhezhkazgan.docx";
                case NBO:
                    return "contract-astana-bogenbay.docx";
                case OKA:
                    return "contract-oskemen-kabanbay.docx";
                case KBU:
                    return "contract-karagandy-buhar.docx";
                case BKS:
                    return "contract-bas-kense.docx";
                case TSA:
                    return "contract-tsa.docx";
                case KRD:
                    return "contract-krd.docx";
                case SKO:
                    return "contract-sko.docx";
                case ESB:
                    return "contract-esb.docx";
                case STA:
                    return "contract-sta.docx";
                default:
                    return "contract.docx";
            }
        }

        private string SearchParts(string text, Contract model, User user)
        {
            var cbDate = new DateTime(2020, 6, 30);
            bool validate = model.ContractDate.Date >= cbDate && (!model.Locked || (model.Locked && !model.PartialPaymentParentId.HasValue)) && model.Status != ContractStatus.BoughtOut;

            if (validate) _clientModelValidateService.ValidateClientModel(model.Client);

            var document = model.Client.Documents?.OrderByDescending(x=>x.CreateDate).FirstOrDefault(x =>
                (x.DocumentType.Code.Contains("IDENTITYCARD") || x.DocumentType.Code.Contains("PASSPORTKZ") || x.DocumentType.Code.Contains("PASSPORTRU") ||
                 x.DocumentType.Code.Contains("RESIDENCE")) && (x.DateExpire.Value.Date >= DateTime.Now.Date)) ?? model.Client.Documents.OrderByDescending(x => x.CreateDate).FirstOrDefault();

            if (!model.Client.LegalForm.IsIndividual)
                document = model.Client.Documents?.OrderByDescending(x => x.CreateDate).FirstOrDefault(x =>
                    x.DocumentType.Code.Contains("LEGALENTITYREGISTRATION") && (x.DateExpire.Value.Date >= DateTime.Now.Date)) ?? model.Client.Documents.OrderByDescending(x => x.CreateDate).FirstOrDefault();

            var registrationAddress =
                model.Client.Addresses.FirstOrDefault(x => x.AddressType.Code.Contains("REGISTRATION") && x.IsActual) ?? model.Client.Addresses.OrderByDescending(x => x.CreateDate).FirstOrDefault();

            var residenceAddress =
                model.Client.Addresses.FirstOrDefault(x => x.AddressType.Code.Contains("RESIDENCE") && x.IsActual) ?? 
                model.Client.Addresses.OrderByDescending(x => x.CreateDate).FirstOrDefault();

            if (document == null) throw new PawnshopApplicationException("Не найден ни один документ у клиента");
            if (registrationAddress == null || residenceAddress == null) throw new PawnshopApplicationException("Не найден ни один адрес у клиента");

            text = ReplacePart(text, "###номер билета&", model.ContractNumber);
            text = ReplacePart(text, "###дата билета&", model.ContractDate.ToString("dd.MM.yyyy"));
            text = ReplacePart(text, "###клиент&", model.Client.FullName);
            text = ReplacePart(text, "###РНН клиента&", model.Client.IdentityNumber);
            text = ReplacePart(text, "###ИИН клиента&", model.Client.IdentityNumber);
            text = ReplacePart(text, "###номер уд&", document.Number);
            text = ReplacePart(text, "###номер документа&", document.Number);
            text = ReplacePart(text, "###выдано&", document.Date.Value.ToString("dd.MM.yyyy"));
            text = ReplacePart(text, "###кем&", document.Provider.Name);
            text = ReplacePart(text, "###адресс клиента&", registrationAddress.FullPathRus);
            text = ReplacePart(text, "###адресс регистрации клиента рус&", registrationAddress.FullPathRus);
            text = ReplacePart(text, "###адресс регистрации клиента каз&", registrationAddress.FullPathKaz);
            text = ReplacePart(text, "###адресс текущий клиента рус&", residenceAddress.FullPathRus);
            text = ReplacePart(text, "###адресс текущий клиента каз&", residenceAddress.FullPathKaz);
            text = ReplacePart(text, "###название ломбарда&", _branchContext.Configuration.LegalSettings.LegalName);
            text = ReplacePart(text, "###оценка&", model.EstimatedCost.ToString());
            text = ReplacePart(text, "###сумма оценки&", model.EstimatedCost.ToWords());
            text = ReplacePart(text, "###ссуда&", model.LoanCost.ToString());
            (string intPart, string floatPart) = model.LoanCost.ToWords();
            text = ReplacePart(text, "###сумма ссуды&", string.IsNullOrWhiteSpace(floatPart) ? $"{intPart} тенге" : $"{intPart} тенге, {floatPart} тиын");
            text = ReplacePart(text, "###дата возврата&", model.OriginalMaturityDate.ToString("dd.MM.yyyy"));
            text = ReplacePart(text, "###процент&", model.LoanPercent.ToString("n4"));
            text = ReplacePart(text, "###телефон ломбарда&", _branchContext.Configuration.ContactSettings.Phone);
            text = ReplacePart(text, "###ФИО директора&", _branchContext.Organization.Configuration.LegalSettings.ChiefName);
            text = ReplacePart(text, "###годовой процент&", Math.Round((model.LoanPercent*30*12),1).ToString());
            text = ReplacePart(text, "###БИН филиала&", _branchContext.Configuration.LegalSettings.BIN);
            text = ReplacePart(text, "###БИК банка&", _branchContext.Configuration.BankSettings.BankBik);
            text = ReplacePart(text, "###количество месяцев&", model.ProductTypeId.HasValue ? $"{Math.Round(((decimal)model.LoanPeriod / 30), 0)} месяцев" : "1 месяц");
            text = ReplacePart(text, "###номер счета&", _branchContext.Configuration.BankSettings.BankAccount);
            if (model.CollateralType == CollateralType.Car)
            {
                var position = model.Positions.FirstOrDefault();
                if (position != null)
                {
                    var car = (Car)position.Position;
                    text = ReplacePart(text, "###марка&", car.Mark);
                    text = ReplacePart(text, "###модель&", car.Model);
                    text = ReplacePart(text, "###номер&", car.TransportNumber);
                    text = ReplacePart(text, "###год выпуска&", car.ReleaseYear.ToString());
                    text = ReplacePart(text, "###двигатель&", car.MotorNumber);
                    text = ReplacePart(text, "###кузов&", car.BodyNumber);
                    text = ReplacePart(text, "###цвет&", car.Color);
                    text = ReplacePart(text, "###номер тс&", car.TechPassportNumber);
                    text = ReplacePart(text, "###дата тс&", (car.TechPassportDate.HasValue ? car.TechPassportDate.Value.ToString("dd.MM.yyyy") : string.Empty));
                }
            }
            else if (model.CollateralType == CollateralType.Machinery)
            {
                var position = model.Positions.FirstOrDefault();
                if (position != null)
                {
                    var machinery = (Machinery)position.Position;
                    text = ReplacePart(text, "###марка&", machinery.Mark);
                    text = ReplacePart(text, "###модель&", machinery.Model);
                    text = ReplacePart(text, "###номер&", machinery.TransportNumber);
                    text = ReplacePart(text, "###год выпуска&", machinery.ReleaseYear.ToString());
                    text = ReplacePart(text, "###двигатель&", machinery.MotorNumber);
                    text = ReplacePart(text, "###кузов&", machinery.BodyNumber);
                    text = ReplacePart(text, "###цвет&", machinery.Color);
                    text = ReplacePart(text, "###номер тс&", machinery.TechPassportNumber);
                    text = ReplacePart(text, "###дата тс&", (machinery.TechPassportDate.HasValue ? machinery.TechPassportDate.Value.ToString("dd.MM.yyyy") : string.Empty));
                }
            }
            text = ReplacePart(text, "###адресс ломбарда&", _branchContext.Configuration.ContactSettings.Address);
            text = ReplacePart(text, "###эксперт&", user.Fullname);
            return text;
        }

        private string ReplacePart(string text, string part, string replace)
        {
            var regex = new Regex(part);
            return regex.Replace(text, replace ?? string.Empty);
        }
    }
}
