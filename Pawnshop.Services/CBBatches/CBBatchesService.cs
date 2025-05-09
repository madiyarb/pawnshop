using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.FillCBBatchesManually;
using System;
using System.Collections.Generic;
using System.Text;
using OfficeOpenXml;
using Microsoft.AspNetCore.Http;
using Pawnshop.Data.Models.CreditBureaus;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.OuterServiceSettings;
using System.Net.Http;
using System.Text.RegularExpressions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Exceptions;
using System.IO;

namespace Pawnshop.Services.CBBatches
{
    public class CBBatchesService : ICBBatchesService
    {
        private readonly CBBatchRepository _cbBatchRepository;
        private readonly IEventLog _eventLog;
        private readonly CBBatchMessagesSCBRepository _batchMessagesSCBRepository;

        public CBBatchesService(
            CBBatchRepository cbBatchRepository, 
            IEventLog eventLog,
            CBBatchMessagesSCBRepository batchMessagesSCBRepository
        )
        {
            _cbBatchRepository = cbBatchRepository;
            _eventLog = eventLog;
            _batchMessagesSCBRepository = batchMessagesSCBRepository;
        }

        public List<int> CreateCBBatches(FillCBBatchesManuallyRequest req, int userId)
        {
            var cbs = JsonConvert.DeserializeObject<int[]>(req.cbs);

            List<int> batchIds = new List<int>();
            try
            {
                string contractIds;

                if (req.file != null && req.file.Length > 0 && (req.contractIds == null || req.contractIds.Length == 0))
                    contractIds = GetContractIdsFromFile(req.file);

                else if (req.contractIds != null && req.contractIds.Length > 0 && (req.file == null || req.file.Length == 0))
                    contractIds = req.contractIds;

                else
                    throw new Exception();


                foreach (int cb in cbs)
                {
                    int schemaId = 0;

                    // ПКБ
                    // ежедневные   schemaId = 4
                    // полумесячные schemaId = 3
                    if (cb == 1 && req.IsDaily)
                        schemaId = 4;
                    else if (cb == 1 && !req.IsDaily)
                        schemaId = 3;

                    // ГКБ
                    // ежедневные   schemaId = 3
                    // полумесячные schemaId = 1
                    else if (cb == 2 && req.IsDaily)
                        schemaId = 3;
                    else if (cb == 2 && !req.IsDaily)
                        schemaId = 1;
                    else
                        throw new Exception();

                    List<int> tempBatchIdList = _cbBatchRepository.FillCBBatchesManually(req.date.Date, cb, contractIds, schemaId, userId);
                    foreach (int batchId in tempBatchIdList)
                        batchIds.Add(batchId);
                }

                if (batchIds.Count == 0)
                    throw new Exception();


                _eventLog.Log(EventCode.CBBatchCreation, EventStatus.Success, EntityType.CBBatch);
            }
            catch (Exception e)
            {
                _eventLog.Log(EventCode.CBBatchCreation, EventStatus.Failed, EntityType.CBBatch);
                return null;
            }

            return batchIds;
        }

        public CBBatchStatus SetBatchStatusSCB(string messege)
        {
            int? status = _batchMessagesSCBRepository.GetStatusIdByMessage(messege);
            if (status == null) return CBBatchStatus.ErrorSCB;
            return (CBBatchStatus)status;
        }

        public CBBatch GetBatchById(int id)
        {
            return _cbBatchRepository.GetBatchById(id);
        }

        public string GetBatchStatusRequestSCB(string userId, int packageId)
        {
            StringBuilder xml = new StringBuilder();

            xml.Append("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:data=\"http://data.chdb.scb.kz\">");
            xml.Append("<soapenv:Header>");
            xml.Append($"<userId>{userId}</userId>");
            xml.Append("</soapenv:Header>");
            xml.Append("<soapenv:Body>");
            xml.Append("<data:getImportInfo>");
            xml.Append("<params>");
            xml.Append($"<packageId>{packageId}</packageId>"); ;
            xml.Append("<language>ru</language>");
            xml.Append("</params>");
            xml.Append("</data:getImportInfo>");
            xml.Append("</soapenv:Body>");
            xml.Append("</soapenv:Envelope>");

            return xml.ToString();
        }

        public string GetBatchStatusRequestFCB(OuterServiceSetting config, int batchId)
        {
            StringBuilder xml = new StringBuilder();

            xml.Append("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ws=\"https://ws.creditinfo.com\">");
            xml.Append("<soapenv:Header>");
            xml.Append("<ws:CigWsHeader>");
            xml.Append($"<ws:UserName>{config.Login}</ws:UserName>");
            xml.Append($"<ws:Password>{config.Password}</ws:Password>");
            xml.Append("</ws:CigWsHeader>");
            xml.Append("</soapenv:Header>");
            xml.Append("<soapenv:Body>");
            xml.Append("<ws:GetBatchStatus3>");
            xml.Append($"<ws:batchId>{batchId}</ws:batchId>");
            xml.Append("</ws:GetBatchStatus3>");
            xml.Append("</soapenv:Body>");
            xml.Append("</soapenv:Envelope>");

            return xml.ToString();
        }

        public void CheckConnectionToSCB(OuterServiceSetting config)
        {
            try
            {
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                using (var httpClient = new HttpClient(clientHandler))
                {
                    // в столбце Login хранится ClientId и Password в формате "ClientId:Password"
                    // в столбце Password хранится UserId
                    byte[] TextBytes = Encoding.UTF8.GetBytes(config.Login);
                    string auth = Convert.ToBase64String(TextBytes);
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + auth);

                    StringBuilder xml = new StringBuilder();

                    xml.Append("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:data=\"http://data.chdb.scb.kz\">");
                    xml.Append("<soapenv:Header>");
                    xml.Append($"<userId>{config.Password}</userId>");
                    xml.Append("</soapenv:Header>");
                    xml.Append("<soapenv:Body>");
                    xml.Append("<data:getImportInfo>");
                    xml.Append("<params>");
                    xml.Append("<paginationSupportDto>");
                    xml.Append("<firstResult>0</firstResult>");
                    xml.Append("<maxResults>1</maxResults>");
                    xml.Append("</paginationSupportDto>");
                    xml.Append("<language>ru</language>");
                    xml.Append("</params>");
                    xml.Append("</data:getImportInfo>");
                    xml.Append("</soapenv:Body>");
                    xml.Append("</soapenv:Envelope>");

                    var response = httpClient.PostAsync(config.URL, new StringContent(xml.ToString(), Encoding.UTF8, "text/xml")).Result;

                  if (!response.IsSuccessStatusCode)
                        throw new CBBatchesException($"Error: {(int)response.StatusCode} {response.ReasonPhrase}");
                }
            }
            catch (Exception e)
            {
                _eventLog.Log(EventCode.CBBatchConnection, EventStatus.Failed, EntityType.CBBatch, responseData: e.Message);
                throw;
            }
        }

        public void CheckConnectionToFCB(OuterServiceSetting config)
        {
            try
            {
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                using (var httpClient = new HttpClient(clientHandler))
                {

                    StringBuilder xml = new StringBuilder();
                    xml.Append("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ws=\"https://ws.creditinfo.com\">");
                    xml.Append("<soapenv:Header>");
                    xml.Append("<ws:CigWsHeader>");
                    xml.Append($"<ws:UserName>{config.Login}</ws:UserName>");
                    xml.Append($"<ws:Password>{config.Password}</ws:Password>");
                    xml.Append("</ws:CigWsHeader>");
                    xml.Append("</soapenv:Header>");
                    xml.Append("<soapenv:Body>");
                    xml.Append("<ws:GetVersion2/>");
                    xml.Append("</soapenv:Body>");
                    xml.Append("</soapenv:Envelope>");

                    var response = httpClient.PostAsync(config.URL, new StringContent(xml.ToString(), Encoding.UTF8, "text/xml")).Result;

                    if (!response.IsSuccessStatusCode)
                        throw new CBBatchesException($"Error: {(int)response.StatusCode} {response.ReasonPhrase}");
                }
            }
            catch (Exception e)
            {
                _eventLog.Log(EventCode.CBBatchConnection, EventStatus.Failed, EntityType.CBBatch, responseData: e.Message);
                throw;
            }
        }

        private string GetContractIdsFromFile(IFormFile file)
        {
            StringBuilder contractIds = new StringBuilder();
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            using (var package = new ExcelPackage(file.OpenReadStream()))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;

                string Needed_columnName = "";
                for (int col = 0; col < colCount; col++)
                {
                    string columnName = GetColumnName(col + 1);
                    for (int row = 0; row < rowCount; row++)
                    {
                        // в worksheet.Cells отсчет от 1 а не от 0
                        string cellValue = worksheet.Cells[row + 1, col + 1].Value?.ToString();
                        cellValue = cellValue.ToLower();

                        if (cellValue != null && (cellValue.Contains("id") || cellValue.Contains("ids") || cellValue.Contains("код контракта") || cellValue.Contains("общее количество")))
                        {
                            Needed_columnName = columnName;
                            continue;
                        }

                        if (columnName != Needed_columnName)
                            break;

                        if (cellValue != null && columnName == Needed_columnName && columnName != "" && Needed_columnName != "")
                        {
                            // удаление всех символов кроме цифр и '-'
                            cellValue = Regex.Replace(cellValue, @"[^0-9-]+", "");

                            if (cellValue.Length >= 10 || cellValue.Contains("-"))
                                continue;

                            contractIds.Append(cellValue + ",");
                        }

                        if (cellValue == null)
                            break;

                    }
                }
            }
            string result = contractIds.ToString();
            if (result == "" || result.Length == 0 || result == null)
                throw new Exception();
            return result;
        }

        private string GetColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;

            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }
    }
}
