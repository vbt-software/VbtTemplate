using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Configuration
{
    public class VbtConfig
    {
        #region Props

        public string WebApiUrl { get; set; }
        public string VbtGlobalPassword { get; set; }
        public string DefaultCultureName { get; set; }
        public string CdnUrl { get; set; }
        public string CdnPath { get; set; }
        public string PrivateKey { get; set; }
        public string RedisEndPoint { get; set; }
        public int RedisPort { get; set; }
        public string MobileVersion { get; set; }
        public string EnvironmentName { get; set; }
        public int TokenExpireTime { get; set; }
        public int TokenBeHalfOfExpireTime { get; set; }
        public int RefreshTokenExpireTime { get; set; }
        public int MobileRefreshTokenExpireTime { get; set; }        

        #endregion
        public VbtConfig()
        {
            PrivateKey = "2909012565820034"; //TODO Db den alınacak 
        }
    }
}
