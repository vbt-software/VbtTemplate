using Core.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Core.CoreContext
{
    public class CoreContext : ICoreContext
    {
        private readonly IOptions<VbtConfig> _vbtConfig;
        public CoreContext(IOptions<VbtConfig> vbtConfig)
        {
            _vbtConfig = vbtConfig;
        }
        public string CdnUrl => _vbtConfig.Value.CdnUrl;
        public string EnvironmentName => _vbtConfig.Value.EnvironmentName;
        public string CdnPath => _vbtConfig.Value.CdnPath;
        public string MobileVersion => _vbtConfig.Value.MobileVersion;
        public string DefaultCultureName => _vbtConfig.Value.DefaultCultureName;
        public int TokenExpireTime => _vbtConfig.Value.TokenExpireTime;
        public int RefreshTokenExpireTime => _vbtConfig.Value.RefreshTokenExpireTime;
        public bool Contains(string source, string destination)
        {
            return CultureInfo.GetCultureInfo(DefaultCultureName)
                .CompareInfo.IndexOf(source, destination, CompareOptions.IgnoreCase) > -1;
        }
    }
}
