using Core.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Caching
{
    public class RedisCacheService : IRedisCacheService
    {
        #region Fields

        public readonly IOptions<VbtConfig> _vbtConfig;
        private readonly RedisEndpoint conf = null;

        #endregion

        public RedisCacheService(IOptions<VbtConfig> vbtConfig)
        {
            _vbtConfig = vbtConfig;
            conf = new RedisEndpoint { Host = _vbtConfig.Value.RedisEndPoint, Port = _vbtConfig.Value.RedisPort, Password = "" };
        }
        public T Get<T>(string key)
        {
            using (IRedisClient client = new RedisClient(conf))
            {
                return client.Get<T>(key);
            }
        }

        public IList<T> GetAll<T>(string key)
        {
            using (IRedisClient client = new RedisClient(conf))
            {
                var keys = client.SearchKeys(key);
                if (keys.Any())
                {
                    IEnumerable<T> dataList = client.GetAll<T>(keys).Values;
                    return dataList.ToList();
                }
                return new List<T>();
            }
        }

        public void Set(string key, object data)
        {
            Set(key, data, DateTime.Now.AddMinutes(CacheExtensions.DefaultCacheTimeMinutes));
        }

        public void Set(string key, object data, DateTime time)
        {
            using (IRedisClient client = new RedisClient(conf))
            {
                var dataSerialize = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });
                client.Set(key, Encoding.UTF8.GetBytes(dataSerialize), time);
            }
        }

        public void SetAll<T>(IDictionary<string, T> values)
        {
            using (IRedisClient client = new RedisClient(conf))
            {
                client.SetAll(values);
            }
        }

        public int Count(string key)
        {
            using (IRedisClient client = new RedisClient(conf))
            {
                return client.SearchKeys(key).Where(s => s.Contains(":") && s.Contains("Mobile-RefreshToken")).ToList().Count;
            }
        }

        public bool IsSet(string key)
        {
            using (IRedisClient client = new RedisClient(conf))
            {
                return client.ContainsKey(key);
            }
        }

        public void Remove(string key)
        {
            using (IRedisClient client = new RedisClient(conf))
            {
                client.Remove(key);
            }
        }

        public void RemoveByPattern(string pattern)
        {
            using (IRedisClient client = new RedisClient(conf))
            {
                var keys = client.SearchKeys(pattern);
                client.RemoveAll(keys);
            }
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        //public string GetTokenKey(int userId, bool isMobile, bool isRefreshToken, string unqDeviceId)
        //{
        //    if (!string.IsNullOrEmpty(unqDeviceId))
        //        unqDeviceId = unqDeviceId.ToUpper();

        //    //Mobile Demek
        //    if (!string.IsNullOrEmpty(unqDeviceId))
        //    {
        //        //Mobile RefreshToken
        //        if (isRefreshToken)
        //            return $"{userId}:{unqDeviceId}-Mobile-RefreshToken";

        //        //Mobile Token. Kontrol amaçlı koyduk(Web Buraya düşmez. Çünkü unqDeviceId boş gelmez.)
        //        return isMobile ? $"{userId}:{unqDeviceId}-Mobile-Token" : $"{userId}:Token";
        //    }

        //    //Web RefreshToken
        //    if (isRefreshToken)
        //        return $"{userId}:RefreshToken";

        //    //Web Token. Kontrol amaçlı koyduk(Mobile buraya düşmez Çünkü unqDeviceId boş gelir.)
        //    return isMobile ? $"{userId}:Mobile-Token" : $"{userId}:Token";
        //}
        public string GetTokenKey(int userId, bool isMobile, bool isRefreshToken, string unqDeviceId)
        {
            if (!string.IsNullOrEmpty(unqDeviceId))
                unqDeviceId = unqDeviceId.ToUpper();

            //Mobile Demek
            if (isMobile)
            {
                if (!string.IsNullOrEmpty(unqDeviceId))
                {
                    //Mobile RefreshToken With unqDeviceId
                    if (isRefreshToken)
                        return $"{userId}:{unqDeviceId}-Mobile-RefreshToken";

                    return $"{userId}:{unqDeviceId}-Mobile-Token";
                }
                else
                {
                    //Mobile RefreshToken Without unqDeviceId
                    if (isRefreshToken)
                        return $"{userId}:Mobile-RefreshToken";

                    return $"{userId}:Mobile-Token";
                }
            }
            else //Web Demek
            {
                //Web RefreshToken
                if (isRefreshToken)
                    return $"{userId}:RefreshToken";

                //Web Token.
                return $"{userId}:Token";
            }
        }
    }
}
