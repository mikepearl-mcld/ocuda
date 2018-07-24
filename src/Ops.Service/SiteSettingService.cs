﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Defaults;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class SiteSettingService : ISiteSettingService
    {
        private readonly ILogger _logger;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;
        private readonly ISiteSettingRepository _siteSettingRepository;
        private readonly IUserRepository _userRepository;

        private int CacheMinutes { get; set; }

        public SiteSettingService(ILogger<SiteSettingService> logger,
            IDistributedCache cache,
            IConfiguration config,
            ISiteSettingRepository siteSettingRepository,
            IUserRepository userRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _siteSettingRepository = siteSettingRepository
                ?? throw new ArgumentNullException(nameof(siteSettingRepository));
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));

            string timeoutSetting
                = _config[Utility.Keys.Configuration.OpsSiteSettingCacheMinutes] ?? "60";
            int.TryParse(timeoutSetting, out int timeout);
            CacheMinutes = timeout;
        }

        /// <summary>
        /// Ensure all site settings exist in the database.
        /// </summary>
        public async Task EnsureSiteSettingsExistAsync(int sysadminId)
        {
            var settingsToAdd = new List<SiteSetting>();

            var defaultSiteSettings = SiteSettings.Get;
            foreach (var defaultSetting in defaultSiteSettings)
            {
                var siteSetting = await _siteSettingRepository.FindByKeyAsync(defaultSetting.Key);
                if (siteSetting == null)
                {
                    defaultSetting.CreatedAt = DateTime.Now;
                    defaultSetting.CreatedBy = sysadminId;
                    settingsToAdd.Add(defaultSetting);
                }
            }

            if (settingsToAdd.Count > 0)
            {
                await _siteSettingRepository.AddRangeAsync(settingsToAdd);
                await _siteSettingRepository.SaveAsync();
            }
        }

        public async Task<ICollection<SiteSetting>> GetAllAsync()
        {
            return await _siteSettingRepository.ToListAsync(_ => _.Name);
        }

        public async Task<bool> GetSettingBoolAsync(string key)
        {
            var settingValue = await GetSettingValueAsync(key);

            if (bool.TryParse(settingValue, out bool result))
            {
                return result;
            }
            else
            {
                throw new OcudaException($"Invalid value for boolean setting {key}: {settingValue}");
            }
        }

        public async Task<int> GetSettingIntAsync(string key)
        {
            var settingValue = await GetSettingValueAsync(key);

            if (int.TryParse(settingValue, out int result))
            {
                return result;
            }
            else
            {
                throw new OcudaException($"Invalid value for integer setting {key}: {settingValue}");
            }
        }

        public async Task<string> GetSettingStringAsync(string key)
        {
            return await GetSettingValueAsync(key);
        }

        private async Task<string> GetSettingValueAsync(string key)
        {
            if (CacheMinutes == 0)
            {
                var siteSetting = await _siteSettingRepository.FindByKeyAsync(key);
                return siteSetting.Value;
            }
            else
            {
                string cacheKey = string.Format(Utility.Keys.Cache.OpsSiteSetting, key);
                var value = await _cache.GetStringAsync(key);
                if (value == null)
                {
                    var siteSetting = await _siteSettingRepository.FindByKeyAsync(key);
                    value = siteSetting.Value;
                    await _cache.SetStringAsync(cacheKey, value, new DistributedCacheEntryOptions()
                            .SetAbsoluteExpiration(new TimeSpan(0, CacheMinutes, 0)));
                }
                return value;
            }
        }

        public async Task<SiteSetting> UpdateAsync(string key, string value)
        {
            var currentSetting = await _siteSettingRepository.FindByKeyAsync(key);

            if (currentSetting.Type == SiteSettingType.Bool)
            {
                if (!bool.TryParse(value, out bool result))
                {
                    _logger.LogError($"Invalid format for boolean key {key}: {value}");
                    throw new OcudaException("Invald format.");
                }
            }
            else if (currentSetting.Type == SiteSettingType.Int)
            {
                if (!int.TryParse(value, out int result))
                {
                    _logger.LogError($"Invalid format for integer key {key}: {value}");
                    throw new OcudaException("Invald format.");
                }
            }

            currentSetting.Value = value;

            await ValidateSiteSettingAsync(currentSetting);

            _siteSettingRepository.Update(currentSetting);
            await _siteSettingRepository.SaveAsync();

            if (CacheMinutes > 0)
            {
                string cacheKey = string.Format(Utility.Keys.Cache.OpsSiteSetting, key);
                await _cache.SetStringAsync(cacheKey, value, new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(new TimeSpan(0, CacheMinutes, 0)));
            }

            return currentSetting;
        }

        public async Task ValidateSiteSettingAsync(SiteSetting siteSetting)
        {
            var message = string.Empty;

            if (await _siteSettingRepository.IsDuplicateKey(siteSetting.Key))
            {
                message = $"Site Setting with key '{siteSetting.Key}' already exists.";
                _logger.LogWarning(message, siteSetting.Key);
                throw new OcudaException(message);
            }

            if (string.IsNullOrWhiteSpace(siteSetting.Key))
            {
                message = $"Site Setting must have a key.";
                _logger.LogWarning(message);
                throw new OcudaException(message);
            }

            if (string.IsNullOrWhiteSpace(siteSetting.Name))
            {
                message = $"Site Setting must have a name.";
                _logger.LogWarning(message);
                throw new OcudaException(message);
            }

            if (string.IsNullOrWhiteSpace(siteSetting.Value))
            {
                message = $"Site Setting value cannot be empty.";
                _logger.LogWarning(message);
                throw new OcudaException(message);
            }

            if (!Enum.IsDefined(typeof(SiteSettingType), siteSetting.Type))
            {
                message = $"Site Setting type is invalid.";
                _logger.LogWarning(message, siteSetting.Type);
                throw new OcudaException(message);
            }

            if (siteSetting.Type == SiteSettingType.Bool)
            {
                if (!bool.TryParse(siteSetting.Value, out bool result))
                {
                    message = $"{siteSetting.Name} requires a value of type {siteSetting.Type}.";
                    _logger.LogWarning(message, siteSetting.Value, result);
                    throw new OcudaException(message);
                }
            }

            else if (siteSetting.Type == SiteSettingType.Int)
            {
                if (!int.TryParse(siteSetting.Value, out int result))
                {
                    message = $"{siteSetting.Name} requires a value of type {siteSetting.Type}.";
                    _logger.LogWarning(message, siteSetting.Value, result);
                    throw new OcudaException(message);
                }
            }

            var creator = await _userRepository.FindAsync(siteSetting.CreatedBy);
            if (creator == null)
            {
                message = $"Created by invalid User Id: {siteSetting.CreatedBy}";
                _logger.LogWarning(message, siteSetting.CreatedBy);
                throw new OcudaException(message);
            }
        }
    }
}
