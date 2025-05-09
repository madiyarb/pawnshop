using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Services.Clients;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Models.Clients.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Engine.Services
{
    public class ClientAssetService : IClientAssetService
    {
        private readonly ClientAssetRepository _clientAssetRepository;
        private readonly IClientService _clientService;
        private readonly IDomainService _domainService;
        private readonly ISessionContext _sessionContext;
        public ClientAssetService(ClientAssetRepository clientAssetRepository, IClientService clientService, 
            IDomainService domainService, ISessionContext sessionContext)
        {
            _clientAssetRepository = clientAssetRepository;
            _clientService = clientService;
            _domainService = domainService;
            _sessionContext = sessionContext;
        }

        public List<ClientAsset> Get(int clientId)
        {
            _clientService.CheckClientExists(clientId);
            return _clientAssetRepository.GetListByClientId(clientId);
        }
        public List<ClientAsset> Save(int clientId, List<ClientAssetDto> assets)
        {
            if (assets == null)
                throw new ArgumentNullException(nameof(assets));

            Validate(assets);
            List<ClientAsset> assetsFromDB = Get(clientId);
            HashSet<int> uniqueIdsFromAssets = assets.Where(e => e.Id != default).Select(e => e.Id).ToHashSet();
            Dictionary<int, ClientAsset> assetsFromDBDict = assetsFromDB.ToDictionary(e => e.Id, e => e);
            if (!uniqueIdsFromAssets.IsSubsetOf(assetsFromDBDict.Keys))
                throw new PawnshopApplicationException($"В аргументе {nameof(assets)} присутствуют несуществующие или не принадлежащие Id активов данного клиента");

            var syncList = new List<ClientAsset>();
            foreach (ClientAssetDto asset in assets)
            {
                ClientAsset assetFromDB = null;
                if (!assetsFromDBDict.TryGetValue(asset.Id, out assetFromDB))
                {
                    assetFromDB = new ClientAsset
                    {
                        ClientId = clientId,
                        AssetTypeId = asset.AssetTypeId.Value,
                        Count = asset.Count.Value,
                        AuthorId = _sessionContext.UserId
                    };
                }
                else
                {
                    assetFromDB.AssetTypeId = asset.AssetTypeId.Value;
                    assetFromDB.Count = asset.Count.Value;
                    assetsFromDBDict.Remove(asset.Id);
                }

                syncList.Add(assetFromDB);
            }

            // если есть что менять, то вызываем транзакцию
            if (syncList.Count > 0 || assetsFromDBDict.Count > 0)
                using (var transaction = _clientAssetRepository.BeginTransaction())
                {
                    // удаляем ненужные места работ
                    foreach ((int id, ClientAsset _) in assetsFromDBDict)
                    {
                        _clientAssetRepository.Delete(id);
                    }

                    foreach (ClientAsset asset in syncList)
                    {
                        if (asset.Id == default)
                        {
                            _clientAssetRepository.Insert(asset);
                            _clientAssetRepository.LogChanges(asset, _sessionContext.UserId, true);
                        }
                        else
                        {
                            _clientAssetRepository.Update(asset);
                            _clientAssetRepository.LogChanges(asset, _sessionContext.UserId);
                        }
                    }

                    transaction.Commit();
                }

            return syncList;
        }

        private void Validate(List<ClientAssetDto> assets)
        {
            if (assets == null)
                throw new ArgumentNullException(nameof(assets));

            HashSet<int> assetTypeDomainValuesSet = _domainService.GetDomainValues(Constants.ASSET_TYPE_DOMAIN).Select(dv => dv.Id).ToHashSet();
            var uniqueAssetTypes = new HashSet<int>();
            var errors = new HashSet<string>();
            foreach (ClientAssetDto asset in assets)
            {
                if (asset == null)
                    errors.Add($"В аргументе {nameof(assets)} присутствуют пустые элементы");
                else
                {
                    if (!asset.AssetTypeId.HasValue)
                        errors.Add($"Поле {nameof(asset.AssetTypeId)} не должно быть пустым");
                    else
                    {
                        if (!assetTypeDomainValuesSet.Contains(asset.AssetTypeId.Value))
                            errors.Add($"Поле {nameof(asset.AssetTypeId)} имеет неверное значение");
                        if (uniqueAssetTypes.Contains(asset.AssetTypeId.Value))
                            errors.Add($"Поле {nameof(asset.AssetTypeId)} должно быть уникальным");

                        uniqueAssetTypes.Add(asset.AssetTypeId.Value);
                    }

                    if (!asset.Count.HasValue)
                        errors.Add($"Поле {nameof(asset.Count)} не должно быть пустым");
                    else if (asset.Count.Value <= 0)
                        errors.Add($"Поле {nameof(asset.Count)} должно быть положительным числом");
                }
            }

            if (errors.Count > 0)
                throw new PawnshopApplicationException(errors.ToArray());
        }
    }
}
