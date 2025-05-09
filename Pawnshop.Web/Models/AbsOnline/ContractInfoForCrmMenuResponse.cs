using Pawnshop.Data.Models.Contracts.Revisions;
using System;

namespace Pawnshop.Web.Models.AbsOnline
{
    public class ContractInfoForCrmMenuResponse
    {
        // TODO: abs-migration при возможности изменить интеграцию с CRM и избавиться от этой дебильной структуры

        /// <summary>
        /// Параметр шины DataProverkiZadolzhenosty
        /// </summary>
        public DateTime CallDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Параметр шины ZaimNomer / NomerDogovora
        /// </summary>
        public string ContractNumber { get; set; } = string.Empty;

        /// <summary>
        /// ZaimData Параметр шины / DataDogovora
        /// </summary>
        public DateTime ContractDate { get; set; }

        /// <summary>
        /// Параметр шины SummOsnovnoyDolg
        /// </summary>
        public decimal PrincipalDebt { get; set; }

        /// <summary>
        /// Параметр шины SummOsnovnoyDolgPoGrafiku
        /// </summary>
        public decimal PrincipalDebtSchedule { get; set; }

        /// <summary>
        /// Параметр шины SummProcenti
        /// </summary>
        public decimal Profit { get; set; }

        /// <summary>
        /// Параметр шины SummKomissii
        /// </summary>
        public decimal Commission { get; set; }

        /// <summary>
        /// Параметр шины SummChlenskieVznosi
        /// </summary>
        public decimal MembershipFees { get; set; }

        /// <summary>
        /// Параметр шины SummShtraf
        /// </summary>
        public decimal Fine { get; set; }

        /// <summary>
        /// Параметр шины SummPeni
        /// </summary>
        public decimal Penalty { get; set; }

        /// <summary>
        /// Параметр шины SummGosposhlina
        /// </summary>
        public decimal StateDuty { get; set; }

        /// <summary>
        /// Параметр шины DataPervoyProSrochky
        /// </summary>
        public DateTime FirstPaymentExpiredDate { get; set; }

        /// <summary>
        /// Параметр шины DneyProsrochki
        /// </summary>
        public int PaymentExpiredDays { get; set; }

        /// <summary>
        /// Параметр шины KolichestvoProsrochenihPlatezhey
        /// </summary>
        public int PaymentExpiredCount { get; set; }

        /// <summary>
        /// Параметр шины SummProsrochkiOsnovnoyDolg
        /// </summary>
        public decimal OverduePrincipalDebt { get; set; }

        /// <summary>
        /// Параметр шины SummProsrochkiProcenti
        /// </summary>
        public decimal OverdueProfit { get; set; }

        /// <summary>
        /// Параметр шины SummProsrochkiKomissii
        /// </summary>
        public decimal OverdueCommission { get; set; }

        /// <summary>
        /// Параметр шины SummProsrochkiChlenskieVznosi
        /// </summary>
        public decimal OverdueMembershipFees { get; set; }

        /// <summary>
        /// Параметр шины SummProsrochkiPeni
        /// </summary>
        public decimal OverduePenalty { get; set; }

        /// <summary>
        /// Параметр шины SummProsrochkiShtraf
        /// </summary>
        public decimal OverdueFine { get; set; }

        /// <summary>
        /// Параметр шины SummProsrochkiProchieDohodi
        /// </summary>
        public decimal OverdueOtherIncome { get; set; }

        /// <summary>
        /// Параметр шины SummSrochnayaOsnovnoyDolg
        /// </summary>
        public decimal UrgentPrincipalDebt { get; set; }

        /// <summary>
        /// Параметр шины SummSrochnayaProcenti
        /// </summary>
        public decimal UrgentProfit { get; set; }

        /// <summary>
        /// Параметр шины SummSrochnayaKomissii
        /// </summary>
        public decimal UrgenCommission { get; set; }

        /// <summary>
        /// Параметр шины SummSrochnayaChlenskieVznosi
        /// </summary>
        public decimal UrgenMembershipFees { get; set; }

        /// <summary>
        /// Параметр шины SummSrochnayaPeni
        /// </summary>
        public decimal UrgenPenalty { get; set; }

        /// <summary>
        /// Параметр шины SummSrochnayaShtrafi
        /// </summary>
        public decimal UrgenFine { get; set; }

        /// <summary>
        /// Параметр шины SummSrochnayaProchieDohodi
        /// </summary>
        public decimal UrgenOtherIncome { get; set; }

        /// <summary>
        /// Параметр шины DataBlizhayshegoPlatezha
        /// </summary>
        public DateTime NextPaymentDate { get; set; }

        /// <summary>
        /// Параметр шины DneyDoPlatezha
        /// </summary>
        public int NextPaymentDays { get; set; }

        /// <summary>
        /// Параметр шины BlizhayshiyPlatezh_OsnovnoyDolg
        /// </summary>
        public decimal NextPaymentPrincipalDebt { get; set; }

        /// <summary>
        /// Параметр шины BlizhayshiyPlatezh_Procenti
        /// </summary>
        public decimal NextPaymentProfit { get; set; }

        /// <summary>
        /// Параметр шины BlizhayshiyPlatezh_Komissii
        /// </summary>
        public decimal NextPaymentCommission { get; set; }

        /// <summary>
        /// Параметр шины BlizhayshiyPlatezh_Vznosi
        /// </summary>
        public decimal NextPaymentMembershipFees { get; set; }

        /// <summary>
        /// Параметр шины DataActualnogoRascheta
        /// </summary>
        public DateTime PaymentScheduleDate { get; set; }

        /// <summary>
        /// Параметр шины Period
        /// </summary>
        public DateTime Period { get; set; }

        /// <summary>
        /// Параметр шины TekuschiyDogovorPredstavlenie
        /// </summary>
        public string ContractViewName { get; set; } = string.Empty;

        /// <summary>
        /// Параметр шины DataNachalaDogovora
        /// </summary>
        public DateTime SignDate { get; set; }

        /// <summary>
        /// Параметр шины DataOkonchaniyaDogovora
        /// </summary>
        public DateTime MaturityDate { get; set; }

        /// <summary>
        /// Параметр шины ActualniiZaimNomerDogovora
        /// </summary>
        public string ActualContractNumber { get; set; } = string.Empty;

        /// <summary>
        /// Параметр шины ActualniiZaimDataDogovora
        /// </summary>
        public DateTime ActualContractDate { get; set; }

        /// <summary>
        /// Параметр шины ActualniiZaimDeystvuet ???
        /// </summary>
        public DateTime ActualContractRunDate { get; set; }

        /// <summary>
        /// Параметр шины ProcentnayaStavkaZnachenie
        /// </summary>
        public decimal YearPercent { get; set; }

        /// <summary>
        /// Параметр шины PosledniyPlatezh_Data
        /// </summary>
        public DateTime LastPaymentDate { get; set; }

        /// <summary>
        /// Параметр шины PosledniyPlatezh_OsnovnoyDolg
        /// </summary>
        public decimal LastPaymentPrincipalDebt { get; set; }

        /// <summary>
        /// Параметр шины PosledniyPlatezh_Procent
        /// </summary>
        public decimal LastPaymentProfit { get; set; }

        /// <summary>
        /// Параметр шины PosledniyPlatezh_Komissii
        /// </summary>
        public decimal LastPaymentCommission { get; set; }

        /// <summary>
        /// Параметр шины PosledniyPlatezh_ChlenskieVznosi
        /// </summary>
        public decimal LastPaymentMembershipFees { get; set; }

        /// <summary>
        /// Параметр шины PosledniyPlatezh_Peni
        /// </summary>
        public decimal LasyPaymentPenalty { get; set; }

        /// <summary>
        /// Параметр шины PosledniyPlatezh_Shtrafi
        /// </summary>
        public decimal LasyPaymentFine { get; set; }

        /// <summary>
        /// Параметр шины VsegoViplacheno_OsnovnoyDolg
        /// </summary>
        public decimal PaidPrincipalDebt { get; set; }

        /// <summary>
        /// Параметр шины VsegoViplacheno_Procent
        /// </summary>
        public decimal PaidProfit { get; set; }

        /// <summary>
        /// Параметр шины VsegoViplacheno_Komissii
        /// </summary>
        public decimal PaidCommission { get; set; }

        /// <summary>
        /// Параметр шины VsegoViplacheno_ChlenskieVznosi
        /// </summary>
        public decimal PaidMembershipFees { get; set; }

        /// <summary>
        /// Параметр шины VsegoViplacheno_Peni
        /// </summary>
        public decimal PaidPenalty { get; set; }

        /// <summary>
        /// Параметр шины VsegoViplacheno_Shtrafi
        /// </summary>
        public decimal PaidFine { get; set; }

        /// <summary>
        /// Параметр шины FinansoviyProduct
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Параметр шины OstatokPoDogovoruZaima
        /// </summary>
        public decimal RemnantContract { get; set; }

        /// <summary>
        /// Параметр шины OstatokPoObschemuDogovoru
        /// </summary>
        public decimal RemnantCommonContract { get; set; }

        /// <summary>
        /// Параметр шины DataZakritiaZaima
        /// </summary>
        public DateTime ContractCloseDate { get; set; }

        /// <summary>
        /// Параметр шины Zacrit
        /// </summary>
        public bool IsClosed { get; set; }

        /// <summary>
        /// Параметр шины KokichestvoOstavshihsyaPlatezhey
        /// </summary>
        public int RemainingPaymentsCount { get; set; }

        /// <summary>
        /// Параметр шины DataStatusaProblemnogoZaima
        /// </summary>
        public DateTime ProblemStatusDate { get; set; }

        /// <summary>
        /// Параметр шины ProblemniyZaimNomer
        /// </summary>
        public string ProblemContractNumber { get; set; } = string.Empty;

        /// <summary>
        /// Параметр шины ProblemniyZaimData
        /// </summary>
        public DateTime ProblemContractDate { get; set; }

        /// <summary>
        /// Параметр шины ProblemniyZaimSostoysniyaText
        /// </summary>
        public string ProblemContractStatusMessage { get; set; } = string.Empty;

        /// <summary>
        /// Параметр шины NomerResheniaSuda
        /// </summary>
        public string CourtDecisionNumber { get; set; } = string.Empty;
    }
}
