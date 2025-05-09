using System;
using System.Collections;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.Extensions.Options;
using Pawnshop.Core.Options;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Engine.Audit;
using Newtonsoft.Json;
using Pawnshop.Core;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Hangfire;
using Microsoft.EntityFrameworkCore.Internal;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Dictionaries.Address;
using Pawnshop.Data.Models.Egov;
using RestSharp;
using Pawnshop.Web.Models.Dictionary;
using RestSharp.Extensions;

namespace Pawnshop.Web.Engine.Jobs
{
    public class EGOVDictionaryActualizationJob
    {
        private readonly EnviromentAccessOptions _options;
        private readonly ReportDataRepository _reportDataRepository;
        private readonly JobLog _jobLog;
        private readonly AddressATETypeRepository _addressATETypeRepository;
        private readonly AddressBuildingTypeRepository _addressBuildingTypeRepository;
        private readonly AddressGeonimTypeRepository _addressGeonimTypeRepository;
        private readonly AddressRoomTypeRepository _addressRoomTypeRepository;
        private readonly AddressATERepository _addressATERepository;
        private readonly AddressBuildingRepository _addressBuildingRepository;
        private readonly AddressGeonimRepository _addressGeonimRepository;
        private readonly AddressRoomRepository _addressRoomRepository;
        private readonly AddressRepository _addressRepository;

        public EGOVDictionaryActualizationJob(IOptions<EnviromentAccessOptions> options, ReportDataRepository reportDataRepository, JobLog jobLog,
            AddressATETypeRepository addressATETypeRepository, AddressBuildingTypeRepository addressBuildingTypeRepository,
            AddressGeonimTypeRepository addressGeonimTypeRepository, AddressRoomTypeRepository addressRoomTypeRepository,
            AddressATERepository addressATERepository, AddressBuildingRepository addressBuildingRepository,
            AddressGeonimRepository addressGeonimRepository, AddressRoomRepository addressRoomRepository,
            AddressRepository addressRepository)
        {
            _options = options.Value;
            _reportDataRepository = reportDataRepository;
            _jobLog = jobLog;
            _addressATETypeRepository = addressATETypeRepository;
            _addressBuildingTypeRepository = addressBuildingTypeRepository;
            _addressGeonimTypeRepository = addressGeonimTypeRepository;
            _addressRoomTypeRepository = addressRoomTypeRepository;
            _addressATERepository = addressATERepository;
            _addressBuildingRepository = addressBuildingRepository;
            _addressGeonimRepository = addressGeonimRepository;
            _addressRoomRepository = addressRoomRepository;
            _addressRepository = addressRepository;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        public void Execute()
        {
            if (!_options.DictionaryActualizationEGOV)
            {
               return;
            }

            List<string> queue = new List<string>()
            { /*"AddressATEType",
                "AddressGeonimType",
                "AddressBuildingType",
                "AddressRoomType",*/
                "AddressATE",
                "AddressGeonim"/*,
                "AddressBuilding",
                "AddressRoom" */
            };
            queue.ForEach(item =>
            {
                try
                {
                    var dictionary = EgovDictionaries.All.FirstOrDefault(x => x.PawnshopType.Name == item);
                    if (dictionary == null)
                    {
                        throw new PawnshopApplicationException($"Справочник {item} не найден");
                    }

                    var url = $"https://data.egov.kz/api/detailed/{dictionary.OuterName}/data";

                    var client = new RestClient(url)
                    {
                        Timeout = -1
                    };
                    var request = new RestRequest(Method.GET);

                    if (dictionary.IsType)
                    {
                        IRestResponse response = client.Execute(request);

                        dynamic answer = JsonConvert.DeserializeObject(response.Content);
                        int totalCount = answer.totalCount;
                        var filter = new {size = totalCount};
                        client = new RestClient(string.Concat(url, "?source=", JsonConvert.SerializeObject(filter)));
                        response = client.Execute(request);

                        if (response.IsSuccessful)
                        {
                            if (dictionary.PawnshopType == typeof(AddressATEType))
                            {
                                if (totalCount == _addressATETypeRepository.Count(new ListQuery())) return;

                                var res =
                                    (dynamic) JsonConvert.DeserializeObject(response.Content);

                                var types = new List<AddressATEType>();

                                foreach (var r in res.data)
                                {
                                    var type = new AddressATEType();

                                    type.Id = r.id;
                                    type.NameRus = r.value_ru;
                                    type.NameKaz = r.value_kz;
                                    type.ShortNameRus = r.short_value_ru;
                                    type.ShortNameKaz = r.short_value_kz;
                                    type.Code = r.code;
                                    type.IsActual = ParseBool(r.actual.ToString());

                                    types.Add(type);
                                }

                                _addressATETypeRepository.InsertOrUpdate(types);
                            }
                            else if (dictionary.PawnshopType == typeof(AddressBuildingType))
                            {
                                if (totalCount == _addressBuildingTypeRepository.Count(new ListQuery())) return;

                                var res =
                                    (dynamic) JsonConvert.DeserializeObject(response.Content);

                                var types = new List<AddressBuildingType>();

                                foreach (var r in res.data)
                                {
                                    var type = new AddressBuildingType();

                                    type.Id = r.id;
                                    type.NameRus = r.value_ru;
                                    type.NameKaz = r.value_kz;
                                    type.ShortNameRus = r.short_value_ru;
                                    type.ShortNameKaz = r.short_value_kz;
                                    type.Code = r.code;
                                    type.IsActual = ParseBool(r.actual.ToString());

                                    types.Add(type);
                                }

                                _addressBuildingTypeRepository.InsertOrUpdate(types);
                            }
                            else if (dictionary.PawnshopType == typeof(AddressGeonimType))
                            {
                                if (totalCount == _addressGeonimRepository.Count(new ListQuery())) return;

                                var res =
                                    (dynamic) JsonConvert.DeserializeObject(response.Content);

                                var types = new List<AddressGeonimType>();

                                foreach (var r in res.data)
                                {
                                    var type = new AddressGeonimType();

                                    type.Id = r.id;
                                    type.NameRus = r.value_ru;
                                    type.NameKaz = r.value_kz;
                                    type.ShortNameRus = r.short_value_ru;
                                    type.ShortNameKaz = r.short_value_kz;
                                    type.Code = r.code;
                                    type.IsActual = ParseBool(r.actual.ToString());

                                    types.Add(type);
                                }

                                _addressGeonimTypeRepository.InsertOrUpdate(types);
                            }
                            else if (dictionary.PawnshopType == typeof(AddressRoomType))
                            {
                                if (totalCount == _addressRoomTypeRepository.Count(new ListQuery())) return;

                                var res =
                                    (dynamic) JsonConvert.DeserializeObject(response.Content);

                                var types = new List<AddressRoomType>();

                                foreach (var r in res.data)
                                {
                                    var type = new AddressRoomType();

                                    type.Id = r.id;
                                    type.NameRus = r.value_ru;
                                    type.NameKaz = r.value_kz;
                                    type.ShortNameRus = r.short_value_ru;
                                    type.ShortNameKaz = r.short_value_kz;
                                    type.Code = r.code;
                                    type.IsActual = ParseBool(r.actual.ToString());

                                    types.Add(type);
                                }

                                _addressRoomTypeRepository.InsertOrUpdate(types);
                            }
                        }
                        else return;
                    }
                    else
                    {
                        IRestResponse response = client.Execute(request);

                        dynamic answer = JsonConvert.DeserializeObject(response.Content);

                        DateTime lastDate = GetLastDate(dictionary);
                        int totalCount = answer.totalCount;
                        int step = 1000;
                        int counter = 0;
                        while (counter < totalCount)
                        {
                            var filter = new
                            {
                                from = counter, size = step, sort = new object[] {new {modified = new {order = "desc"}}}
                        };
                            client = new RestClient(string.Concat(url, "?source=",
                                JsonConvert.SerializeObject(filter)));
                            response = client.Execute(request);

                            if (dictionary.PawnshopType == typeof(AddressATE))
                            {
                                //if (totalCount == _addressATERepository.Count(new ListQuery())) return;

                                var res =
                                    (dynamic) JsonConvert.DeserializeObject(response.Content);

                                var types = new List<AddressATE>();

                                if (res.data != null)
                                {
                                    foreach (var r in res.data)
                                    {
                                        var type = new AddressATE();

                                        type.Id = r.id;
                                        type.ParentId = int.TryParse(r.parent_id.ToString(), out int par)
                                            ? int.Parse(r.parent_id.ToString())
                                            : null;
                                        type.ATETypeId = r.d_ats_type_id;
                                        type.FullPathRus = r?.full_path_rus;
                                        type.FullPathKaz = r?.full_path_kaz;
                                        type.NameRus = r.name_rus;
                                        type.NameKaz = r.name_kaz;
                                        type.KATOCode = int.TryParse(r.cato.ToString(), out int cato)
                                            ? int.Parse(r.cato.ToString())
                                            : null;
                                        type.IsActual = ParseBool(r.actual.ToString());
                                        type.ModifyDate = r?.modified;
                                        type.RCACode = r?.rco;

                                        types.Add(type);
                                    }

                                    _addressATERepository.InsertOrUpdate(types);

                                    if (types.Max(x => x.ModifyDate) <= lastDate) break;
                                }
                            }
                            else if (dictionary.PawnshopType == typeof(AddressGeonim))
                            {
                                if (totalCount == _addressGeonimRepository.Count(new ListQuery())) return;

                                var res =
                                    (dynamic) JsonConvert.DeserializeObject(response.Content);

                                var types = new List<AddressGeonim>();

                                if (res.data != null)
                                {
                                    foreach (var r in res.data)
                                    {
                                        var type = new AddressGeonim();

                                        type.Id = r.id;
                                        type.ParentId = int.TryParse(r.parent_id.ToString(), out int par)
                                            ? int.Parse(r.parent_id.ToString())
                                            : null;
                                        type.ATEId = r.s_ats_id;
                                        type.GeonimTypeId = r.d_geonims_type_id;
                                        type.FullPathRus = r?.full_path_rus;
                                        type.FullPathKaz = r?.full_path_kaz;
                                        type.NameRus = r.name_rus;
                                        type.NameKaz = r.name_kaz;
                                        type.KATOCode = int.TryParse(r.cato.ToString(), out int cato)
                                            ? int.Parse(r.cato.ToString())
                                            : null;
                                        type.IsActual = r.actual == 1 || r.actual.Value == "true";
                                        type.ModifyDate = r?.modified;
                                        type.RCACode = r?.rco;

                                        //type.ATETypeId = r.d_ats_type_id; TODO: узнать у Элоны где взять значение этого поля

                                        types.Add(type);
                                    }
                                }

                                _addressGeonimRepository.InsertOrUpdate(types);
                                if (types.Max(x => x.ModifyDate) <= lastDate) break;
                            }
                            else return;

                            counter += step;
                        }

                    }

                }
                catch (Exception ex)
                {
                    _jobLog.Log("EGOVDictionaryActualizationJob", JobCode.Error, JobStatus.Failed,
                        requestData: JsonConvert.SerializeObject(queue), responseData: JsonConvert.SerializeObject(ex));
                }
                finally
                {
                    _addressRepository.CalculateHasChild();
                    _addressRepository.UpdateNullFullPath();
                }
            });
        }

        private DateTime GetLastDate(EgovDictionary dictionary)
        {
            if (dictionary.PawnshopType == typeof(AddressATE))
            {
                return _addressATERepository.GetLastModifiedDate();
            }
            else if (dictionary.PawnshopType == typeof(AddressGeonim))
            {
                return _addressGeonimRepository.GetLastModifiedDate();
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        private bool ParseBool(string input)
        {
            switch (input.ToLower())
            {
                case "1":
                case "true":
                    return true;
                default: return false;
            }
        }
    }
}
