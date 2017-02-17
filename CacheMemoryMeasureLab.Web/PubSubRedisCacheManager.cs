using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CacheMemoryMeasureLab.Web
{
    public class PubSubRedisCacheManager : ICacheManager
    {
        public static int SubscribeNoticeCount = 0;
        public static int SubscribePid = 0;
        RedisCacheManager RedisCache;
        MemoryCacheManager MemoryCache;
        static ISubscriber sub = null;
        public PubSubRedisCacheManager()
        {
            this.RedisCache = new RedisCacheManager();
            this.MemoryCache = new MemoryCacheManager();
            //這個要改成static 否則會被多次通知
            if (sub == null)
            {
                sub = this.RedisCache.RedisInstance.GetSubscriber();
                sub.Subscribe("removeStamp", (channel, message) =>
                {
                    MemoryCache.Clear();
                    SubscribeNoticeCount += 1;
                    SubscribePid = System.Diagnostics.Process.GetCurrentProcess().Id;
                });
            }
        }


        public void Clear()
        {
            sub.Publish("removeStamp", "hello");
            MemoryCache.Clear();
        }

        public void Clear(Action<string, object> removingItemCallBack)
        {
            MemoryCache.Clear(removingItemCallBack);
        }

        public T Get<T>(string key)
        {
            return MemoryCache.Get<T>(key);
        }

        public bool IsSet(string key)
        {
            return this.MemoryCache.IsSet(key);
        }

        public void Remove(string key)
        {
            MemoryCache.Remove(key);
        }

        public void RemoveByPattern(string pattern)
        {
            MemoryCache.RemoveByPattern(pattern);
        }

        public void RemoveByPattern(string pattern, Action<string, object> removingItemCallBack)
        {
            MemoryCache.RemoveByPattern(pattern, removingItemCallBack);
        }

        public void Set(string key, object data, int cacheTime)
        {
            MemoryCache.Set(key, data, cacheTime);

        }


    }
}