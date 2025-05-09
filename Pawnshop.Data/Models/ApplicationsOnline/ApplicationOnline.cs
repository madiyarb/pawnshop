using System;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;

namespace Pawnshop.Data.Models.ApplicationsOnline
{

    [Table("ApplicationsOnline")]
    public sealed class ApplicationOnline
    {
        [ExplicitKey]
        public Guid Id { get; set; }

        /// <summary>
        /// Кем создан
        /// </summary>
        public int CreationAuthorId { get; set; }

        /// <summary>
        /// Кем изменен
        /// </summary>
        public int LastChangeAuthorId { get; set; }

        /// <summary>
        /// Ответственный менеджер
        /// </summary>
        public int? ResponsibleManagerId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата обновления
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Человекочитаемый номер 
        /// </summary>
        public string ApplicationNumber { get; set; }

        /// <summary>
        /// Сумма заявки
        /// </summary>
        public decimal ApplicationAmount { get; set; }
        /// <summary>
        /// Срок займа в месяцах
        /// </summary>
        public int LoanTerm { get; set; }

        /// <summary>
        /// Стадия
        /// </summary>
        public int Stage { get; set; }

        /// <summary>
        /// Продукт
        /// </summary>
        public int ProductId { get; set; }

        ///// <summary>
        ///// Аккаунт
        ///// </summary>
        //public string MobileAccountUserName { get; set; }

        /// <summary>
        /// Источник заявки
        /// </summary>
        public string ApplicationSource { get; set; }

        /// <summary>
        /// Цель кредита
        /// </summary>
        public int? LoanPurposeId { get; set; }

        /// <summary>
        /// Сфера деятельности бизнеса
        /// </summary>
        public int? BusinessLoanPurposeId { get; set; }

        /// <summary>
        /// Идентификатор для физических лиц (ОКЭД)
        /// если Цель кредита - бизнеса
        /// </summary>
        public int? OkedForIndividualsPurposeId { get; set; }

        /// <summary>
        /// Идентификатор целевой цели кредита
        /// если Цель кредита - бизнеса
        /// </summary>
        public int? TargetPurposeId { get; set; }

        /// <summary>
        /// Канал привлечения
        /// </summary>
        public int? AttractionChannelId { get; set; }

        /// <summary>
        /// Кредитная линия
        /// </summary>
        public int? CreditLineId { get; set; }

        /// <summary>
        /// Идентификатор контракта
        /// </summary>
        public int? ContractId { get; set; }

        /// <summary>
        /// Данные страховки
        /// </summary>
        [Computed]
        public Guid? ApplicationOnlineInsuranceId { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Привязка к филиалу
        /// </summary>
        public int? BranchId { get; set; }

        public Guid ApplicationOnlinePositionId { get; set; }

        /// <summary>
        /// Среднемесячный доход за 6 мес со слов клиента
        /// </summary>
        public decimal? IncomeAmount { get; set; }

        /// <summary>
        /// Среднемесячный доход за 6 мес со слов клиента скорректированный менеджером
        /// </summary>
        public decimal? CorrectedIncomeAmount { get; set; }

        /// <summary>
        /// Расходы по прочим платежам со слов клиента
        /// </summary>
        public decimal? ExpenseAmount { get; set; }

        /// <summary>
        /// Расходы по прочим платежам со слов клиента скорректированный менеджером
        /// </summary>
        public decimal? CorrectedExpenseAmount { get; set; }

        /// <summary>
        /// Максимально доступный срок займа 
        /// </summary>
        public int? MaximumAvailableLoanTerm { get; set; }

        public int? ListId { get; set; }

        /// <summary>
        /// Идентификатор причины отказа
        /// </summary>
        public int? RejectionReasonId { get; set; }

        /// <summary>
        /// Комментарий к причине отказа 
        /// </summary>
        public string RejectReasonComment { get; set; }

        /// <summary>
        /// Степень похожести по документы
        /// </summary>
        public decimal? Similarity { get; set; }

        /// <summary>
        /// Код причины отказа
        /// </summary>
        [Computed]
        public string RejectionReasonCode { get; set; }

        /// <summary>
        /// День первого платежа 
        /// </summary>
        public DateTime? FirstPaymentDate { get; set; }

        /// <summary>
        /// Тип рефинанс добор базовый
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// BranchId договора 
        /// </summary>
        public int? ContractBranchId { get; set; }

        /// <summary>
        /// Ответственный верификатор
        /// </summary>
        public int? ResponsibleVerificatorId { get; set; }

        /// <summary>
        /// Ответственный администратор
        /// </summary>
        public int? ResponsibleAdminId { get; set; }

        /// <summary>
        /// Признак выдачи через кассу
        /// </summary>
        public bool IsCashIssue { get; set; }

        /// <summary>
        /// Идентификатор филиала для выдачи средств через кассу
        /// </summary>
        public int? CashIssueBranchId { get; set; }

        /// <summary>
        /// Признак отправки залога на регистрацию
        /// </summary>
        public bool EncumbranceRegistered { get; set; }

        /// <summary>
        /// Код партнёра
        /// </summary>
        public string PartnerCode { get; set; }

        /// <summary>
        /// Способ подписания заявки
        /// </summary>
        public ApplicationOnlineSignType SignType { get; set; }


        public ApplicationOnline()
        {

        }

        public ApplicationOnline(Guid id, int clientId, int percentSettingId, decimal applicationAmount, int loanTerm, ApplicationOnlineSignType signType,
            Guid applicationOnlinePositionId, bool? insurance, decimal? incomeAmount, decimal? expenseAmount, int? maximumAvailableLoanTerm,
            int listId, int? creditLineId = null, int creationAuthorId = 1, DateTime? firstPaymentDate = null, string type = null, string partnerCode = null)
        {
            Id = id;
            ClientId = clientId;
            ProductId = percentSettingId;
            CreationAuthorId = creationAuthorId;
            CreateDate = DateTime.Now;
            UpdateDate = DateTime.Now;
            Status = "Created";
            ApplicationAmount = applicationAmount;
            CreditLineId = creditLineId;
            Stage = 1;
            LoanTerm = loanTerm;
            LastChangeAuthorId = creationAuthorId;
            ApplicationOnlinePositionId = applicationOnlinePositionId;
            IncomeAmount = incomeAmount;
            ExpenseAmount = expenseAmount;
            MaximumAvailableLoanTerm = maximumAvailableLoanTerm;
            ListId = listId;
            if (firstPaymentDate.HasValue && firstPaymentDate > DateTime.Now.Date)
            {
                FirstPaymentDate = firstPaymentDate.Value.Date;
            }
            Type = type;
            EncumbranceRegistered = creditLineId.HasValue;
            PartnerCode = partnerCode;
            SignType = signType;
        }

        public bool CanEditing(int userId)
        {
            if (ResponsibleManagerId == userId || ResponsibleVerificatorId == userId || ResponsibleAdminId == userId)
                return true;
            return false;
        }

        public void Update(int? loanTerm, int? stage, string applicationSource, int? loanPurposeId,
            int? businessLoanPurposeId,
            int? okedForIndividualsPurposeId, int? targetPurposeId,
            int? attractionChannelId, int? branchId, int userId, int? maximumAvailableLoanTerm = null, DateTime? firstPaymentDate = null)
        {
            if (loanTerm.HasValue && LoanTerm != loanTerm)
            {
                LoanTerm = loanTerm.Value;
            }

            if (stage.HasValue && Stage != stage)
            {
                Stage = stage.Value;
            }

            if (!string.IsNullOrEmpty(applicationSource) && ApplicationSource != applicationSource)
            {
                ApplicationSource = applicationSource;
            }

            if (loanPurposeId.HasValue && LoanPurposeId != loanPurposeId)
            {
                LoanPurposeId = loanPurposeId;
            }

            if (BusinessLoanPurposeId != businessLoanPurposeId)
            {
                BusinessLoanPurposeId = businessLoanPurposeId;
            }

            if (OkedForIndividualsPurposeId != okedForIndividualsPurposeId)
            {
                OkedForIndividualsPurposeId = okedForIndividualsPurposeId;
            }

            if (TargetPurposeId != targetPurposeId)
            {
                TargetPurposeId = targetPurposeId;
            }

            if (attractionChannelId.HasValue && AttractionChannelId != attractionChannelId)
            {
                AttractionChannelId = attractionChannelId;
            }

            if (branchId.HasValue && BranchId != branchId)
            {
                BranchId = branchId.Value;
            }

            if (maximumAvailableLoanTerm.HasValue && MaximumAvailableLoanTerm != maximumAvailableLoanTerm)
            {
                MaximumAvailableLoanTerm = maximumAvailableLoanTerm.Value;
            }

            if (firstPaymentDate.HasValue && firstPaymentDate > DateTime.Now.Date)
            {
                FirstPaymentDate = firstPaymentDate.Value.Date;
            }

            LastChangeAuthorId = userId;
            UpdateDate = DateTime.Now;
        }

        public void Considerate(int userId, int branchId)
        {
            ContractBranchId = branchId;
            ChangeStatus(ApplicationOnlineStatus.Consideration, userId);
        }

        public void ChangeStatus(ApplicationOnlineStatus status, int userId)
        {
            if (status == ApplicationOnlineStatus.Consideration)
            {
                ResponsibleManagerId = userId;
            }

            if (status == ApplicationOnlineStatus.Verification)
            {
                ResponsibleVerificatorId = userId;
            }

            if (status == ApplicationOnlineStatus.RequisiteCheck)
            {
                ResponsibleManagerId = null;
            }

            if (status == ApplicationOnlineStatus.Approved)
                Stage = 2;

            Status = status.ToString();
            UpdateDate = DateTime.Now;
            LastChangeAuthorId = userId;
        }

        public void Reject(int userId, int rejectionReasonId, string rejectReasonCode, string comment = null)
        {
            ChangeStatus(ApplicationOnlineStatus.Declined, userId);
            RejectionReasonId = rejectionReasonId;
            RejectReasonComment = comment;
            RejectionReasonCode = rejectReasonCode;
        }

        public void Cancel(int userId, int rejectionReasonId, string rejectReasonCode, string comment = null)
        {
            ChangeStatus(ApplicationOnlineStatus.Canceled, userId);
            RejectionReasonId = rejectionReasonId;
            RejectReasonComment = comment;
            RejectionReasonCode = rejectReasonCode;
        }

        public void ToBiometricCheck(int userId, decimal similarity)
        {
            ChangeStatus(ApplicationOnlineStatus.BiometricCheck, userId);
            Similarity = similarity;
        }

        public void SetMaximumAvailableLoanTerm(int maximumAvailableLoanTerm)
        {
            UpdateDate = DateTime.Now;
            MaximumAvailableLoanTerm = maximumAvailableLoanTerm;
        }

        public void ChangeResponsibleManager(int userId)
        {
            UpdateDate = DateTime.Now;
            ResponsibleManagerId = userId;
        }

        public void ChangeResponsibleActor(int userId, int? branchId = null)
        {
            if (Status == ApplicationOnlineStatus.Created.ToString() ||
                Status == ApplicationOnlineStatus.Consideration.ToString() ||
                Status == ApplicationOnlineStatus.RequisiteCheck.ToString() ||
                Status == ApplicationOnlineStatus.ModificationFromVerification.ToString())
            {

                ContractBranchId = branchId;
                ResponsibleManagerId = userId;
            }

            if (Status == ApplicationOnlineStatus.Verification.ToString() ||
                Status == ApplicationOnlineStatus.Approved.ToString() ||
                Status == ApplicationOnlineStatus.OnEstimation.ToString() ||
                Status == ApplicationOnlineStatus.EstimationCompleted.ToString() ||
                Status == ApplicationOnlineStatus.RequiresCorrection.ToString() ||
                Status == ApplicationOnlineStatus.BiometricCheck.ToString() ||
                Status == ApplicationOnlineStatus.BiometricPassed.ToString() ||
                Status == ApplicationOnlineStatus.Declined.ToString())
            {
                ResponsibleVerificatorId = userId;
            }
        }

        public void DeleteResponsibleManager()
        {
            UpdateDate = DateTime.Now;
            ResponsibleManagerId = null;
        }

        public List<string> CheckForVerification()
        {
            List<string> emptyFields = new List<string>();
            if (!AttractionChannelId.HasValue)
                emptyFields.Add("Канал привлечения");
            if (!LoanPurposeId.HasValue)
                emptyFields.Add(" Цель кредита");

            return emptyFields;
        }

        public void ChangeApplicationAmount(decimal amount, int userId)
        {
            ApplicationAmount = amount;
            UpdateDate = DateTime.Now;
            LastChangeAuthorId = userId;
        }

        public List<string> CheckForApprove()
        {
            List<string> emptyFields = new List<string>();
            if (!BranchId.HasValue)
                emptyFields.Add("Привязка к филиалу");

            return emptyFields;
        }

        public void SetContractBranch(int contractBranchId)
        {
            ContractBranchId = contractBranchId;
        }
    }
}
