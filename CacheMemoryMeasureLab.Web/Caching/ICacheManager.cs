using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CacheMemoryMeasureLab.Web
{
    /// <summary>
    /// 在整個CacheManager的實作Pattern主要為以Get為主，取不到才自己作Set,
    /// 此Pattern主要要注意的是在PubSubMemoryCacheManager及PubSubMemoryRedisCacheManager的的架構下
    /// 對於Cache資料的異動，不能用Set，要透過Remove的方式讓多台主機同步
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// For在Get時需要更新Cache用，目前用在PubSubMemoryRedisCacheManager
        /// </summary>
        int CurrentCacheTime { get; set; }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key.</returns>
        T Get<T>(string key);

        /// <summary>
        /// Adds the specified key and object to the cache.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="data">Data</param>
        /// <param name="cacheTime">Cache time</param>
        void Set(string key, object data, int cacheTime);


        /// <summary>
        /// Gets a value indicating whether the value associated with the specified key is cached
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>Result</returns>
        bool IsSet(string key);

        /// <summary>
        /// Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="key">/key</param>
        void Remove(string key);

        /// <summary>
        /// Removes items by pattern
        /// </summary>
        /// <param name="pattern">pattern</param>
        void RemoveByPattern(string pattern);

        void RemoveByPattern(string pattern, Action<string, object> removingItemCallBack);

        /// <summary>
        /// Clear all cache data
        /// </summary>
        void Clear();

        /// <summary>
        /// Clear all cache data
        /// </summary>
        void Clear(Action<string, object> removingItemCallBack);

        ///// <summary>
        ///// 用來在取值後,需要馬上Set回Cache的方法
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key"></param>
        ///// <param name="cacheTime"></param>
        ///// <returns></returns>
        //T Get<T>(string key, int cacheTime);
    }

}