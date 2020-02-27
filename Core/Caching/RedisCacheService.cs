using Core.Configuration;
using Core.CustomException;
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
            try
            {
                using (IRedisClient client = new RedisClient(conf))
                {
                    return client.Get<T>(key);
                }
            }
            catch (Exception ex)
            {
                throw new RedisNotAvailableException();
                //return default;
            }
        }

        public IList<T> GetAll<T>(string key)
        {
            try
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
            catch
            {
                throw new RedisNotAvailableException();
                //return default;
            }
        }

        public void Set(string key, object data)
        {
            Set(key, data, DateTime.Now.AddMinutes(CacheExtensions.DefaultCacheTimeMinutes));
        }

        public void Set(string key, object data, DateTime time)
        {
            try
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
            catch (Exception ex)
            {
                throw new RedisNotAvailableException();
            }
        }

        public void SetAll<T>(IDictionary<string, T> values)
        {
            try
            {
                using (IRedisClient client = new RedisClient(conf))
                {
                    client.SetAll(values);
                }
            }
            catch
            {
                throw new RedisNotAvailableException();
            }
        }

        public int Count(string key)
        {
            try
            {
                using (IRedisClient client = new RedisClient(conf))
                {
                    return client.SearchKeys(key).Where(s => s.Contains(":") && s.Contains("Mobile-RefreshToken")).ToList().Count;
                }
            }
            catch
            {
                throw new RedisNotAvailableException();
                //return 0;
            }
        }

        public bool IsSet(string key)
        {
            try
            {
                using (IRedisClient client = new RedisClient(conf))
                {
                    return client.ContainsKey(key);
                }
            }
            catch
            {
                throw new RedisNotAvailableException();
                //return false;
            }
        }

        public void Remove(string key)
        {
            try
            {
                using (IRedisClient client = new RedisClient(conf))
                {
                    client.Remove(key);
                }
            }
            catch
            {
                throw new RedisNotAvailableException();
            }
        }

        public void RemoveByPattern(string pattern)
        {
            try
            {
                using (IRedisClient client = new RedisClient(conf))
                {
                    var keys = client.SearchKeys(pattern);
                    client.RemoveAll(keys);
                }
            }
            catch
            {
                throw new RedisNotAvailableException();
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

        public string GetTokenKeyForBeHalfOf(int userId)
        {
            string password = $"{CreatePassword(8)}@{userId}";
            return $"{userId}:BeHalfOfToken:{password}";
        }

        public string GetTokenKeyForBeHalfOf(int userId, string password)
        {
            return $"{userId}:BeHalfOfToken:{password}";
        }

        public string GetKeyWithBeHalfOfPassword(string beHalfofPassword, out string beHalfofUserId)
        {
            if (!string.IsNullOrEmpty(beHalfofPassword) && beHalfofPassword.Split('@').Length > 1)
            {
                beHalfofUserId = beHalfofPassword.Split('@')[1];
                return $"{beHalfofUserId}:BeHalfOfToken:{beHalfofPassword}";
            }
            else
            {
                beHalfofUserId = string.Empty;
                return string.Empty;
            }
        }

        public string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
    }
}
