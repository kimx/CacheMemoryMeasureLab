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
    public class RedisCacheManager : ICacheManager
    {
        public static string BeginConn;
        public static string EndConn;
        static Lazy<ConnectionMultiplexer> redisInstance = new Lazy<ConnectionMultiplexer>(() =>
         {
             BeginConn = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss fff");
             var instance = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["RedisConfiguration"]);
             EndConn = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss fff");
             return instance;

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
        }

        protected IDatabase Cache
        {
            get;
            set;
        }

        //static IDatabase _cache;
        //static IDatabase Cache
        //{
        //    get
        //    {
        //        if (_cache == null)
        //            _cache = RedisInstance.GetDatabase();
        //        return _cache;

        //    }
        //}

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key.</returns>
        public virtual T Get<T>(string key)
        {
            return Cache.Get<T>(key);
        }

        /// <summary>
        /// Adds the specified key and object to the cache.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="data">Data</param>
        /// <param name="cacheTime">Cache time Minutes</param>
        public virtual void Set(string key, object data, int cacheTime)
        {
            if (data == null)
                return;
            Cache.Set(key, data, TimeSpan.FromMinutes(cacheTime));
        }

        /// <summary>
        /// Gets a value indicating whether the value associated with the specified key is cached
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>Result</returns>
        public virtual bool IsSet(string key)
        {
            return Cache.KeyExists(key);
        }

        /// <summary>
        /// Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="key">/key</param>
        public virtual void Remove(string key)
        {
            Cache.KeyDelete(key);
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
            var cache = Cache;
            foreach (var key in GetAllKeys())
                if (regex.IsMatch(key))
                {
                    if (removingItemCallBack != null)
                        removingItemCallBack(key, Cache.Get(key));
                    cache.KeyDelete(key);
                }



        }

        private List<RedisKey> GetAllKeys(RedisValue pattern = default(RedisValue))
        {
            var server = RedisInstance.GetServer(RedisInstance.GetEndPoints().First());
            return server.Keys(pattern: pattern).ToList();
        }

        /// <summary>
        /// Clear all cache data
        /// </summary>
        public virtual void Clear()
        {
            var cache = Cache;
            foreach (var key in GetAllKeys())
                cache.KeyDelete(key);
        }


        public void Clear(Action<string, object> removingItemCallBack)
        {
            var cache = Cache;
            foreach (var key in GetAllKeys())
            {
                if (removingItemCallBack != null)
                {
                    removingItemCallBack(key, cache.Get(key));
                }
                cache.KeyDelete(key);
            }
        }



    }

    /// <summary>
    /// Origin=SampleStackExchangeRedisExtensions
    /// </summary>
    internal static class RedisStackExchangeRedisExtensions
    {
        public static T Get<T>(this IDatabase cache, string key)
        {
            return Deserialize<T>(cache.StringGet(key));
        }

        public static object Get(this IDatabase cache, string key)
        {
            return Deserialize<object>(cache.StringGet(key));
        }

        public static void Set(this IDatabase cache, string key, object value, TimeSpan expiry)
        {
            cache.StringSet(key, Serialize(value), expiry: expiry, flags: CommandFlags.FireAndForget);
        }

        static byte[] Serialize(object o)
        {
            if (o == null)
            {
                return null;
            }
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, o);
                byte[] objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
        }

        static T Deserialize<T>(byte[] stream)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            if (stream == null)
                return default(T);

            using (MemoryStream memoryStream = new MemoryStream(stream))
            {
                T result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
        }

    }
}