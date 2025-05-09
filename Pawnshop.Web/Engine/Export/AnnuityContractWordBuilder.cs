using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Storage;
using Pawnshop.Web.Models.Membership;
using static Pawnshop.Core.Constants;

namespace Pawnshop.Web.Engine.Export
{
    public class AnnuityContractWordBuilder
    {
        private readonly IStorage _storage;
        private readonly BranchContext _branchContext;
        private readonly ISessionContext _sessionContext;
        private readonly UserRepository _userRepository;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly IClientModelValidateService _clientModelValidateService;

        public AnnuityContractWordBuilder(IStorage storage, 
            BranchContext branchContext, 
            ISessionContext sessionContext, 
            UserRepository userRepository, 
            LoanPercentRepository loanPercentRepository,
            IClientModelValidateService clientModelValidateService)
        {
            _storage = storage;
            _branchContext = branchContext;
            _sessionContext = sessionContext;
            _userRepository = userRepository;
            _loanPercentRepository = loanPercentRepository;
            _clientModelValidateService = clientModelValidateService;
        }

        public async Task<Stream> Build(Contract model)
        {
            var contractPosition = model.Positions.FirstOrDefault();
            var isIndividual = contractPosition.Position.Client.LegalForm.IsIndividual;

            string template = isIndividual ? Template() : CorporateTemplate();

            var stream = await _storage.Load(template, isIndividual ? ContainerName.AnnuityTemplates : ContainerName.CorporateAnnuityTemplates);
            var user = _userRepository.Get(_sessionContext.UserId);
            var profile = _loanPercentRepository.Find(new LoanPercentQueryModel
            {
                BranchId = _branchContext.Branch.Id,
                CollateralType = model.CollateralType,
                CardType = model.ContractData.Client.CardType,
                LoanCost = model.LoanCost,
                LoanPeriod = model.LoanPeriod
            });

            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using (var document = WordprocessingDocument.Open(memoryStream, true))
            {
                var paragraphs = document.MainDocumentPart.Document.Body.Descendants<Paragraph>();
                foreach (var paragraph in paragraphs)
                {
                    var text = SearchParts(paragraph.InnerText, model, user, profile);
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
            return _branchContext.Branch.Name switch
            {
                AKU => "annuity-contract-aktau.docx",
                AKT => "annuity-contract-aktobe.docx",
                AST => "annuity-contract-astana.docx",
                ATY => "annuity-contract-atyrau.docx",
                ABA => "annuity-contract-almaty.docx",
                BZK => "annuity-contract-bzk.docx",
                KRG => "annuity-contract-karaganda.docx",
                KSK => "annuity-contract-kaskelen.docx",
                KKS => "annuity-contract-kokshetau.docx",
                KZO => "annuity-contract-kyzylorda.docx",
                OSK => "annuity-contract-oskemen.docx",
                PAV => "annuity-contract-pavlodar.docx",
                SEM => "annuity-contract-semey.docx",
                TAL => "annuity-contract-taldyk.docx",
                TRZ => "annuity-contract-taraz.docx",
                TKS => "annuity-contract-turkestan.docx",
                SHM => "annuity-contract-shymkent.docx",
                SAR => "annuity-contract-saryagash.docx",
                ALA => "annuity-contract-ala.docx",
                SRM => "annuity-contract-sayram.docx",
                ZHK => "annuity-contract-zhk.docx",
                URL => "annuity-contract-uralsk.docx",
                NUR => "annuity-contract-nursultan.docx",
                SHY => "annuity-contract-shy.docx",
                DOS => "annuity-contract-dos.docx",
                KSN => "annuity-contract-kostanay.docx",
                PET => "annuity-contract-petropavl.docx",
                TLG => "annuity-contract-talgar.docx",
                ZHA => "annuity-contract-zhanaozen.docx",
                ZHN => "annuity-contract-zhezhkazgan.docx",
                NBO => "annuity-contract-astana-bogenbay.docx",
                OKA => "annuity-contract-oskemen-kabanbay.docx",
                KBU => "annuity-contract-karagandy-buhar.docx",
                BKS => "annuity-contract-bas-kense.docx",
                TSA => "annuity-contract-tsa.docx",
                KRD => "annuity-contract-krd.docx",
                SKO => "annuity-contract-sko.docx",
                ESB => "annuity-contract-esb.docx",
                STA => "annuity-contract-sta.docx",
                _ => "annuity-contract.docx"
            };
        }

        private string CorporateTemplate()
        {
            if (_branchContext.Branch != null)
                return $"corporate-contract-{_branchContext.Branch.Name}.docx";

            return "corporate-contract.docx";
        }

        private string SearchParts(string text, Contract model, User user, LoanPercentSetting profile)
        {
            var cbDate = new DateTime(2020, 6, 30);
            bool validate = model.ContractDate.Date >= cbDate && (!model.Locked || (model.Locked && !model.PartialPaymentParentId.HasValue)) && model.Status != ContractStatus.BoughtOut;

            var position = model.Positions.FirstOrDefault();
            var positionClient = position.Position.Client;
            var isIndividual = positionClient.LegalForm.IsIndividual;

            if (validate) _clientModelValidateService.ValidateClientModel(positionClient);

            var document = positionClient.Documents?.OrderByDescending(x => x.CreateDate).FirstOrDefault(x =>
                (x.DocumentType.Code.Contains("IDENTITYCARD") || x.DocumentType.Code.Contains("PASSPORTKZ") || x.DocumentType.Code.Contains("PASSPORTRU") ||
                 x.DocumentType.Code.Contains("RESIDENCE")) && (x.DateExpire.Value.Date >= DateTime.Now.Date)) ?? positionClient.Documents.OrderByDescending(x => x.CreateDate).FirstOrDefault();

            var registrationAddress =
                positionClient.Addresses.FirstOrDefault(x => x.AddressType.Code.Contains("REGISTRATION") && x.IsActual) ?? positionClient.Addresses.OrderByDescending(x => x.CreateDate).FirstOrDefault();

            var residenceAddress =
                positionClient.Addresses.FirstOrDefault(x => x.AddressType.Code.Contains("RESIDENCE") && x.IsActual) ??
                positionClient.Addresses.OrderByDescending(x => x.CreateDate).FirstOrDefault();

            //var position = model.Positions.FirstOrDefault();

            text = ReplacePart(text, "###номер билета&", model.ContractNumber);
            text = ReplacePart(text, "###дата билета&", model.ContractDate.ToString("dd.MM.yyyy"));
            text = ReplacePart(text, "###клиент&", positionClient.FullName);
            text = ReplacePart(text, "###РНН клиента&", positionClient.IdentityNumber);
            text = ReplacePart(text, "###ИИН клиента&", positionClient.IdentityNumber);
            text = ReplacePart(text, "###номер уд&", document.Number);
            text = ReplacePart(text, "###номер документа&", document.Number);
            text = ReplacePart(text, "###выдано&", isIndividual?document.Date.Value.ToString("dd.MM.yyyy"):string.Empty);
            text = ReplacePart(text, "###кем&", isIndividual?document.Provider.Name:string.Empty);
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
            text = ReplacePart(text, "###срок займа&", model.MaturityDate.MonthDifference(model.ContractDate).ToString());
            text = ReplacePart(text, "###процент год&", decimal.ToInt32(Math.Round(model.LoanPercent * 30 * 12)).ToString());
            text = ReplacePart(text, "###процент месяц&", Math.Round(model.LoanPercent * 30, 1).ToString("N1"));
            text = ReplacePart(text, "###количество месяцев&", Math.Round(((decimal)model.LoanPeriod / 30), 0).ToString());
            text = ReplacePart(text, "###ФИО директора&", _branchContext.Configuration.LegalSettings.ChiefName);
            text = ReplacePart(text, "###правовая форма&", positionClient.LegalForm.Abbreviation);
            text = ReplacePart(text, "###ИИН/БИН&", positionClient.LegalForm.HasIINValidation ? "ИИН" : "БИН");
            text = ReplacePart(text, "###с правом/без права&", position.CategoryId == WITH_DRIVE_RIGHT_CATEGORY ? "с правом" : "без права");

            if (model.CollateralType == CollateralType.Car)
            {
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
            text = ReplacePart(text, "###мораторий&", ((profile?.MinLoanPeriod ?? 90) / 30).ToString());
            return text;
        }

        private string ReplacePart(string text, string part, string replace)
        {
            var regex = new Regex(part);
            return regex.Replace(text, replace ?? string.Empty);
        }
    }
}
