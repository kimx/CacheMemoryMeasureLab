using CacheMemoryMeasureLab.Web.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace CacheMemoryMeasureLab.Web
{
    /// <summary>
    /// 實作本地+Redis同步及通知機制
    /// 結果:ok
    /// 1.取Memory
    /// 2.取不到在取Redis
    /// 此方法目前多台site會不同步，因此patten若設了Set 也要publish會連本地都通知,若要連Set也要同步,
    /// 此Pattern並不適用，因為多台主機可能同時會互相通知到,造成無窮迴圈
    /// 重點提醒:只能用Remove通知其他台清除,並作更新動作
    /// </summary>
    public class PubSubMemoryRedisCacheManager : ICacheManager
    {
        public int CurrentCacheTime
        {
            get; set;
        }

        RedisPerRequestCacheManager RedisCache;
        MemoryCacheManager MemoryCache;
        static ISubscriber sub = null;
        public PubSubMemoryRedisCacheManager()
        {
            this.RedisCache = new RedisPerRequestCacheManager();
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
                    else if (name == "RemoveByPattern")
                        MemoryCache.RemoveByPattern(key);
                    //只需要移除本地就好，因 1.通知方會移除Redis,2.被通知方只要移除本地後,在邏輯方面會自動判斷從資料庫或Redis取得
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
            RedisCache.Clear();
            NotifyRemoved();
        }

        public void Clear(Action<string, object> removingItemCallBack)
        {
            MemoryCache.Clear(removingItemCallBack);
            RedisCache.Clear();
            NotifyRemoved();

        }

        public T Get<T>(string key)
        {
            if (MemoryCache.IsSet(key))
                return MemoryCache.Get<T>(key);
            var data = RedisCache.Get<T>(key); ;
            MemoryCache.Set(key, data, CurrentCacheTime);
            return data;
        }

        public bool IsSet(string key)
        {
            if (MemoryCache.IsSet(key))
                return true;
            return this.RedisCache.IsSet(key);
        }

        public void Remove(string key)
        {
            MemoryCache.Remove(key);
            RedisCache.Remove(key);
            NotifyRemoved(key);

        }

        public void RemoveByPattern(string pattern)
        {
            MemoryCache.RemoveByPattern(pattern);
            RedisCache.RemoveByPattern(pattern);
            NotifyRemoved(pattern);

        }

        public void RemoveByPattern(string pattern, Action<string, object> removingItemCallBack)
        {
            MemoryCache.RemoveByPattern(pattern, removingItemCallBack);
            RedisCache.RemoveByPattern(pattern);
            NotifyRemoved(pattern);

        }

        public void Set(string key, object data, int cacheTime)
        {
            MemoryCache.Set(key, data, cacheTime);
            RedisCache.Set(key, data, cacheTime);

        }


    }
}