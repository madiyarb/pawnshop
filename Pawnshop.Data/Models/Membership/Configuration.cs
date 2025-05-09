using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.CustomTypes;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Data.Models.Membership
{
    public class Configuration : IJsonObject
    {
        /// <summary>
        /// Юридический статус
        /// </summary>
        public LegalSettings LegalSettings { get; set; }
        /// <summary>
        /// Контактная информация
        /// </summary>
        public ContactSettings ContactSettings { get; set; }
        /// <summary>
        /// Банковские реквизиты
        /// </summary>
        public BankSettings BankSettings { get; set; }
        /// <summary>
        /// Профиль договора по умолчанию
        /// </summary>
        public ContractSettings ContractSettings { get; set; }

        /// <summary>
        /// Профиль ПКО/РКО по умолчанию
        /// </summary>
        public CashOrderSettings CashOrderSettings { get; set; }

        /// <summary>
        /// Подписанты
        /// </summary>
        public string Signatories { get; set; } = String.Empty;
    }

    public class CashOrderSettings
    {
        /// <summary>
        /// Код для номера ПКО
        /// </summary>
        public string CashInNumberCode { get; set; }

        /// <summary>
        /// Код для номера РКО
        /// </summary>
        public string CashOutNumberCode { get; set; }

        /// <summary>
        /// Счет кассы
        /// </summary>
        public int? CashAccountId { get; set; }

        /// <summary>
        /// Счет убытка
        /// </summary>
        public int? ProfitlessAccountId { get; set; }

        /// <summary>
        /// Безналичный счет
        /// </summary>
        public int? BankAccountId { get; set; }

        /// <summary>
        /// Настройки для золота
        /// </summary>
        public CollateralSettings GoldCollateralSettings { get; set; }

        /// <summary>
        /// Настройки для товаров
        /// </summary>
        public CollateralSettings GoodCollateralSettings { get; set; }

        /// <summary>
        /// Настройки для автомобилей
        /// </summary>
        public CollateralSettings CarCollateralSettings { get; set; }

        /// <summary>
        /// Настройки для спецтехники
        /// </summary>
        public CollateralSettings MachineryCollateralSettings { get; set; }

        /// <summary>
        /// Настройки для беззалоговых займов
        /// </summary>
        public CollateralSettings UnsecuredCollateralSettings { get; set; }

        /// <summary>
        /// Настройки для передачи автомобилей
        /// </summary>
        public TransferSettings CarTransferSettings { get; set; }

        /// <summary>
        /// Настройки для страхового договора
        /// </summary>
        public InsuranceSettings InsuranceSettings { get; set; }

        /// <summary>
        /// Настройки для принятия онлайн оплат
        /// </summary>
        public OnlinePaymentSettings OnlinePaymentSettings { get; set; }

        public CollateralSettings Get(CollateralType collateralType)
        {
            return collateralType switch
            {
                CollateralType.Gold => GoldCollateralSettings,
                CollateralType.Car => CarCollateralSettings,
                CollateralType.Goods => GoodCollateralSettings,
                CollateralType.Machinery => MachineryCollateralSettings,
                CollateralType.Unsecured => UnsecuredCollateralSettings,
                _ => throw new ArgumentOutOfRangeException(nameof(collateralType), collateralType, null)
            };
        }

        public TransferSettings GetTransfered(CollateralType collateralType)
        {
            return collateralType switch
            {
                CollateralType.Gold => throw new ArgumentOutOfRangeException(nameof(collateralType)),
                CollateralType.Car => CarTransferSettings,
                CollateralType.Goods => throw new ArgumentOutOfRangeException(nameof(collateralType)),
                CollateralType.Machinery => throw new ArgumentOutOfRangeException(nameof(collateralType)),
                CollateralType.Unsecured => throw new ArgumentOutOfRangeException(nameof(collateralType)),
                _ => throw new ArgumentOutOfRangeException(nameof(collateralType), collateralType, null)
            };
        }

        public AccountSettings GetAccountSettings(CollateralType collateralType, AmountType paymentType, bool isTransfered)
        {
            try
            {
                return paymentType switch
                {
                    AmountType.Debt => isTransfered
                        ? GetTransfered(collateralType).DebtSettings
                        : Get(collateralType).DebtSettings,
                    AmountType.Loan => isTransfered
                        ? GetTransfered(collateralType).LoanSettings
                        : Get(collateralType).LoanSettings,
                    AmountType.Penalty => isTransfered
                        ? GetTransfered(collateralType).PenaltySettings
                        : Get(collateralType).PenaltySettings,
                    AmountType.Duty => isTransfered
                        ? GetTransfered(collateralType).DutySettings
                        : Get(collateralType).DutySettings,
                    _ => throw new ArgumentOutOfRangeException(nameof(paymentType))
                };
            }
            catch (NullReferenceException)
            {
                throw new PawnshopApplicationException($"Не найдены настройки для вашего филиала");
            }
        }

        public AccountSettings GetPrepaymentSettings(CollateralType collateralType, bool isTransfered)
        {
            try
            {
                return isTransfered
                    ? GetTransfered(collateralType).PrepaymentSettings
                    : Get(collateralType).PrepaymentSettings;
            }
            catch (NullReferenceException)
            {
                throw new PawnshopApplicationException($"Не найдены авансовые настройки для вашего филиала");
            }
        }
    }

    public class CollateralSettings
    {
        /// <summary>
        /// Настройки для выдачи
        /// </summary>
        public AccountSettings SupplySettings { get; set; }

        /// <summary>
        /// Настройки для долга
        /// </summary>
        public AccountSettings DebtSettings { get; set; }

        /// <summary>
        /// Настройки для пошлина
        /// </summary>
        public AccountSettings LoanSettings { get; set; }

        /// <summary>
        /// Настройки для штрафа
        /// </summary>
        public AccountSettings PenaltySettings { get; set; }

        /// <summary>
        /// Настройки для отправки реализации
        /// </summary>
        public AccountSettings SellingSettings { get; set; }

        /// <summary>
        /// Настройки для реализации
        /// </summary>
        public AccountSettings DisposeSettings { get; set; }

        /// <summary>
        /// Настройки для авансовых платежей
        /// </summary>
        public AccountSettings PrepaymentSettings { get; set; }

        /// <summary>
        /// Настройки для авансовых платежей от сотрудников
        /// </summary>
        public AccountSettings EmployeePrepaymentSettings { get; set; }

        /// <summary>
        /// Настройки для Госпошлины
        /// </summary>
        public AccountSettings DutySettings { get; set; }
    }

    public class TransferSettings
    {
        /// <summary>
        /// Настройки для долга при передаче
        /// </summary>
        public AccountSettings SupplyDebtSettings { get; set; }

        /// <summary>
        /// Настройки для пошлина при передаче
        /// </summary>
        public AccountSettings SupplyLoanSettings { get; set; }

        /// <summary>
        /// Настройки для штрафа при передаче
        /// </summary>
        public AccountSettings SupplyPenaltySettings { get; set; }        

        /// <summary>
        /// Настройки для долга
        /// </summary>
        public AccountSettings DebtSettings { get; set; }

        /// <summary>
        /// Настройки для пошлина
        /// </summary>
        public AccountSettings LoanSettings { get; set; }

        /// <summary>
        /// Настройки для штрафа
        /// </summary>
        public AccountSettings PenaltySettings { get; set; }
        
        /// <summary>
        /// Настройки для авансовых платежей
        /// </summary>
        public AccountSettings PrepaymentSettings { get; set; }

        /// <summary>
        /// Настройки для авансовых платежей от сотрудников
        /// </summary>
        public AccountSettings EmployeePrepaymentSettings { get; set; }

        /// <summary>
        /// Настройки для Госпошлины
        /// </summary>
        public AccountSettings DutySettings { get; set; }
    }

    public class InsuranceSettings
    {
        /// <summary>
        /// Настройки при подписании страхового договора
        /// </summary>
        public AccountSettings SignSettings { get; set; }
    }

    public class OnlinePaymentSettings
    {
        /// <summary>
        /// Настройки для принятия оплаты от Касса24
        /// </summary>
        public AccountSettings Kassa24Settings { get; set; }

        /// <summary>
        /// Настройки для принятия оплаты от RPS Asia
        /// </summary>
        public AccountSettings RpsSettings { get; set; }

        /// <summary>
        /// Настройки для принятия оплаты от Processing.kz
        /// </summary>
        public AccountSettings ProcessingSettings { get; set; }

        /// <summary>
        /// Настройки для принятия оплаты от Qiwi.com
        /// </summary>
        public AccountSettings QiwiSettings { get; set; }
    }

    public class LegalSettings
    {
        /// <summary>
        /// Наименование организации
        /// </summary>
        public string LegalName { get; set; }


        /// <summary>
        /// Наименование организации
        /// </summary>
        public string LegalNameKaz { get; set; }

        /// <summary>
        /// ОКУД
        /// </summary>
        public string OKUD { get; set; }
        /// <summary>
        /// ОКПО
        /// </summary>
        public string OKPO { get; set; }
        /// <summary>
        /// РНН
        /// </summary>
        public string RNN { get; set; }
        /// <summary>
        /// БИН
        /// </summary>
        public string BIN { get; set; }
        /// <summary>
        /// Директор
        /// </summary>
        public string ChiefName { get; set; }
        /// <summary>
        /// Главный бухгалтер
        /// </summary>
        public string ChiefAccountantName { get; set; }
        /// <summary>
        /// Бухгалтер
        /// </summary>
        public string AccountantName { get; set; }
        /// <summary>
        /// Кассир
        /// </summary>
        public string CashierName { get; set; }
    }

    public class ContactSettings
    {
        /// <summary>
        /// Город
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Город на казахском языке
        /// </summary>
        public string? CityAlt { get; set; }

        /// <summary>
        /// Адрес
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Адрес на казахском
        /// </summary>
        public string? AddressAlt { get; set; }
        /// <summary>
        /// Телефон
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// Режим работы
        /// </summary>
        public string Schedule { get; set; }
        /// <summary>
        /// Ссылка на битрикс24
        /// </summary>
        public string BitrixUrl { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Сайт
        /// </summary>
        public string WebsiteUrl { get; set; }
    }

    public class ContractSettings
    {
        /// <summary>
        /// Код для номера договора
        /// </summary>
        public string NumberCode { get; set; }

    }

    public class AccountSettings
    {
        /// <summary>
        /// Ссылка на счет Дебет
        /// </summary>
        public int? DebitId { get; set; }

        /// <summary>
        /// Ссылка на счет Кредит
        /// </summary>
        public int? CreditId { get; set; }
    }

    public class BankSettings
    {
        /// <summary>
        /// Наименование
        /// </summary>
        public string BankName { get; set; }
        /// <summary>
        /// Наименование на казахском
        /// </summary>
        public string BankNameKaz { get; set; }
        /// <summary>
        /// Счет
        /// </summary>
        public string BankAccount { get; set; }
        /// <summary>
        /// Кбе
        /// </summary>
        public string BankKbe { get; set; }
        /// <summary>
        /// БИК
        /// </summary>
        public string BankBik { get; set; }
    }

    public class IntegrationSettings
    {
        /// <summary>
        /// Ссылка
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Логин
        /// </summary>
        public string Login { get; set; }
        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; }
    }
}