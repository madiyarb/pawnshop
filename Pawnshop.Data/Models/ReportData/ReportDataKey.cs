using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.ReportData
{
    public enum ReportDataKey : short
    {
        [Display(Name = "Сумма портфеля (золото)")]
        PortfolioCostGold = 1,
        [Display(Name = "Сумма портфеля (авто)")]
        PortfolioCostAuto = 2,
        [Display(Name = "Сумма портфеля (товары)")]
        PortfolioCostGoods = 3,
        [Display(Name = "Сумма портфеля (беззалоговые)")]
        PortfolioCostUnsecured = 5,
        [Display(Name = "Количество договоров в портфеле (золото)")]
        PortfolioContractsCountGold = 6,
        [Display(Name = "Количество договоров в портфеле (авто)")]
        PortfolioContractsCountAuto = 7,
        [Display(Name = "Количество договоров в портфеле (товары)")]
        PortfolioContractsCountGoods = 8,
        [Display(Name = "Количество договоров в портфеле (беззалоговые)")]
        PortfolioContractsCountUnsecured = 10,
        [Display(Name = "Касса на конец даты")]
        CashboxCost = 11,
        [Display(Name = "Доход проценты (золото)")]
        PercentProfitGold = 12,
        [Display(Name = "Доход проценты (авто)")]
        PercentProfitAuto = 13,
        [Display(Name = "Доход проценты (товары)")]
        PercentProfitGoods = 14,
        [Display(Name = "Доход проценты (беззалоговые)")]
        PercentProfitUnsecured = 16,
        [Display(Name = "Доход штрафы (золото)")]
        PenaltyProfitGold = 17,
        [Display(Name = "Доход штрафы (авто)")]
        PenaltyProfitAuto = 18,
        [Display(Name = "Доход штрафы (товары)")]
        PenaltyProfitGoods = 19,
        [Display(Name = "Доход штрафы (беззалоговые)")]
        PenaltyProfitUnsecured = 21,
        [Display(Name = "Сумма просрочки (золото)")]
        DelayedCostGold = 22,
        [Display(Name = "Сумма просрочки (авто)")]
        DelayedCostAuto = 23,
        [Display(Name = "Сумма просрочки (товары)")]
        DelayedCostGoods = 24,
        [Display(Name = "Сумма просрочки (беззалоговые)")]
        DelayedCostUnsecured = 26,
        [Display(Name = "Количество договоров просрочки (золото)")]
        DelayedCountGold = 27,
        [Display(Name = "Количество договоров просрочки (авто)")]
        DelayedCountAuto = 28,
        [Display(Name = "Количество договоров просрочки (товары)")]
        DelayedCountGoods = 29,
        [Display(Name = "Количество договоров просрочки (беззалоговые)")]
        DelayedCountUnsecured = 31,
        [Display(Name = "Количество новых договоров (мигрированный)")]
        IssuanceCountMigrated = 32,
        [Display(Name = "Количество новых договоров (ЧДП)")]
        IssuanceCountPartialPayment = 33,
        [Display(Name = "Количество новых договоров (добор)")]
        IssuanceCountAddition = 34,
        [Display(Name = "Количество новых договоров (повторная выдача)")]
        IssuanceCountOld = 35,
        [Display(Name = "Количество новых договоров (новая выдача)")]
        IssuanceCountNew = 36,
        [Display(Name = "Сумма ОД новых договоров (мигрированный)")]
        IssuanceDebtCostMigrated = 37,
        [Display(Name = "Сумма ОД новых договоров (ЧДП)")]
        IssuanceDebtCostPartialPayment = 38,
        [Display(Name = "Сумма ОД новых договоров (добор)")]
        IssuanceDebtCostAddition = 39,
        [Display(Name = "Сумма ОД новых договоров (повторная выдача)")]
        IssuanceDebtCostOld = 40,
        [Display(Name = "Сумма ОД новых договоров (новая выдача)")]
        IssuanceDebtCostNew = 41,
        [Display(Name = "Сумма погашения новых договоров (ЧДП)")]
        IssuancePartialPaymentCost = 42,
        [Display(Name = "Сумма добора новых договоров (добор)")]
        IssuanceAdditionCost = 43,
        [Display(Name = "Количество выкупленных договоров")]
        BuyoutCount = 44,
        [Display(Name = "Сумма ОД выкупленных договоров")]
        BuyoutCost = 45,
        [Display(Name = "Сумма комиссии за страховку")]
        InsurancePremium = 50,
        //СФК
        [Display(Name = "Сумма портфеля (золото) в СФК")]
        SFKPortfolioCostGold = 201,
        [Display(Name = "Сумма портфеля (авто) в СФК")]
        SFKPortfolioCostAuto = 202,
        [Display(Name = "Сумма портфеля (товары) в СФК")]
        SFKPortfolioCostGoods = 203,
        [Display(Name = "Сумма портфеля (беззалоговые) в СФК")]
        SFKPortfolioCostUnsecured = 205,
        [Display(Name = "Количество договоров в портфеле (золото) в СФК")]
        SFKPortfolioContractsCountGold = 206,
        [Display(Name = "Количество договоров в портфеле (авто) в СФК")]
        SFKPortfolioContractsCountAuto = 207,
        [Display(Name = "Количество договоров в портфеле (товары) в СФК")]
        SFKPortfolioContractsCountGoods = 208,
        [Display(Name = "Количество договоров в портфеле (беззалоговые) в СФК")]
        SFKPortfolioContractsCountUnsecured = 210,
        [Display(Name = "Доход проценты (золото) в СФК")]
        SFKPercentProfitGold = 212,
        [Display(Name = "Доход проценты (авто) в СФК")]
        SFKPercentProfitAuto = 213,
        [Display(Name = "Доход проценты (товары) в СФК")]
        SFKPercentProfitGoods = 214,
        [Display(Name = "Доход проценты (беззалоговые) в СФК")]
        SFKPercentProfitUnsecured = 216,
        [Display(Name = "Доход штрафы (золото) в СФК")]
        SFKPenaltyProfitGold = 217,
        [Display(Name = "Доход штрафы (авто) в СФК")]
        SFKPenaltyProfitAuto = 218,
        [Display(Name = "Доход штрафы (товары) в СФК")]
        SFKPenaltyProfitGoods = 219,
        [Display(Name = "Доход штрафы (беззалоговые) в СФК")]
        SFKPenaltyProfitUnsecured = 221,
        [Display(Name = "Сумма просрочки (золото) в СФК")]
        SFKDelayedCostGold = 222,
        [Display(Name = "Сумма просрочки (авто) в СФК")]
        SFKDelayedCostAuto = 223,
        [Display(Name = "Сумма просрочки (товары) в СФК")]
        SFKDelayedCostGoods = 224,
        [Display(Name = "Сумма просрочки (беззалоговые) в СФК")]
        SFKDelayedCostUnsecured = 226,
        [Display(Name = "Количество договоров просрочки (золото) в СФК")]
        SFKDelayedCountGold = 227,
        [Display(Name = "Количество договоров просрочки (авто) в СФК")]
        SFKDelayedCountAuto = 228,
        [Display(Name = "Количество договоров просрочки (товары) в СФК")]
        SFKDelayedCountGoods = 229,
        [Display(Name = "Количество договоров просрочки (беззалоговые) в СФК")]
        SFKDelayedCountUnsecured = 231,
        [Display(Name = "Количество выкупленных договоров в СФК")]
        SFKBuyoutCount = 244,
        [Display(Name = "Сумма ОД выкупленных договоров в СФК")]
        SFKBuyoutCost = 245,

        [Display(Name = "TFG. Остаток ОД. Просрочка в поиске (1-5). Авто")]
        DebtBalance_1_5_Auto = 500,
        [Display(Name = "TFG. Количество договоров. Просрочка в поиске (1-5). Авто")]
        ContractsCount_1_5_Auto = 501,

        [Display(Name = "TFG. Остаток ОД. Просрочка в поиске (6-30). Авто")]
        DebtBalance_6_30_Auto = 502,
        [Display(Name = "TFG. Количество договоров. Просрочка в поиске (6-30). Авто")]
        ContractsCount_6_30_Auto = 503,

        [Display(Name = "TFG. Остаток ОД. Просрочка в поиске (31-60). Авто")]
        DebtBalance_31_60_Auto = 504,
        [Display(Name = "TFG. Количество договоров. Просрочка в поиске (31-60). Авто")]
        ContractsCount_31_60_Auto = 505,

        [Display(Name = "TFG. Остаток ОД. Просрочка в поиске (61-90). Авто")]
        DebtBalance_61_90_Auto = 506,
        [Display(Name = "TFG. Количество договоров. Просрочка в поиске (61-90). Авто")]
        ContractsCount_61_90_Auto = 507,

        [Display(Name = "TFG. Остаток ОД. Просрочка в поиске (91-120). Авто")]
        DebtBalance_91_120_Auto = 508,
        [Display(Name = "TFG. Количество договоров. Просрочка в поиске (91-120). Авто")]
        ContractsCount_91_120_Auto = 509,

        [Display(Name = "TFG. Остаток ОД. Просрочка в поиске (121-150). Авто")]
        DebtBalance_121_150_Auto = 510,
        [Display(Name = "TFG. Количество договоров. Просрочка в поиске (121-150). Авто")]
        ContractsCount_121_150_Auto = 511,

        [Display(Name = "TFG. Остаток ОД. Просрочка в поиске (151-180). Авто")]
        DebtBalance_151_180_Auto = 512,
        [Display(Name = "TFG. Количество договоров. Просрочка в поиске (151-180). Авто")]
        ContractsCount_151_180_Auto = 513,

        [Display(Name = "TFG. Остаток ОД. Просрочка в поиске (181-360). Авто")]
        DebtBalance_181_360_Auto = 514,
        [Display(Name = "TFG. Количество договоров. Просрочка в поиске (181-360). Авто")]
        ContractsCount_181_360_Auto = 515,

        [Display(Name = "TFG. Остаток ОД. Просрочка в поиске (361+). Авто")]
        DebtBalance_361_Plus_Auto = 516,
        [Display(Name = "TFG. Количество договоров. Просрочка в поиске (361+). Авто")]
        ContractsCount_361_Plus_Auto = 517,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (ждем) (1-5). Авто")]
        DebtBalance_1_5_ParkingWaitingAuto = 518,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (ждем) (1-5). Авто")]
        ContractsCount_1_5_ParkingWaitingAuto = 519,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (ждем) (6-30). Авто")]
        DebtBalance_6_30_ParkingWaitingAuto = 520,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (ждем) (6-30). Авто")]
        ContractsCount_6_30_ParkingWaitingAuto = 521,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (ждем) (31-60). Авто")]
        DebtBalance_31_60_ParkingWaitingAuto = 522,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (ждем) (31-60). Авто")]
        ContractsCount_31_60_ParkingWaitingAuto = 523,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (ждем) (61-90). Авто")]
        DebtBalance_61_90_ParkingWaitingAuto = 524,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (ждем) (61-90). Авто")]
        ContractsCount_61_90_ParkingWaitingAuto = 525,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (ждем) (91-120). Авто")]
        DebtBalance_91_120_ParkingWaitingAuto = 526,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (ждем) (91-120). Авто")]
        ContractsCount_91_120_ParkingWaitingAuto = 527,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (ждем) (121-150). Авто")]
        DebtBalance_121_150_ParkingWaitingAuto = 528,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (ждем) (121-150). Авто")]
        ContractsCount_121_150_ParkingWaitingAuto = 529,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (ждем) (151-180). Авто")]
        DebtBalance_151_180_ParkingWaitingAuto = 530,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (ждем) (151-180). Авто")]
        ContractsCount_151_180_ParkingWaitingAuto = 531,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (ждем) (181-360). Авто")]
        DebtBalance_181_360_ParkingWaitingAuto = 532,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (ждем) (181-360). Авто")]
        ContractsCount_181_360_ParkingWaitingAuto = 533,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (ждем) (361+). Авто")]
        DebtBalance_361_Plus_ParkingWaitingAuto = 534,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (ждем) (361+). Авто")]
        ContractsCount_361_Plus_ParkingWaitingAuto = 535,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (у юристов) (1-5). Авто")]
        DebtBalance_1_5_ParkingLayersAuto = 536,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (у юристов) (1-5). Авто")]
        ContractsCount_1_5_ParkingLayersAuto = 537,
        
        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (у юристов) (6-30). Авто")]
        DebtBalance_6_30_ParkingLayersAuto = 538,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (у юристов) (6-30). Авто")]
        ContractsCount_6_30_ParkingLayersAuto = 539,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (у юристов) (31-60). Авто")]
        DebtBalance_31_60_ParkingLayersAuto = 540,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (у юристов) (31-60). Авто")]
        ContractsCount_31_60_ParkingLayersAuto = 541,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (у юристов) (61-90). Авто")]
        DebtBalance_61_90_ParkingLayersAuto = 542,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (у юристов) (61-90). Авто")]
        ContractsCount_61_90_ParkingLayersAuto = 543,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (у юристов) (91-120). Авто")]
        DebtBalance_91_120_ParkingLayersAuto = 544,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (у юристов) (91-120). Авто")]
        ContractsCount_91_120_ParkingLayersAuto = 545,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (у юристов) (121-150). Авто")]
        DebtBalance_121_150_ParkingLayersAuto = 546,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (у юристов) (121-150). Авто")]
        ContractsCount_121_150_ParkingLayersAuto = 547,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (у юристов) (151-180). Авто")]
        DebtBalance_151_180_ParkingLayersAuto = 548,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (у юристов) (151-180). Авто")]
        ContractsCount_151_180_ParkingLayersAuto = 549,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (у юристов) (181-360). Авто")]
        DebtBalance_181_360_ParkingLayersAuto = 550,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (у юристов) (181-360). Авто")]
        ContractsCount_181_360_ParkingLayersAuto = 551,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (у юристов) (361+). Авто")]
        DebtBalance_361_Plus_ParkingLayersAuto = 552,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (у юристов) (361+). Авто")]
        ContractsCount_361_Plus_ParkingLayersAuto = 553,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (на продажу) (1-5). Авто")]
        DebtBalance_1_5_ParkingSellingAuto = 554,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (на продажу) (1-5). Авто")]
        ContractsCount_1_5_ParkingSellingAuto = 555,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (на продажу) (6-30). Авто")]
        DebtBalance_6_30_ParkingSellingAuto = 556,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (на продажу) (6-30). Авто")]
        ContractsCount_6_30_ParkingSellingAuto = 557,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (на продажу) (31-60). Авто")]
        DebtBalance_31_60_ParkingSellingAuto = 558,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (на продажу) (31-60). Авто")]
        ContractsCount_31_60_ParkingSellingAuto = 559,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (на продажу) (61-90). Авто")]
        DebtBalance_61_90_ParkingSellingAuto = 560,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (на продажу) (61-90). Авто")]
        ContractsCount_61_90_ParkingSellingAuto = 561,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (на продажу) (91-120). Авто")]
        DebtBalance_91_120_ParkingSellingAuto = 562,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (на продажу) (91-120). Авто")]
        ContractsCount_91_120_ParkingSellingAuto = 563,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (на продажу) (121-150). Авто")]
        DebtBalance_121_150_ParkingSellingAuto = 564,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (на продажу) (121-150). Авто")]
        ContractsCount_121_150_ParkingSellingAuto = 565,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (на продажу) (151-180). Авто")]
        DebtBalance_151_180_ParkingSellingAuto = 566,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (на продажу) (151-180). Авто")]
        ContractsCount_151_180_ParkingSellingAuto = 567,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (на продажу) (181-360). Авто")]
        DebtBalance_181_360_ParkingSellingAuto = 568,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (на продажу) (181-360). Авто")]
        ContractsCount_181_360_ParkingSellingAuto = 569,

        [Display(Name = "TFG. Остаток ОД. Просрочка на стоянке (на продажу) (361+). Авто")]
        DebtBalance_361_Plus_ParkingSellingAuto = 570,
        [Display(Name = "TFG. Количество договоров. Просрочка на стоянке (на продажу) (361+). Авто")]
        ContractsCount_361_Plus_ParkingSellingAuto = 571,

        [Display(Name = "TFG. Остаток ОД. Просрочка передано в CarTAS (1-5). Авто")]
        DebtBalance_1_5_CarTasAuto = 572,
        [Display(Name = "TFG. Количество договоров. Просрочка передано в CarTAS (1-5). Авто")]
        ContractsCount_1_5_CarTasAuto = 573,

        [Display(Name = "TFG. Остаток ОД. Просрочка передано в CarTAS (6-30). Авто")]
        DebtBalance_6_30_CarTasAuto = 574,
        [Display(Name = "TFG. Количество договоров. Просрочка передано в CarTAS (6-30). Авто")]
        ContractsCount_6_30_CarTasAuto = 575,

        [Display(Name = "TFG. Остаток ОД. Просрочка передано в CarTAS (31-60). Авто")]
        DebtBalance_31_60_CarTasAuto = 576,
        [Display(Name = "TFG. Количество договоров. Просрочка передано в CarTAS (31-60). Авто")]
        ContractsCount_31_60_CarTasAuto = 577,

        [Display(Name = "TFG. Остаток ОД. Просрочка передано в CarTAS (61-90). Авто")]
        DebtBalance_61_90_CarTasAuto = 578,
        [Display(Name = "TFG. Количество договоров. Просрочка передано в CarTAS (61-90). Авто")]
        ContractsCount_61_90_CarTasAuto = 579,

        [Display(Name = "TFG. Остаток ОД. Просрочка передано в CarTAS (91-120). Авто")]
        DebtBalance_91_120_CarTasAuto = 580,
        [Display(Name = "TFG. Количество договоров. Просрочка передано в CarTAS (91-120). Авто")]
        ContractsCount_91_120_CarTasAuto = 581,

        [Display(Name = "TFG. Остаток ОД. Просрочка передано в CarTAS (121-150). Авто")]
        DebtBalance_121_150_CarTasAuto = 582,
        [Display(Name = "TFG. Количество договоров. Просрочка передано в CarTAS (121-150). Авто")]
        ContractsCount_121_150_CarTasAuto = 583,

        [Display(Name = "TFG. Остаток ОД. Просрочка передано в CarTAS (151-180). Авто")]
        DebtBalance_151_180_CarTasAuto = 584,
        [Display(Name = "TFG. Количество договоров. Просрочка передано в CarTAS (151-180). Авто")]
        ContractsCount_151_180_CarTasAuto = 585,
        [Display(Name = "TFG. Остаток ОД. Просрочка передано в CarTAS (181-360). Авто")]
        DebtBalance_181_360_CarTasAuto = 586,
        [Display(Name = "TFG. Количество договоров. Просрочка передано в CarTAS (181-360). Авто")]
        ContractsCount_181_260_CarTasAuto = 587,
        [Display(Name = "TFG. Остаток ОД. Просрочка передано в CarTAS (361+). Авто")]
        DebtBalance_361_Plus_CarTasAuto = 588,
        [Display(Name = "TFG. Количество договоров. Просрочка передано в CarTAS (361+). Авто")]
        ContractsCount_361_Plus_CarTasAuto = 589,
    }
}