using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Extensions;
using Pawnshop.Core;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Data.Models.OnlineApplications;
using Pawnshop.Web.Models.AbsOnline;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pawnshop.Web.Converters
{
    public static class OnlineApplicationConverter
    {
        public static OnlineApplication ToDomainModel(
            this CreateApplicationRequest rq,
            OnlineApplication application,
            int productId,
            int clientId,
            int? creditLineProductId = null,
            int? creditLineId = null,
            DateTime? endDate = null,
            bool isOpeningCreditLine = false
            )
        {
            application ??= new OnlineApplication();
            application.ClientId = clientId;
            application.ContractNumber = rq.ContractNumber;
            application.CreditLineId = creditLineId;
            application.CreditLineSettingId = creditLineProductId;
            application.IsOpeningCreditLine = isOpeningCreditLine;
            application.LoanCost = rq.LoanCost;
            application.MaturityDate = endDate.HasValue ? endDate.Value : DateTime.Today.AddMonths(rq.Period);
            application.PartnerCode = rq.PartnerCode.IsNullOrEmpty(application.PartnerCode);
            application.Period = rq.Period;
            application.Position = rq.ToPositionDomainModel(application.Position);
            application.SettingId = productId;
            application.Source = rq.ApplicationSource;
            application.UtmTags = rq.UtmTags;
            application.WithInsurance = rq.Insurance;

            return application;
        }

        public static OnlineApplication ToDomainModel(
            this UpdateApplicationRequest rq,
            OnlineApplication application,
            int productId,
            int? creditLineProductId
            )
        {
            application ??= new OnlineApplication();
            application.ContractNumber = rq.ContractNumber;
            application.Period = rq.Period ?? application.Period;
            application.Position = rq.ToPositionDomainModel(application.Position);
            application.SettingId = productId;
            application.CreditLineSettingId = creditLineProductId ?? application.CreditLineSettingId;

            if (application.IsOpeningCreditLine)
                application.MaturityDate = DateTime.Today.AddMonths(application.Period);

            return application;
        }

        public static OnlineApplication ToDomainModel(this ApplicationVerificationResultRequest rq, OnlineApplication application)
        {
            application.LoanCost = rq.LoanCost;
            application.LTV = rq.LTV;
            application.PartnerCode = rq.PartnerCode.IsNullOrEmpty(application.PartnerCode);
            application.Position = rq.ToPositionDomainModel(application.Position);
            application.WithInsurance = rq.Insurance ?? application.WithInsurance;

            return application;
        }

        public static ApplicationResponse ToApplicationResponse(
            this OnlineApplication application,
            Client client,
            ClientRequisite clientRequisite,
            Client bankInfo
            )
        {
            var document = client.Documents?.FirstOrDefault(x => x.Id == application.ClientDocumentId);
            var addresses = client.Addresses?.FirstOrDefault(x => x.IsActual && x.AddressTypeId == 5);
            var car = application.Position?.Car;

            return new ApplicationResponse
            {
                AdditionalPhone = client.StaticPhone,
                Address = addresses?.FullPathRus,
                BankName = bankInfo?.FullName,
                BirthDate = client.BirthDay,
                Car = car != null ? $"{car.Mark} {car.Model}" : string.Empty,
                CarColor = car?.Color ?? string.Empty,
                CarCost = application.Position?.LoanCost ?? 0,
                CarId = car?.TransportNumber ?? string.Empty,
                CarVin = car?.BodyNumber ?? string.Empty,
                CarYear = car?.ReleaseYear,
                ClientAddress = addresses?.FullPathRus,
                ClientName = client.FullName,
                ContractNumber = application.ContractNumber,
                Email = client.Email,
                Firstname = client.Name,
                Iban = clientRequisite?.Value,
                IIN = client.IdentityNumber,
                Lastname = client.Surname,
                LoanCost = application.LoanCost,
                MarketCost = application.Position?.EstimatedCost,
                Middlename = client.Patronymic,
                MobilePhone = client.MobilePhone ?? string.Empty,
                PartnerCode = application.PartnerCode,
                PassportIssueDate = document?.Date,
                PassportIssuer = document?.Provider?.Name,
                PassportNumber = document?.Number,
                ProductId = application.SettingId.ToString(),
                TechPassportIssueDate = car?.TechPassportDate,
                TechPassportNumber = car?.TechPassportNumber ?? string.Empty,
                RefinanceList = application.OnlineApplicationRefinances.Select(refinance => refinance.RefinancedContractNumber).ToList()
            };
        }

        public static Contract ToContractDomainModel(
            this OnlineApplication application,
            Contract contract,
            LoanPercentSetting product,
            Client client,
            int periodTypeId,
            int tsoBranchId,
            int carId,
            int? creditLineId = null
            )
        {
            var loanCoast = product.ContractClass switch
            {
                ContractClass.CreditLine => application.Position.LoanCost.Value,
                ContractClass.Tranche => application.LoanCost,
                ContractClass.Credit => application.LoanCost
            };

            contract ??= new Contract();
            contract.AttractionChannelId = 25;
            contract.AuthorId = 1;
            contract.BranchId = tsoBranchId;
            contract.Client = client;
            contract.ClientId = application.ClientId;
            contract.CollateralType = application.Position.CollateralType;
            contract.ContractClass = product.ContractClass;
            contract.ContractDate = DateTime.Today;
            contract.ContractNumber = contract.ContractNumber.IsNullOrEmpty(application.ContractNumber);
            contract.ContractTypeId = product.ContractTypeId;
            contract.CreditLineId = creditLineId;
            contract.EstimatedCost = application.Position.EstimatedCost.Value;
            contract.FirstPaymentDate = application.FirstPaymentDate;
            contract.LoanCost = loanCoast;
            contract.LoanPercent = product.LoanPercent;
            contract.LoanPercentCost = Math.Round(loanCoast * product.LoanPercent / 100, 4, MidpointRounding.AwayFromZero);
            contract.LoanPeriod = product.ContractClass != ContractClass.Tranche
                ? (int)product.PaymentPeriodType * application.Period
                : Math.Abs((DateTime.Now - application.MaturityDate).Days);
            contract.LoanPurposeId = 11;
            contract.MaturityDate = application.MaturityDate;
            contract.MaxCreditLineCost = application.Position.LoanCost ?? contract.MaxCreditLineCost;
            contract.MinimalInitialFee = 0;
            contract.OriginalMaturityDate = contract.MaturityDate;
            contract.OtherLoanPurpose = "online";
            contract.OwnerId = tsoBranchId;
            contract.PercentPaymentType = PercentPaymentType.Product;
            contract.PeriodTypeId = periodTypeId;
            contract.ProductTypeId = product.ProductTypeId;
            contract.RequiredInitialFee = 0;
            contract.Setting = product;
            contract.SettingId = product.Id;
            contract.UsePenaltyLimit = product.UsePenaltyLimit;
            contract.ContractData = new ContractData { Client = client };
            contract.ContractSpecific = new GoldContractSpecific { };

            if (product.ContractClass != ContractClass.CreditLine && (contract.ContractRates == null || !contract.ContractRates.Any()))
            {
                contract.ContractRates = product.LoanSettingRates
                    .Where(x => x.RateSetting.Code == Constants.ACCOUNT_SETTING_PENY_ACCOUNT || x.RateSetting.Code == Constants.ACCOUNT_SETTING_PENY_PROFIT)
                    .Select(x => new ContractRate
                    {
                        AuthorId = Constants.ADMINISTRATOR_IDENTITY,
                        CreateDate = DateTime.Now,
                        Date = DateTime.Now,
                        Rate = x.Rate,
                        RateSettingId = x.RateSettingId,
                    }).ToList();
            }

            if (product.ContractClass == ContractClass.Tranche)
                return contract;

            if (application.Position?.Car == null)
                return contract;

            if (contract.Positions.FirstOrDefault() is ContractPosition position)
            {
                position.EstimatedCost = application.Position.EstimatedCost.Value;
                position.LoanCost = application.Position.LoanCost.Value;
            }
            else
            {
                contract.Positions = new List<ContractPosition>
                {
                    new ContractPosition
                    {
                        CategoryId = 1,
                        EstimatedCost = application.Position.EstimatedCost.Value,
                        LoanCost = application.Position.LoanCost.Value,
                        PositionCount = 1,
                        PositionId = carId,
                    }
                };
            }

            return contract;
        }
    }
}
