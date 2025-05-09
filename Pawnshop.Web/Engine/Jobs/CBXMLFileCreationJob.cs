using System;
using System.Collections.Generic;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Web.Engine.MessageSenders;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using System.Dynamic;
using Pawnshop.Core.Exceptions;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Core;
using System.IO;
using System.IO.Compression;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Pawnshop.Data.Models.CreditBureaus;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Storage;
using System.Globalization;
using Hangfire;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Web.Engine.Jobs
{
    public class CBXMLFileCreationJob
    {
        private readonly CBBatchRepository _cbBatchRepository;
        private readonly CBContractRepository _cbContractRepository;
        private readonly ContractRepository _contractRepository;
        private readonly EventLog _eventLog;
        private readonly EnviromentAccessOptions _options;
        private readonly IStorage _storage;
        private readonly JobLog _jobLog;

        private string dateTemplate = "yyyy-MM-dd";
        private NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
        private const int creditLineFundingType = 20;

        public CBXMLFileCreationJob(
            IOptions<EnviromentAccessOptions> options, 
            EventLog eventLog, 
            ContractRepository contractRepository, 
            CBBatchRepository cbBatchRepository,
            CBContractRepository cbContractRepository, 
            IStorage storage, JobLog jobLog
            )
        {
            _contractRepository = contractRepository;
            _eventLog = eventLog;
            _options = options.Value;
            _cbBatchRepository = cbBatchRepository;
            _cbContractRepository = cbContractRepository;
            _storage = storage;
            _jobLog = jobLog;

            nfi.NumberDecimalSeparator = ".";
        }

        [Queue("cb")]
        public void Execute()
        {
            if (!_options.CBUpload) return;

            try
            {
                _jobLog.Log("CBXMLFileCreationJob", JobCode.Begin, JobStatus.Success);

                var batches = _cbBatchRepository.List(new ListQuery() { Page = null },
                    new { Status = CBBatchStatus.Fulfilled });

                foreach (var batch in batches)
                {
                    try
                    {
                        var xml = CreateXML(batch);

                        var name = $@"{batch.Id}";
                        
                        //XmlReaderSettings settings = new XmlReaderSettings();
                        //settings.Schemas.Add("http://www.datapump.cig.com","records.xsd");
                        //settings.ValidationType = ValidationType.Schema;

                        //XmlReader reader = XmlReader.Create(new StringReader(xml.ToString()) , settings);
                        //reader.Read();
                        XmlDocument doc = new XmlDocument();
                        //doc.Load(reader);
                        doc.LoadXml(xml.ToString());

                        //doc.Validate(settingsValidationEventHandler);

                        using (MemoryStream zipStream = new MemoryStream())
                        {
                            using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                            {
                                var entry = zip.CreateEntry($"batch{name}.xml", CompressionLevel.Optimal);
                                using (StreamWriter sw = new StreamWriter(entry.Open()))
                                {
                                    sw.Write(xml.ToString());
                                }
                            }

                            zipStream.Flush();
                            zipStream.Position = 0;

                            var fileName = _storage.Save(zipStream, ContainerName.CBBatches, $@"batch{name}.zip").Result;

                            batch.FileName = fileName;
                        }


                        _eventLog.Log(EventCode.CBXMLCreation, EventStatus.Success, EntityType.CBBatch, batch.Id);

                        batch.BatchStatusId = CBBatchStatus.XMLCreated;
                    }
                    catch (Exception e)
                    {
                        batch.BatchStatusId = CBBatchStatus.XMLCreationError;
                        _eventLog.Log(EventCode.CBXMLCreation, EventStatus.Failed, EntityType.CBBatch, batch.Id,
                            e.Message, e.StackTrace);
                    }
                    finally
                    {
                        _cbBatchRepository.Update(batch);
                    }
                }

                _jobLog.Log("CBXMLFileCreationJob", JobCode.End, JobStatus.Success);
            }
            catch (Exception e)
            {
                _jobLog.Log("CBXMLFileCreationJob", JobCode.Error, JobStatus.Failed, responseData: e.Message);
            }
            finally
            {
                BackgroundJob.Enqueue<CBBatchUploadJob>(x => x.Execute());
            }

        }

        private StringBuilder CreateXML(CBBatch batch)
        {
            var contracts = _cbContractRepository.List(new ListQuery() { Page = null }, new { BatchId = batch.Id });
            StringBuilder xml = new StringBuilder();

            xml.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xml.Append("<Records xmlns=\"http://www.datapump.cig.com\">");
            contracts.ForEach(c =>
            {
                var contract = _cbContractRepository.Get(c.Id);
                bool isCollateralTypeCar = _contractRepository.GetOnlyContract(contract.ContractId).CollateralType == CollateralType.Car;

                xml.Append($"<Contract {(contract.OperationId.HasValue ? ($"operation=\"{contract.OperationId}\"") : string.Empty)}>");
                xml.Append("<General>");
                xml.Append($"<ContractCode>{contract.ContractCode}</ContractCode>");
                xml.Append($"<AgreementNumber>{contract.AgreementNumber}</AgreementNumber>");
                //xml.Append($"<ProviderGroupExplicit>true</ProviderGroupExplicit>"); //TODO: необязательное поле?
                xml.Append($"<FundingType id=\"{contract.FundingType}\"/>");
                xml.Append($"<FundingSource id=\"{contract.FundingSource}\"/>");
                xml.Append($"<CreditPurpose2 id=\"{contract.CreditPurpose}\"/>");
                xml.Append($"<CreditObject id=\"{(isCollateralTypeCar == true ? "04" : contract.CreditObject)}\"/>");
                xml.Append($"<ContractPhase id=\"{contract.ContractPhase}\"/>");
                xml.Append($"<ContractStatus id=\"{contract.ContractStatus}\">");
                if (!string.IsNullOrEmpty(contract.ThirdPartyHolder))
                    xml.Append($"<ThirdPartyHolder language=\"kk-KZ\">{contract.ThirdPartyHolder}</ThirdPartyHolder>");

                xml.Append($"</ContractStatus>");
                //xml.Append($"<ApplicationDate>2005-12-31</ApplicationDate>");
                xml.Append($"<StartDate>{contract.StartDate.Value.ToString(dateTemplate)}</StartDate>");
                xml.Append($"<EndDate>{contract.EndDate.Value.ToString(dateTemplate)}</EndDate>");
                xml.Append($"<ActualDate>{contract.ActualDate.Value.ToString(dateTemplate)}</ActualDate>");
                if (contract.AvailableDate.HasValue)
                {
                    xml.Append($"<AvailableDate>{contract.AvailableDate.Value.ToString(dateTemplate)}</AvailableDate>");
                }

                if (contract.RealPaymentDate.HasValue)
                    xml.Append($"<RealPaymentDate>{contract.RealPaymentDate.Value.ToString(dateTemplate)}</RealPaymentDate>");

                xml.Append($"<SpecialRelationship id=\"{contract.SpecialRelationship}\"/>");

                if (contract.AnnualEffectiveRate.HasValue)
                    xml.Append($"<AnnualEffectiveRate>{contract.AnnualEffectiveRate.Value.ToString(nfi)}</AnnualEffectiveRate>");

                if (contract.NominalRate.HasValue)
                    xml.Append($"<NominalRate>{contract.NominalRate.Value.ToString(nfi)}</NominalRate>");

                //xml.Append($"<AmountProvisions currency=\"ISK\">100</AmountProvisions>");
                //xml.Append($"<LoanAccount>");
                //    xml.Append($"<Text language=\"en-GB\">Some text</Text>");
                //    xml.Append($"<Text language=\"ru-RU\">Some text</Text>");
                //    xml.Append($"<Text language=\"kk-KZ\">Some text</Text>");
                //xml.Append($"</LoanAccount>");
                //xml.Append($"<GracePrincipal id=\"2\"/>");
                //xml.Append($"<GracePay id=\"2\"/>");
                //xml.Append($"<PlaceOfDisbursement locationId=\"0\" katoId=\"0\"/>");
                xml.Append($"<Classification id=\"{contract.Classification}\"/>");
                if (!string.IsNullOrEmpty(contract.ParentContractCode))
                    xml.Append($"<ParentContractCode>{contract.ParentContractCode}</ParentContractCode>");

                if (contract.ParentProvider.HasValue)
                    xml.Append($"<ParentProvider id=\"{contract.ParentProvider}\"/>");

                if (contract.ParentContractStatus.HasValue)
                    xml.Append($"<ParentContractStatus id=\"{contract.ParentContractStatus}\"/>");

                if (contract.ParentOperationDate.HasValue)
                    xml.Append($"<ParentOperationDate>{contract.ParentOperationDate.Value.ToString(dateTemplate)}</ParentOperationDate>");

                if (contract.ProlongationCount.HasValue)
                    xml.Append($"<ProlongationCount>{contract.ProlongationCount}</ProlongationCount>");

                //xml.Append($"<BranchLocation id=\"2\"/>");
                //Collaterals
                if (contract.Collaterals != null && contract.Collaterals.Count > 0)
                { 
                    xml.Append("<Collaterals>");
                    //Collateral
                    contract.Collaterals.ForEach(collateral =>
                    {
                        xml.Append($"<Collateral typeId=\"{collateral.TypeId}\">");
                        xml.Append($"<Location {(collateral.LocationId.HasValue ? ($"id=\"{collateral.LocationId}\" ") : string.Empty)}katoId=\"{collateral.KATOID}\" />");
                        xml.Append($"<Value currency=\"{collateral.Currency}\" typeId=\"{collateral.ValueTypeId}\">{collateral.Value.ToString(nfi)}</Value>");
                        xml.Append($"</Collateral>");
                    });
                    xml.Append("</Collaterals>");
                }

                //Subjects
                xml.Append("<Subjects>");
                contract.Subjects.ForEach(subject =>
                {
                    xml.Append($"<Subject roleId=\"{subject.RoleId}\">");
                    xml.Append($"<Entity>");
                    if (subject.IsIndividual)
                    {
                        var client = (CBSubjectIndividual)subject;
                        xml.Append($"<Individual>");
                        xml.Append($"<FirstName>");
                        xml.Append($"<Text language=\"kk-KZ\">{client.FirstName}</Text>");
                        xml.Append($"</FirstName>");
                        xml.Append($"<Surname>");
                        xml.Append($"<Text language=\"kk-KZ\">{client.Surname}</Text>");
                        xml.Append($"</Surname>");
                        if (!string.IsNullOrEmpty(client.FathersName))
                        {
                            xml.Append($"<FathersName>");
                            xml.Append($"<Text language=\"kk-KZ\">{client.FathersName}</Text>");
                            xml.Append($"</FathersName>");
                        }

                        xml.Append($"<Gender>{client.Gender}</Gender>");
                        xml.Append($"<Classification id=\"{client.Classification}\"/>");
                        xml.Append($"<Residency id=\"{client.Residency}\"/>");
                        //xml.Append($"<Education id=\"1\"/>");
                        //xml.Append($"<MaritalStatus id=\"1\"/>");
                        xml.Append($"<DateOfBirth>{client.DateOfBirth.ToString(dateTemplate)}</DateOfBirth>");
                        //xml.Append($"<NegativeStatus id=\"1\"/>");
                        //xml.Append($"<Profession id=\"1\"/>");
                        //xml.Append($"<EconomyActivityGroup id=\"3\"/>");
                        //xml.Append($"<EmploymentNature id=\"2\"/>");
                        xml.Append($"<Citizenship id=\"{client.Citizenship}\"/>");
                        //xml.Append($"<Salary id=\"2\"/>");
                        //TODO: доделать документы

                        xml.Append($"<Identifications>");
                        client.Identifications.ForEach(identification =>
                                    {
                                        BuildIdentification(xml, identification);
                                    });
                        xml.Append($"</Identifications>");
                        //TODO: доделать адреса

                        xml.Append($"<Addresses>");
                        client.Addresses.ForEach(address =>
                                    {
                                        BuildAddress(xml, address);
                                    });
                        xml.Append($"</Addresses>");
                        /// TODO: необязательные поля?
                        if (batch.CBId == CBType.SCB)
                        {
                            xml.Append($"<Communications>");
                            xml.Append($" ");
                            //////    xml.Append($"<Communication typeId=\"1\">+3545509600</Communication>");
                            //////    xml.Append($"<Communication typeId=\"5\">something@something.com</Communication>");
                            xml.Append($"</Communications>");
                            xml.Append($"<Dependants>");
                            xml.Append($" ");
                            //////    xml.Append($"<Dependant count=\"3\"typeId=\"2\"/>");
                            xml.Append($"</Dependants>");
                        }

                        xml.Append($"</Individual>");
                    }
                    else
                    {
                        var client = (CBSubjectCompany)subject;
                        xml.Append($"<Company>");
                        xml.Append($"<Name>");
                        xml.Append($"<Text language=\"kk-KZ\">{client.Name}</Text>");
                        xml.Append($"</Name>");
                        xml.Append($"<Status id=\"{client.Status}\"/>");
                        xml.Append($"<TradeName>");
                        xml.Append($"<Text language=\"kk-KZ\">{client.TradeName}</Text>");
                        xml.Append($"</TradeName>");
                        xml.Append($"<Abbrevation>");
                        xml.Append($"<Text language=\"kk-KZ\">{client.Abbreviation}</Text>");
                        xml.Append($"</Abbrevation>");
                        xml.Append($"<LegalForm id=\"{client.LegalForm}\"/>");
                        //xml.Append($"<Ownership id=\"1\"/>");
                        xml.Append($"<Nationality id=\"{client.Nationality}\"/>");
                        xml.Append($"<RegistrationDate>{client.RegistrationDate.ToString(dateTemplate)}</RegistrationDate>");
                        //xml.Append($"<EmployeeCount id=\"2\"/>");
                        //xml.Append($"<EconomicActivity id=\"3\"/>");
                        //TODO: доделать адреса

                        xml.Append($"<Addresses>");
                        client.Addresses.ForEach(address =>
                                    {
                                        BuildAddress(xml, address);
                                    });
                        xml.Append($"</Addresses>");
                        xml.Append($"<Identifications>");
                        client.Identifications.ForEach(identification =>
                                    {
                                        BuildIdentification(xml, identification);
                                    });
                        xml.Append($"</Identifications>");
                        /// /// TODO: необязательные поля?
                        //xml.Append($"<Communications>");
                        //    xml.Append($"<Communication typeId=\"1\">+3545509600</Communication>");
                        //    xml.Append($"<Communication typeId=\"5\">something@something.com</Communication>");
                        //xml.Append($"</Communications>");
                        xml.Append($"<Management>");
                        xml.Append($"<CEO>");
                        xml.Append($"<FirstName>");
                        xml.Append($"<Text language=\"kk-KZ\">{client.CEOFirstName}</Text>");
                        xml.Append($"</FirstName>");
                        xml.Append($"<Surname>");
                        xml.Append($"<Text language=\"kk-KZ\">{client.CEOSurname}</Text>");
                        xml.Append($"</Surname>");
                        if (!string.IsNullOrEmpty(client.CEOFathersName))
                        {
                            xml.Append($"<FathersName>");
                            xml.Append($"<Text language=\"kk-KZ\">{client.CEOFathersName}</Text>");
                            xml.Append($"</FathersName>");
                        }

                        xml.Append($"<DateOfBirth>{client.CEODateOfBirth.ToString(dateTemplate)}</DateOfBirth>");

                        //TODO: доделать документы
                        xml.Append($"<Identifications>");
                        client.CEOIdentifications.ForEach(identification =>
                                    {
                                        BuildIdentification(xml, identification);
                                    });
                        xml.Append($"</Identifications>");
                        xml.Append($"</CEO>");
                        xml.Append($"</Management>");
                        xml.Append($"</Company>");
                    }
                    xml.Append($"</Entity>");
                    xml.Append("</Subject>");

                });
                xml.Append("</Subjects>");

                xml.Append("</General>");
                xml.Append("<Type>");
                switch (contract.FundingType)
                {
                    case creditLineFundingType:
                        BuildCreditLineInstallment(xml, contract.Installment);
                        break;
                    default:
                        BuildLoanAndTrancheInstallment(xml, contract.Installment);
                        break;
                }
                xml.Append("</Type>");
                xml.Append("</Contract>");
            });
            xml.Append("</Records>");

            return xml;
        }

        private void BuildCreditLineInstallment(StringBuilder xml, CBInstallment installment)
        {
            xml.Append($"<Credit paymentMethodId =\"{installment.PaymentMethodId}\">");
            xml.Append($"<CreditLimit currency=\"{installment.Currency}\">{installment.TotalAmount.ToString(nfi)}</CreditLimit>");
            xml.Append($"<Records>");
            installment.Records.ForEach(record =>
            {
                xml.Append($"<Record accountingDate=\"{record.AccountingDate.ToString(dateTemplate)}\">");
                xml.Append($"<AvailableLimit currency=\"{installment.Currency}\">{record.AvailableLimit.ToString(nfi)}</AvailableLimit>");
                xml.Append($"</Record>");
            });
            xml.Append($"</Records>");
            xml.Append($"</Credit>");
        }

        private void BuildLoanAndTrancheInstallment(StringBuilder xml, CBInstallment installment)
        {
            xml.Append($"<Instalment paymentMethodId =\"{installment.PaymentMethodId}\" paymentPeriodId=\"{installment.PaymentPeriodId}\">");
            xml.Append($"<TotalAmount currency=\"{installment.Currency}\">{installment.TotalAmount.ToString(nfi)}</TotalAmount>");
            xml.Append($"<InstalmentAmount currency=\"{installment.Currency}\">{installment.InstallmentAmount.ToString(nfi)}</InstalmentAmount>");
            xml.Append($"<InstalmentCount>{installment.InstallmentCount}</InstalmentCount>");
            xml.Append($"<Records>");
            installment.Records.ForEach(record =>
            {
                xml.Append($"<Record accountingDate=\"{record.AccountingDate.ToString(dateTemplate)}\">");
                xml.Append($"<OutstandingInstalmentCount>{record.OutstandingInstallmentCount}</OutstandingInstalmentCount>");
                xml.Append($"<OutstandingAmount currency=\"{installment.Currency}\">{record.OutstandingAmount.ToString(nfi)}</OutstandingAmount>");
                xml.Append($"<OverdueInstalmentCount>{record.OverdueInstallmentCount}</OverdueInstalmentCount>");
                xml.Append($"<OverdueAmount currency=\"{installment.Currency}\">{record.OverdueAmount.ToString(nfi)}</OverdueAmount>");
                xml.Append($"<Fine currency=\"{installment.Currency}\">{record.Fine.ToString(nfi)}</Fine>");
                xml.Append($"<Penalty currency=\"{installment.Currency}\">{record.Penalty.ToString(nfi)}</Penalty>");
                if (record.ProlongationStartDate.HasValue)
                    xml.Append($"<ProlongationStartDate>{record.ProlongationStartDate.Value.ToString(dateTemplate)}</ProlongationStartDate>");

                if (record.ProlongationEndDate.HasValue)
                    xml.Append($"<ProlongationEndDate>{record.ProlongationEndDate.Value.ToString(dateTemplate)}</ProlongationEndDate>");

                if (record.LastPaymentDate.HasValue)
                    xml.Append($"<LastPaymentDate>{record.LastPaymentDate.Value.ToString(dateTemplate)}</LastPaymentDate>");

                xml.Append($"</Record>");
            });
            xml.Append($"</Records>");
            xml.Append("</Instalment>");
        }

        private void BuildAddress(StringBuilder xml, CBAddress address)
        {
            xml.Append($"<Address typeId=\"{address.TypeId}\" {(address.LocationId.HasValue ? ($"locationId=\"{address.LocationId}\" ") : string.Empty)}{(!string.IsNullOrEmpty(address.KATOID) ? ($"katoId=\"{address.KATOID}\" ") : string.Empty)}>");
            xml.Append($"<StreetName>");
            xml.Append($"<Text language=\"kk-KZ\">{address.StreetName}</Text>");
            xml.Append($"</StreetName>");
            //xml.Append($"<Streetnumber>123</Streetnumber>");
            //xml.Append($"<PostBox>123</PostBox>");
            //xml.Append($"<AdditionalInformation>");
            //    xml.Append($"<Text language=\"en-GB\">More</Text>");
            //    xml.Append($"<Text language=\"en-KZ\">Street name</Text>");
            //xml.Append($"</AdditionalInformation>");
            //xml.Append($"<PostalCode>3962</PostalCode>");
            xml.Append($"</Address>");
        }

        private void BuildIdentification(StringBuilder xml, CBIdentification identification)
        {
            xml.Append($"<Identification typeId=\"{identification.TypeId}\" rank=\"{identification.Rank}\">");
            xml.Append($"<Number>{identification.Number}</Number>");
            xml.Append($"<RegistrationDate>{identification.RegistrationDate.ToString(dateTemplate)}</RegistrationDate>");
            if (identification.IssueDate.HasValue) 
                xml.Append($"<IssueDate>{identification.IssueDate.Value.ToString(dateTemplate)}</IssueDate>");

            if (identification.ExpirationDate.HasValue) 
                xml.Append($"<ExpirationDate>{identification.ExpirationDate.Value.ToString(dateTemplate)}</ExpirationDate>");

            //xml.Append($"<IssuedBy id=\"675\" katoId=\"750000000\"/>");
            if (!string.IsNullOrEmpty(identification.DocumentTypeText)) 
                xml.Append($"<DocumentTypeText>{identification.DocumentTypeText}</DocumentTypeText>");

            xml.Append($"</Identification>");
        }
        void settingsValidationEventHandler(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Warning)
                _eventLog.Log(EventCode.CBXMLCreation, EventStatus.Failed, EntityType.CBBatch, responseData: $@"WARNING: {e.Message}");
            else if (e.Severity == XmlSeverityType.Error)
                _eventLog.Log(EventCode.CBXMLCreation, EventStatus.Failed, EntityType.CBBatch, responseData: $@"ERROR: {e.Message}");
        }
    }
}
