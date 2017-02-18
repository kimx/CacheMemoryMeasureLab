using CacheMemoryMeasureLab.Web.Caching;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Web;

namespace CacheMemoryMeasureLab.Web
{
    /// <summary>
    /// 無法連線處理
    ///     get 丟出例外,mvc收到後處理通知機制,不對使用者報錯
    ///     set及remove等 使用FireAndForget
    /// FireAndForget =呼叫了就不管-->無法寫入時不會發生例外,只適用Set的使用,用在Get會傳null
    /// </summary>
    public class RedisCacheManager : ICacheManager
    {
        public int CurrentCacheTime
        {
            get; set;
        }

        ExecuteRetryer Retryer;
        private static Lazy<ConnectionMultiplexer> redisInstance = new Lazy<ConnectionMultiplexer>(() =>
        {
            //        var configurationOptions = new ConfigurationOptions
            //        {
            //            EndPoints =
            //{
            //    { "127.0.0.1", 6379 }
            //},
            //            KeepAlive = 180,
            //            Ssl = false,
            //            AllowAdmin = true,
            //            AbortOnConnectFail = false,
            //            SyncTimeout = int.MaxValue,
            //            ConnectTimeout = int.MaxValue,
            //            ConnectRetry = 5,
            //        };
            return ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["RedisConfiguration"]);

        });
        internal ConnectionMultiplexer RedisInstance
        {
            get
            {
                return redisInstance.Value;
            }
        }

        public RedisCacheManager()
        {
            Cache = RedisInstance.GetDatabase();
            Retryer = new ExecuteRetryer(3, 10);
        }

        protected IDatabase Cache
        {
            get;
            set;
        }


        public virtual T Get<T>(string key)
        {
            return Retryer.Execute(() =>
            {
                return Cache.Get<T>(key);
            });

        }

        public virtual void Set(string key, object data, int cacheTime)
        {
            if (data == null)
                return;
            Cache.Set(key, data, TimeSpan.FromMinutes(cacheTime));
        }

        public virtual bool IsSet(string key)
        {
            try
            {
                return Retryer.Execute(() =>
                {
                    return Cache.KeyExists(key);
                });
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="key">/key</param>
        public virtual void Remove(string key)
        {
            Cache.KeyDelete(key, CommandFlags.FireAndForget);
        }

        /// <summary>
        /// Removes items by pattern
        /// </summary>
        /// <param name="pattern">pattern</param>
        public virtual void RemoveByPattern(string pattern)
        {
            RemoveByPattern(pattern, null);
        }

        public void RemoveByPattern(string pattern, Action<string, object> removingItemCallBack)
        {
            var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var keysToRemove = new List<String>();
            foreach (var key in GetAllKeys())
            {
                if (regex.IsMatch(key))
                {
                    if (removingItemCallBack != null)
                        removingItemCallBack(key, Cache.Get(key));
                    Remove(key);
                }
            }
        }

        private List<RedisKey> GetAllKeys(RedisValue pattern = default(RedisValue))
        {
            List<RedisKey> result = new List<RedisKey>();
            try
            {
                return Retryer.Execute(() =>
                {
                    //var server = this.RedisInstance.GetServer(RedisInstance.GetEndPoints().First());
                    //return server.Keys(pattern: pattern).ToList();
                    foreach (var ep in RedisInstance.GetEndPoints())
                    {
                        var server = RedisInstance.GetServer(ep);
                        var keys = server.Keys(pattern: pattern);
                        foreach (var key in keys)
                            result.Add(key);
                    }
                    return result;
                });
            }
            catch//不處理,讓Get的Exception處理即可
            {
                return result;
            }
        }

        /// <summary>
        /// Clear all cache data
        /// </summary>
        public virtual void Clear()
        {
            Clear(null);
        }


        public void Clear(Action<string, object> removingItemCallBack)
        {
            foreach (var key in GetAllKeys())
            {
                if (removingItemCallBack != null)
                    removingItemCallBack(key, Cache.Get(key));
                Remove(key);
            }
        }



    }

    /// <summary>
    /// Inspire from nopCommerce
    /// 此實作的重點減少在於同一Request多次叫用同一個key的快取
    /// </summary>
    public class RedisPerRequestCacheManager : RedisCacheManager
    {
        private readonly ICacheManager _perRequestCacheManager;
        public RedisPerRequestCacheManager()
        {
            _perRequestCacheManager = new PerRequestCacheManager();
        }
        public override T Get<T>(string key)
        {
            if (_perRequestCacheManager.IsSet(key))//Avoiding too many connecting per request
                return _perRequestCacheManager.Get<T>(key);
            var result = base.Get<T>(key);
            _perRequestCacheManager.Set(key, result, 0);//Avoiding too many connecting per request
            return result;
        }

        public override bool IsSet(string key)
        {
            if (_perRequestCacheManager.IsSet(key))//Avoiding too many connecting per request
                return true;
            return base.IsSet(key);
        }

        //改良用,加入此,第二取得才取得perRequest,否則要第三次
        public override void Set(string key, object data, int cacheTime)
        {
            _perRequestCacheManager.Set(key, data, 0);
            base.Set(key, data, cacheTime);
        }
    }

    /// <summary>
    /// Origin=SampleStackExchangeRedisExtensions
    /// </summary>
    internal static class RedisStackExchangeRedisExtensions
    {
        public static T Get<T>(this IDatabase cache, string key)
        {
            return BinaryHelper.Deserialize<T>(cache.StringGet(key));
        }

        public static object Get(this IDatabase cache, string key)
        {
            return BinaryHelper.Deserialize<object>(cache.StringGet(key));
        }

        public static void Set(this IDatabase cache, string key, object value, TimeSpan expiry)
        {
            cache.StringSet(key, BinaryHelper.Serialize(value), expiry: expiry, flags: CommandFlags.FireAndForget);
        }


    }
}