using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CacheMemoryMeasureLab.Web
{

    public static class CacheExtensions
    {
        public const int DefaultCacheTime = 1440;//24 hr

        public static T Get<T>(this ICacheManager cacheManager, string key, Func<T> acquire)
        {
            return Get(cacheManager, key, DefaultCacheTime, acquire);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheManager"></param>
        /// <param name="key"></param>
        /// <param name="cacheTime">minute</param>
        /// <param name="acquire"></param>
        /// <returns></returns>
        public static T Get<T>(this ICacheManager cacheManager, string key, int cacheTime, Func<T> acquire)
        {
            if (cacheManager.IsSet(key))
            {
                try
                {
                    cacheManager.CurrentCacheTime = cacheTime;
                    return cacheManager.Get<T>(key);
                }
                catch (Exception ex)//針對使用Redis Cache所作的錯誤處理機制
                {
                    //1.Notify Exception Process

                    //2.Keep Server Normal
                    return acquire(); ;
                }
            }
            else
            {
                var result = acquire();
                cacheManager.Set(key, result, cacheTime);
                return result;
            }
        }

        /// <summary>
        /// For PubSub及Premise同步用法，Client不要直接叫用Set 要使用此方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheManager"></param>
        /// <param name="result"></param>
        /// <param name="key"></param>
        /// <param name="cacheTime"></param>
        public static void Update<T>(this ICacheManager cacheManager, string key, T result, int cacheTime)
        {
            cacheManager.Remove(key);
            cacheManager.Set(key, result, cacheTime);
        }
    }
}