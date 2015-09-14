using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Linq;
namespace CacheMemoryMeasureLab
{
    /// <summary>
    /// Represents a MemoryCacheCache
    /// </summary>
    public partial class RedisCacheManager
    {
        Lazy<ConnectionMultiplexer> redisInstance = new Lazy<ConnectionMultiplexer>(() =>
        {
            var configurationOptions = new ConfigurationOptions
            {
                EndPoints =
    {
        { "127.0.0.1", 6379 }
    },
                KeepAlive = 180,
                AllowAdmin = true
            };
            return ConnectionMultiplexer.Connect(configurationOptions);

        });
        protected ConnectionMultiplexer RedisInstance
        {
            get
            {
                return redisInstance.Value;
            }
        }

        //   IDatabase _cache;
        public IDatabase Cache
        {
            get
            {
                return this.RedisInstance.GetDatabase();
                //if (_cache == null)
                //{
                //    _cache = this.RedisInstance.GetDatabase();
                //}
                //return _cache;
            }
        }

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
            var cache = this.Cache;
            foreach (var key in GetAllKeys())
                if (regex.IsMatch(key))
                {
                    if (removingItemCallBack != null)
                        removingItemCallBack(key, Cache.Get(key));
                    cache.KeyDelete(key);
                }



        }

        public List<RedisKey> GetAllKeys(RedisValue pattern = default(RedisValue))
        {
            var server = this.RedisInstance.GetServer(RedisInstance.GetEndPoints().First());
            return server.Keys(pattern: pattern).ToList();
        }

        /// <summary>
        /// Clear all cache data
        /// </summary>
        public virtual void Clear()
        {
            var cache = this.Cache;
            foreach (var key in GetAllKeys())
                cache.KeyDelete(key);
        }


        public void Clear(Action<string, object> removingItemCallBack)
        {
            var cache = this.Cache;
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

    public static class SampleStackExchangeRedisExtensions
    {
        public static T Get<T>(this IDatabase cache, string key)
        {
            return Deserialize<T>(cache.StringGet(key));
        }

        public static object Get(this IDatabase cache, string key)
        {
            return Deserialize<object>(cache.StringGet(key));
        }

        public static T GetProtobuf<T>(this IDatabase cache, string key)
        {
            return DeserializeProtobuf<T>(cache.StringGet(key));
        }

        public static void Set(this IDatabase cache, string key, object value, TimeSpan expiry)
        {
            cache.StringSet(key, Serialize(value), expiry: expiry);
        }

        public static void SetProtobuf(this IDatabase cache, string key, object value, TimeSpan expiry)
        {
            cache.StringSet(key, SerializeProtobuf(value), expiry: expiry);
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
            if (stream == null)
            {
                return default(T);
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(stream))
            {
                T result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
        }

        static byte[] SerializeProtobuf(object o)
        {
            if (o == null)
            {
                return null;
            }
            using (MemoryStream memoryStream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(memoryStream, o);
                byte[] objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
        }

        static T DeserializeProtobuf<T>(byte[] stream)
        {
            if (stream == null)
            {
                return default(T);
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(stream))
            {
                T result = ProtoBuf.Serializer.Deserialize<T>(memoryStream);
                return result;
            }
        }
    }
}
