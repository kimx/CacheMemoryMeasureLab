using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CacheMemoryMeasureLab.Web
{
    /// <summary>
    /// 第一版原型,使用timestamp,ps:不使用,因為更好的機制為PubSub
    /// </summary>
    public class PremiseRedisCacheManager : ICacheManager
    {
        RedisCacheManager RedisCache;
        MemoryCacheManager MemoryCache;

        public int CurrentCacheTime
        {
            get; set;
        }

        public PremiseRedisCacheManager()
        {
            this.RedisCache = new RedisCacheManager();
            this.MemoryCache = new MemoryCacheManager();
        }
        public void Clear()
        {
            MemoryCache.Clear();
            RedisCache.Clear();
        }

        public void Clear(Action<string, object> removingItemCallBack)
        {
            MemoryCache.Clear(removingItemCallBack);
            RedisCache.Clear();
        }

        public T Get<T>(string key)
        {
            return MemoryCache.Get<T>(key);
        }

        public bool IsSet(string key)
        {
            if (!this.MemoryCache.IsSet(key))
                return false;
            string stampKey = GetDateStampKey(key);
            DateTime redisStamp = RedisCache.Get<DateTime>(stampKey);
            DateTime memoryStamp = MemoryCache.Get<DateTime>(stampKey);
            return redisStamp == memoryStamp;
        }

        public void Remove(string key)
        {
            string stampKey = GetDateStampKey(key);
            RedisCache.Remove(stampKey);
            MemoryCache.Remove(stampKey);
            MemoryCache.Remove(key);
        }

        public void RemoveByPattern(string pattern)
        {
            MemoryCache.RemoveByPattern(pattern);
            RedisCache.RemoveByPattern(pattern);
        }

        public void RemoveByPattern(string pattern, Action<string, object> removingItemCallBack)
        {
            MemoryCache.RemoveByPattern(pattern, removingItemCallBack);
            RedisCache.RemoveByPattern(pattern);
        }

        public void Set(string key, object data, int cacheTime)
        {
            string stampKey = GetDateStampKey(key);

            MemoryCache.Set(key, data, cacheTime);
            DateTime dateStamp = RedisCache.Get<DateTime>(stampKey);//一定要從共用來取得stamp，否則多台site會各自設定此值,造成判斷錯誤,同步要靠remove
            if (dateStamp == DateTime.MinValue)
                dateStamp = DateTime.UtcNow;
            RedisCache.Set(stampKey, dateStamp, cacheTime);
            MemoryCache.Set(stampKey, dateStamp, cacheTime);
        }

        private string GetDateStampKey(string key)
        {
            string newKey = key;
            if (newKey.EndsWith("."))
                newKey = newKey + "ScDateStamp";
            else
                newKey = newKey + ".ScDateStamp";
            return newKey;

        }
    }
}