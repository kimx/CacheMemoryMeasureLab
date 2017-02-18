using CacheMemoryMeasureLab.Web.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace CacheMemoryMeasureLab.Web
{
    public class PubSubMemoryCacheManager : ICacheManager
    {
        public static int SubscribeNoticeCount = 0;
        public static int SubscribePid = 0;
        RedisCacheManager RedisCache;
        MemoryCacheManager MemoryCache;
        static ISubscriber sub = null;
        public PubSubMemoryCacheManager()
        {
            this.RedisCache = new RedisCacheManager();
            this.MemoryCache = new MemoryCacheManager();
            //這個要static 否則會被多次通知
            if (sub == null)
            {
                sub = this.RedisCache.RedisInstance.GetSubscriber();
                sub.Subscribe("NotifyRemoved", (channel, message) =>
                {
                    var args = message.ToString().Split(new string[] { "##" }, StringSplitOptions.None);
                    var name = args[0];
                    var key = args[1];
                    if (name == "Clear")
                        MemoryCache.Clear();
                    else if (name == "Remove")
                        MemoryCache.Remove(key);
                    else if (name == "RemoveByPattern")
                        MemoryCache.RemoveByPattern(key);
                    SubscribeNoticeCount += 1;
                    SubscribePid = System.Diagnostics.Process.GetCurrentProcess().Id;
                });
            }
        }

        private void NotifyRemoved(string key = "", [CallerMemberName]string caller = "")
        {
            sub.Publish("NotifyRemoved", $"{caller}##{key}");
        }

        public void Clear()
        {
            MemoryCache.Clear();
            NotifyRemoved();
        }

        public void Clear(Action<string, object> removingItemCallBack)
        {
            MemoryCache.Clear(removingItemCallBack);
            NotifyRemoved();

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
            NotifyRemoved(key);

        }

        public void RemoveByPattern(string pattern)
        {
            MemoryCache.RemoveByPattern(pattern);
            NotifyRemoved(pattern);

        }

        public void RemoveByPattern(string pattern, Action<string, object> removingItemCallBack)
        {
            MemoryCache.RemoveByPattern(pattern, removingItemCallBack);
            NotifyRemoved(pattern);

        }

        public void Set(string key, object data, int cacheTime)
        {
            MemoryCache.Set(key, data, cacheTime);

        }


    }
}