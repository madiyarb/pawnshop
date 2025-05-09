using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.ReportData;


namespace Pawnshop.Services.ReportDatas
{
    public class ReportDataService : IReportDataService
    {

        private readonly ReportDataRepository _reportDataRepository;
        private readonly ReportDataRowRepository _reportDataRowRepository;
        private readonly IEventLog _eventLog;
        private readonly ISessionContext _sessionContext;


        public ReportDataService(ReportDataRepository reportDataRepository, ReportDataRowRepository reportDataRowRepository, 
            IEventLog eventLog, ISessionContext sessionContext)
        {
            _reportDataRepository = reportDataRepository;
            _reportDataRowRepository = reportDataRowRepository;
            _eventLog = eventLog;
            _sessionContext = sessionContext;
        }

        public ReportData Save(ReportData model)
        {
            model = _reportDataRepository.Insert(model);
            foreach (ReportDataRow row in model.Rows)
            {
                row.ReportDataId = model.Id;
                row.Id = _reportDataRowRepository.Insert(row);
            }
            return model;
        }

        public void Delete(int id)
        {
            _reportDataRepository.Delete(id);
        }

        public ReportDataResponseModel Create(ReportDataModel model, int organizationId, int branchId)
        {
            var reportDataResponse = new ReportDataResponseModel();
           try
            {
                using (var transaction = _reportDataRepository.BeginTransaction())
                {
                    reportDataResponse = Validate(model, organizationId, branchId);
                    if (reportDataResponse.Code != null)
                    {
                        throw new PawnshopApplicationException();
                    }

                    var reportDataForDelete = _reportDataRepository.GetForDelete(organizationId, branchId, model.Date.Date);
                    if (reportDataForDelete != 0)
                        _reportDataRepository.Delete(reportDataForDelete);

                    var reportData = new ReportData(0, organizationId, branchId, model.Date.Date, model.Rows);

                    reportData = this.Save(reportData);

                    reportDataResponse.Code = ((int)ReportDataResponse.Success).ToString();
                    reportDataResponse.Message = ReportDataResponse.Success.GetDisplayName();

                    _eventLog.Log(EventCode.ReportDataSuccess, EventStatus.Success, EntityType.ReportData, reportData.Id, userId: _sessionContext.UserId, requestData: JsonConvert.SerializeObject(model), responseData: JsonConvert.SerializeObject(reportDataResponse));
                    transaction.Commit();
                }
            }
            catch(Exception e)
            {
                if (reportDataResponse.Code == null || reportDataResponse.Code.Equals("0"))
                {
                    reportDataResponse.Code = ((int)ReportDataResponse.OtherError).ToString();
                    reportDataResponse.Message = e.StackTrace;
                }
                _eventLog.Log(EventCode.ReportDataError, EventStatus.Failed, EntityType.ReportData, userId: _sessionContext.UserId, requestData: JsonConvert.SerializeObject(model), responseData: JsonConvert.SerializeObject(reportDataResponse));
            }
            return reportDataResponse;

        }


        private ReportDataResponseModel Validate(ReportDataModel model, int organizationId, int branchId)
        {
            ReportDataResponseModel reportDataResponse = new ReportDataResponseModel();

            if(model == null)
            {
                reportDataResponse.Code  =  ((int) ReportDataResponse.IncorrectFormat).ToString();
                reportDataResponse.Message = ReportDataResponse.IncorrectFormat.GetDisplayName();
                return reportDataResponse;
            }

            if (model.Date > _reportDataRepository.Now() || model.Date.Equals(new DateTime()))
            {
                reportDataResponse.Code = ((int)ReportDataResponse.IncorrectDate).ToString();
                reportDataResponse.Message = ReportDataResponse.IncorrectDate.GetDisplayName();
                return reportDataResponse;
            }

            if (model.Rows == null || model.Rows.Count ==0)
            {
                reportDataResponse.Code = ((int)ReportDataResponse.EmptyList).ToString();
                reportDataResponse.Message = ReportDataResponse.EmptyList.GetDisplayName();
                return reportDataResponse;
            }
            var invalidRows = model.Rows.Where(x => x.Value<0).ToList();
            if (invalidRows.Any())
            {
                reportDataResponse.Code = ((int)ReportDataResponse.NegativeValues).ToString();
                reportDataResponse.Message = ReportDataResponse.NegativeValues.GetDisplayName();
                reportDataResponse.ErrorRows = GetKeyFromDataRow(invalidRows);
                return reportDataResponse;
            }
            var keysInt = this.KeysInt();
            var keysInput = GetKeyFromDataRow(model.Rows);


            //недостающие ключи
            var notAllKeys = keysInt.Except(keysInput).ToList();
            if(notAllKeys.Any())
            {
                reportDataResponse.Code = ((int)ReportDataResponse.MissingKeys).ToString();
                reportDataResponse.Message = ReportDataResponse.MissingKeys.GetDisplayName();
                reportDataResponse.ErrorRows = notAllKeys;
                return reportDataResponse;
            }
            //несуществующие ключи
            var wrongKeys = keysInput.Except(keysInt).ToList();
            if (wrongKeys.Any())
            {
                reportDataResponse.Code = ((int)ReportDataResponse.WrongKeys).ToString();
                reportDataResponse.Message = ReportDataResponse.WrongKeys.GetDisplayName();
                reportDataResponse.ErrorRows = wrongKeys;
                return reportDataResponse;
            }
            //Дупликаты
            var duplicateKeys = keysInput.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            if (duplicateKeys.Any())
            {
                reportDataResponse.Code = ((int)ReportDataResponse.DuplicateKeys).ToString();
                reportDataResponse.Message = ReportDataResponse.DuplicateKeys.GetDisplayName();
                reportDataResponse.ErrorRows = duplicateKeys;
                return reportDataResponse;
            }
            return reportDataResponse;
        }

        private List<int> KeysInt()
        {

            var result = new List<int>();
            foreach (var value in Enum.GetValues(typeof(ReportDataKey)))
            {
                var type = Enum.Parse<ReportDataKey>(value.ToString());
                result.Add((int)type);
            }

            return result;
        }

        private List<int> GetKeyFromDataRow(List<ReportDataRow> reportDataRows)
        {
            var result = new List<int>();

            foreach(ReportDataRow row in reportDataRows)
            {
                var type = Enum.Parse<ReportDataKey>(row.Key.ToString());
                result.Add((int)type);
            }

            return result;
        }
    }
}
