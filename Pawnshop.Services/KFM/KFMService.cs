using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.KFM;
using Pawnshop.Services.KFM.Models;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System;
using System.Globalization;
using Serilog;
using Pawnshop.Services.Exceptions;

namespace Pawnshop.Services.KFM
{
    public class KFMService : IKFMService
    {
        private readonly KFMPersonRepository _kfmPersonRepository;
        private readonly OuterServiceSettingRepository _outerServiceSettingRepository;
        private readonly ILogger _logger;

        public KFMService(KFMPersonRepository kfmPersonRepository,
            OuterServiceSettingRepository outerServiceSettingRepository,
            ILogger logger)
        {
            _kfmPersonRepository = kfmPersonRepository;
            _outerServiceSettingRepository = outerServiceSettingRepository;
            _logger = logger;
        }

        public async Task<bool> FindByClientIdAsync(int clientId) =>
            await _kfmPersonRepository.FindByClientIdAsync(clientId);

        public async Task<bool> FindByIdentityNumberAsync(string identityNumber) =>
            await _kfmPersonRepository.FindByIdentityNumberAsync(identityNumber);

        public async Task<IEnumerable<KFMPerson>> FindListAsync(object query) =>
            await _kfmPersonRepository.FindListAsync(query);

        public async Task<List<KFMPerson>> GetListAsync()
        {
            var kfmConfig = _outerServiceSettingRepository.Find(new { Code = Constants.KFM_SETTING });
            var url = kfmConfig.URL;

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode != true)
                        return null;

                    var body = await response.Content.ReadAsStringAsync();

                    XmlSerializer serializer = new XmlSerializer(typeof(XmlRoot));
                    using (StringReader reader = new StringReader(body))
                    {
                        var list = (XmlRoot)serializer.Deserialize(reader);
                        var kfmList = new List<KFMPerson>();

                        foreach (var item in list?.Persons?.Person)
                        {
                            var resultParseBirthDate = DateTime.TryParseExact(item.Birthdate, "dd.MM.yyyy",
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out var birthDate);

                            kfmList.Add(new KFMPerson
                            {
                                Birthdate = resultParseBirthDate ? birthDate : (DateTime?)default,
                                Correction = item.Correction,
                                Name = item.Fname,
                                IdentityNumber = item.Iin,
                                Surname = item.Lname,
                                Patronymic = item.Mname,
                                Note = item.Note,
                                Num = item.Num,
                                UploadDate = DateTime.Now,
                            });
                        }

                        return kfmList;
                    }
                }
            }
            catch (HttpRequestException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException($"{url}", exception.Message);
            }
            catch (TaskCanceledException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException($"{url}", exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
        }
    }
}
