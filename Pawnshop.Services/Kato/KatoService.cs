using Pawnshop.Data.Access;
using System;
using System.IO;
using System.Net;
using Pawnshop.Data.Models.Dictionaries.Address;
using ExcelDataReader;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Pawnshop.Data.Models.Kato;
using Newtonsoft.Json;

namespace Pawnshop.Services.Kato
{
    public class KatoService : IKatoService
    {
        private readonly JobLogRepository _jobLogRepository;
        private readonly KatoNewRepository _repository;
        private readonly AddressATERepository _oldRepository;
        private const string _jobName = "KatoJob";
        public KatoService(JobLogRepository jobLogRepository, KatoNewRepository repository, AddressATERepository oldRepository)
        {
            _jobLogRepository = jobLogRepository;
            _repository = repository;
            _oldRepository = oldRepository;
        }

        public void StartWork(string fileUrl)
        {
            using (var stream = DownloadFileFromUrl(fileUrl))
            {
                if (stream != null)
                {
                    if (MapFilDataToDbData(stream))
                    {
                        SetToDownDataToNewTable();
                    }
                }
            }
        }

        private MemoryStream DownloadFileFromUrl(string url)
        {
            MemoryStream stream = null;
            try
            {
                _jobLogRepository.Insert(new Data.Models.Audit.JobLogItem()
                {
                    JobName = _jobName,
                    JobCode = Data.Models.Audit.JobCode.Begin,
                    JobStatus = Data.Models.Audit.JobStatus.Success,
                    EntityType = Core.EntityType.KatoData,
                    CreateDate = DateTime.Now,
                });

                string currentDateString = $"{DateTime.Now.ToString("dd")}.{DateTime.Now.ToString("MM")}.{DateTime.Now.Year}";
                string localFileName = Path.Combine(Directory.GetCurrentDirectory(), "MappingData", $"KATO_{currentDateString}.xls");
                using (WebClient webClient = new WebClient())
                {
                    stream = new MemoryStream(webClient.DownloadData(url));
                }

                _jobLogRepository.Insert(new Data.Models.Audit.JobLogItem()
                {
                    JobName = _jobName,
                    JobCode = Data.Models.Audit.JobCode.Start,
                    JobStatus = Data.Models.Audit.JobStatus.Success,
                    EntityType = Core.EntityType.KatoData,
                    RequestData = url,
                    ResponseData = $"Downloaded file named: {url}",
                    CreateDate = DateTime.Now,
                });
            }
            catch (Exception ex)
            {
                _jobLogRepository.Insert(new Data.Models.Audit.JobLogItem()
                {
                    JobName = _jobName,
                    JobCode = Data.Models.Audit.JobCode.Error,
                    JobStatus = Data.Models.Audit.JobStatus.Failed,
                    EntityType = Core.EntityType.KatoData,
                    RequestData = url,
                    ResponseData = JsonConvert.SerializeObject(ex),
                    CreateDate = DateTime.Now,
                });
            }
            return stream;
        }

        private bool MapFilDataToDbData(MemoryStream stream)
        {
            int fileRowCount = 0;
            try
            {
                using (IExcelDataReader excelDataReader = ExcelReaderFactory.CreateReader(stream))
                {
                    var conf = new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = a => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true
                        }
                    };

                    DataSet dataSet = excelDataReader.AsDataSet(conf);
                    DataRowCollection row = dataSet.Tables[0].Rows;
                    fileRowCount = row.Count;

                    List<object> rowDataList = null;
                    List<object> allRowsList = new List<object>();
                    KatoRow record = new KatoRow();
                    var dbItem = new KatoNew();
                    foreach (DataRow item in row)
                    {
                        rowDataList = item.ItemArray.ToList(); //list of each rows

                        dbItem.KatoCode = rowDataList[0].ToString();
                        dbItem.Ab = Convert.ToInt32(rowDataList[1]);
                        dbItem.Cd = Convert.ToInt32(rowDataList[2]);
                        dbItem.Ef = Convert.ToInt32(rowDataList[3]);
                        dbItem.Hij = Convert.ToInt32(rowDataList[4]);
                        dbItem.NameKaz = rowDataList[6].ToString();
                        dbItem.NameRus = rowDataList[7].ToString();
                        if (rowDataList[8] == DBNull.Value || rowDataList[8].ToString() == string.Empty)
                        {
                            dbItem.Nn = 0;
                        }
                        else
                        {
                            dbItem.Nn = Convert.ToInt32(rowDataList[8]);
                        }

                        if (dbItem.Ab != record.Ab.Value)
                        {
                            record.KatoCode = dbItem.KatoCode;
                            record.Ab.Value = dbItem.Ab;
                            record.Cd.Value = dbItem.Cd;
                            record.Ef.Value = dbItem.Ef;
                            record.Hij.Value = dbItem.Hij;
                            record.NameKaz = dbItem.NameKaz;
                            record.NameRus = dbItem.NameRus;
                            record.Nn = dbItem.Nn;

                            dbItem.ParentId = 0;
                            _repository.Insert(dbItem);
                            record.Ab.Id = dbItem.Id;
                            continue;
                        }
                        else if (dbItem.Cd != record.Cd.Value)
                        {
                            record.KatoCode = dbItem.KatoCode;
                            record.Cd.Value = dbItem.Cd;
                            record.Ef.Value = dbItem.Ef;
                            record.Hij.Value = dbItem.Hij;
                            record.NameRus = dbItem.NameRus;
                            record.NameKaz = dbItem.NameKaz;
                            record.Nn = dbItem.Nn;

                            dbItem.ParentId = record.Ab.Id;
                            _repository.Insert(dbItem);
                            record.Cd.Id = dbItem.Id;
                            continue;
                        }
                        else if (dbItem.Ef != record.Ef.Value)
                        {
                            record.KatoCode = dbItem.KatoCode;
                            record.Ef.Value = dbItem.Ef;
                            record.Hij.Value = dbItem.Hij;
                            record.NameRus = dbItem.NameRus;
                            record.NameKaz = dbItem.NameKaz;
                            record.Nn = dbItem.Nn;

                            dbItem.ParentId = record.Cd.Id;
                            _repository.Insert(dbItem);
                            record.Ef.Id = dbItem.Id;
                            continue;
                        }
                        else if (dbItem.Hij != record.Hij.Value)
                        {
                            record.KatoCode = dbItem.KatoCode;
                            dbItem.ParentId = record.Ef.Id;
                            record.Hij.Value = dbItem.Hij;
                            record.NameRus = dbItem.NameRus;
                            record.NameKaz = dbItem.NameKaz;
                            record.Nn = dbItem.Nn;

                            _repository.Insert(dbItem);
                            continue;
                        }
                    }

                    _jobLogRepository.Insert(new Data.Models.Audit.JobLogItem()
                    {
                        JobName = _jobName,
                        JobCode = Data.Models.Audit.JobCode.Start,
                        JobStatus = Data.Models.Audit.JobStatus.Success,
                        EntityType = Core.EntityType.KatoData,
                        ResponseData = $"Данные сохранены в таблицу KatoNew, количество строк: {fileRowCount}",
                        CreateDate = DateTime.Now,
                    });

                    return true;
                }
            }
            catch (Exception ex)
            {
                _jobLogRepository.Insert(new Data.Models.Audit.JobLogItem()
                {
                    JobName = _jobName,
                    JobCode = Data.Models.Audit.JobCode.Error,
                    JobStatus = Data.Models.Audit.JobStatus.Failed,
                    EntityType = Core.EntityType.KatoData,
                    ResponseData = JsonConvert.SerializeObject(ex),
                    CreateDate = DateTime.Now,
                });

                return false;
            }
        }

        private void SetToDownDataToNewTable()
        {
            try
            {
                var headsRecorsList = _repository.List();
                foreach (var head in headsRecorsList)
                {
                    Recursion(head);
                }

                _jobLogRepository.Insert(new Data.Models.Audit.JobLogItem()
                {
                    JobName = _jobName,
                    JobCode = Data.Models.Audit.JobCode.Start,
                    JobStatus = Data.Models.Audit.JobStatus.Success,
                    EntityType = Core.EntityType.KatoData,
                    ResponseData = $"Просадка данных в таблицу закончена успешно",
                    CreateDate = DateTime.Now,
                });

            }
            catch (Exception ex)
            {
                _jobLogRepository.Insert(new Data.Models.Audit.JobLogItem()
                {
                    JobName = _jobName,
                    JobCode = Data.Models.Audit.JobCode.Error,
                    JobStatus = Data.Models.Audit.JobStatus.Failed,
                    EntityType = Core.EntityType.KatoData,
                    ResponseData = JsonConvert.SerializeObject(ex),
                    CreateDate = DateTime.Now,
                });
            }
        }

        private void Recursion(KatoNew record)
        {
            var childRecordList = _repository.GetByParentId(record.Id);
            foreach (var child in childRecordList)
            {
                CreateOrUpdateOldRecord(child, record.KatoCode);
            }

            foreach (var child in childRecordList)
            {
                Recursion(child);
            }
        }

        private void CreateOrUpdateOldRecord(KatoNew katoElement, string parentKatoCode)
        {
            bool isMapped = false;
            try
            {
                var oldParentRecord = _oldRepository.Get(parentKatoCode);
                var oldRecord = _oldRepository.Get(katoElement.KatoCode);
                if (oldRecord is null)
                {
                    var kazName = katoElement.NameKaz.Split(' ', '.').FirstOrDefault(x => char.IsUpper(x.ToCharArray()[0]));
                    oldRecord = _oldRepository.GetByNameInChilds(kazName, parentKatoCode);
                    if (oldRecord is null)
                    {
                        var rusName = katoElement.NameRus.Split(' ', '.').FirstOrDefault(x => char.IsUpper(x.ToCharArray()[0]));
                        oldRecord = _oldRepository.GetByNameInChilds(rusName, parentKatoCode, false);
                        if (oldRecord is null)
                        {
                            _oldRepository.Insert(
                                new AddressATE()
                                {
                                    ParentId = oldParentRecord.Id,
                                    ATETypeId = null,
                                    FullPathRus = null,
                                    FullPathKaz = null,
                                    NameRus = rusName,
                                    NameKaz = kazName,
                                    KATOCode = Convert.ToInt32(katoElement.KatoCode),
                                    IsActual = true,
                                    ModifyDate = DateTime.Now,
                                    RCACode = null
                                });
                            isMapped = true;
                        }
                        else
                        {
                            oldRecord.KATOCode = Convert.ToInt32(katoElement.KatoCode);
                            oldRecord.IsActual = true;
                            oldRecord.ParentId = oldParentRecord.Id;
                            _oldRepository.Update(oldRecord);
                            isMapped = true;
                        }
                    }
                    else
                    {
                        oldRecord.KATOCode = Convert.ToInt32(katoElement.KatoCode);
                        oldRecord.IsActual = true;
                        oldRecord.ParentId = oldParentRecord.Id;
                        _oldRepository.Update(oldRecord);
                        isMapped = true;
                    }
                }
                else
                {
                    isMapped = true;
                }

                if (isMapped)
                {
                    katoElement.Mapped = true;
                    _repository.Update(katoElement);
                }
            }
            catch (Exception ex)
            {
                isMapped = false;
                katoElement.Note = JsonConvert.SerializeObject(ex);
                _repository.Update(katoElement);
            }
        }
    }
}