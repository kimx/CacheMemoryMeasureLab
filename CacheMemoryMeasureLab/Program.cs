using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
namespace CacheMemoryMeasureLab
{
    /// <summary>
    ///  static int perUserCacheCount = 100;
    ///  static int onlineUsers = 2000;
    /// ======================OnPremise-Begin=================================
    ///CacheName:On-Premise,MemorySizeChangeInKB:0KB
    ///CacheName:On-Premise,MemorySizeChangeInKB:33,647KB
    ///CacheName:On-Premise,Set-Milliseconds:568
    ///CacheName:On-Premise,Get-Milliseconds:131
    ///======================OnPremise-End=================================
    ///======================RedisCacheLab-Begin=================================
    ///CacheName:RedisCache,MemorySizeChangeInKB:0KB
    ///CacheName:RedisCache,MemorySizeChangeInKB:6KB
    ///CacheName:RedisCache,Set-Milliseconds:18011
    ///CacheName:RedisCache,Get-Milliseconds:17202
    ///======================RedisCacheLab-End=================================
    ///======================RedisCacheLab-Begin=================================
    ///CacheName:RedisCacheProtobuf,MemorySizeChangeInKB:0KB
    ///CacheName:RedisCacheProtobuf,MemorySizeChangeInKB:14KB
    ///CacheName:RedisCacheProtobuf,Set-Milliseconds:13815
    ///CacheName:RedisCacheProtobuf,Get-Milliseconds:12794
    ///======================RedisCacheLab-End=================================
    /// </summary>
    class Program
    {
        static int perUserCacheCount = 100;
        static int onlineUsers = 1;
        static RedisCacheManager redisManager = new RedisCacheManager();
        /// <summary>
        /// 100 menu及2000user 約26mb
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
          //  OnPremiseLab();
            redisManager.Clear();
            //RedisCacheLab();
            //  RedisCacheProtobufLab();
            //   SqlCacheLab();
            Console.Read();
        }

        #region Lab Method
        static void OnPremiseLab()
        {
            Console.WriteLine("======================OnPremise-Begin=================================");
            MeasureSetMemoryAndTime("On-Premise", MemoryCache.Default);
            MeasureGetTime("On-Premise", MemoryCache.Default);
            Console.WriteLine("======================OnPremise-End=================================");

        }

        static void RedisCacheLab()
        {
            Console.WriteLine("======================RedisCacheLab-Begin=================================");
            var redisCache = redisManager.Cache;
            MeasureSetMemoryAndTimeByRedis("RedisCache", redisCache);
            MeasureGetTimeByRedis("RedisCache", redisCache);
            Console.WriteLine("======================RedisCacheLab-End=================================");
        }
        static void RedisCacheProtobufLab()
        {

            Console.WriteLine("======================RedisCacheLab-Begin=================================");
            var redisCache = redisManager.Cache;
            MeasureSetMemoryAndTimeByRedisProtobuf("RedisCacheProtobuf", redisCache);
            MeasureGetTimeByRedisProtobuf("RedisCacheProtobuf", redisCache);
            Console.WriteLine("======================RedisCacheLab-End=================================");
        }
        static void SqlCacheLab()
        {
            Console.WriteLine("======================SqlCacheLab-Begin=================================");
            string connStr = "Data Source=KIM-MSI\\KIMSSQLSERVER;Initial Catalog=MVWDataBase;User ID=sa;Password=mis123;MultipleActiveResultSets=True";
            var sqlCache = new SqlCache(connStr);
            MeasureSetMemoryAndTime("SqlCache", sqlCache);
            MeasureGetTime("SqlCache", sqlCache);
            Console.WriteLine("======================SqlCacheLab-End=================================");

        }


        #endregion

        #region MeasureMethod
        /// <summary>
        /// 測量寫入的時間及記憶體
        /// </summary>
        /// <param name="cache"></param>
        private static void MeasureSetMemoryAndTime(string cacheName, ObjectCache cache)
        {
            System.Diagnostics.Stopwatch sh = new System.Diagnostics.Stopwatch();
            sh.Start();
            MemWatch mw = new MemWatch();
            List<UserMenuInfo> menus = GetTestData();
            Console.WriteLine(string.Format("CacheName:{0},MemorySizeChangeInKB:{1}", cacheName, mw.MemorySizeChangeInKB));
            mw.Start();
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddDays(1));
            foreach (var menu in menus)
            {
                cache.Set(menu.PRG_NO, menu, policy);
            }
            mw.Stop();
            Console.WriteLine(string.Format("CacheName:{0},MemorySizeChangeInKB:{1}", cacheName, mw.MemorySizeChangeInKB));
            sh.Stop();
            Console.WriteLine(string.Format("CacheName:{0},Set-Milliseconds:{1}", cacheName, sh.ElapsedMilliseconds));

        }

        /// <summary>
        /// 測量讀取的時間
        /// </summary>
        /// <param name="cache"></param>
        private static void MeasureGetTime(string cacheName, ObjectCache cache)
        {
            System.Diagnostics.Stopwatch sh = new System.Diagnostics.Stopwatch();
            sh.Start();
            foreach (dynamic item in cache)
            {
            }
            sh.Stop();
            Console.WriteLine(string.Format("CacheName:{0},Get-Milliseconds:{1}", cacheName, sh.ElapsedMilliseconds));

        }

        /// <summary>
        /// 測量寫入的時間及記憶體
        /// </summary>
        /// <param name="cache"></param>
        private static void MeasureSetMemoryAndTimeByRedis(string cacheName, IDatabase cache)
        {
            System.Diagnostics.Stopwatch sh = new System.Diagnostics.Stopwatch();

            sh.Start();
            MemWatch mw = new MemWatch(true);
            List<UserMenuInfo> menus = GetTestData();
            Console.WriteLine(string.Format("CacheName:{0},MemorySizeChangeInKB:{1}", cacheName, mw.MemorySizeChangeInKB));
            mw.Start();
            foreach (var menu in menus)
            {
                cache.Set("OP-" + menu.PRG_NO, menu, TimeSpan.FromDays(1));
            }
            mw.Stop();
            Console.WriteLine(string.Format("CacheName:{0},MemorySizeChangeInKB:{1}", cacheName, mw.MemorySizeChangeInKB));
            sh.Stop();
            Console.WriteLine(string.Format("CacheName:{0},Set-Milliseconds:{1}", cacheName, sh.ElapsedMilliseconds));

        }

        /// <summary>
        /// 測量讀取的時間
        /// </summary>
        /// <param name="cache"></param>
        private static void MeasureGetTimeByRedis(string cacheName, IDatabase cache)
        {
            System.Diagnostics.Stopwatch sh = new System.Diagnostics.Stopwatch();
            sh.Start();
            foreach (string key in redisManager.GetAllKeys())
            {
                if (key.StartsWith("OP-"))
                {
                    object t = cache.Get(key);
                }
            }
            sh.Stop();
            Console.WriteLine(string.Format("CacheName:{0},Get-Milliseconds:{1}", cacheName, sh.ElapsedMilliseconds));

        }

        /// <summary>
        /// 測量寫入的時間及記憶體
        /// </summary>
        /// <param name="cache"></param>
        private static void MeasureSetMemoryAndTimeByRedisProtobuf(string cacheName, IDatabase cache)
        {
            System.Diagnostics.Stopwatch sh = new System.Diagnostics.Stopwatch();
            sh.Start();
            MemWatch mw = new MemWatch(true);
            List<UserMenuInfo> menus = GetTestData();
            Console.WriteLine(string.Format("CacheName:{0},MemorySizeChangeInKB:{1}", cacheName, mw.MemorySizeChangeInKB));
            mw.Start();
            foreach (var menu in menus)
            {
                cache.SetProtobuf("PF-" + menu.PRG_NO, menu, TimeSpan.FromDays(1));
            }
            mw.Stop();
            Console.WriteLine(string.Format("CacheName:{0},MemorySizeChangeInKB:{1}", cacheName, mw.MemorySizeChangeInKB));
            sh.Stop();
            Console.WriteLine(string.Format("CacheName:{0},Set-Milliseconds:{1}", cacheName, sh.ElapsedMilliseconds));

        }

        /// <summary>
        /// 測量讀取的時間
        /// </summary>
        /// <param name="cache"></param>
        private static void MeasureGetTimeByRedisProtobuf(string cacheName, IDatabase cache)
        {
            System.Diagnostics.Stopwatch sh = new System.Diagnostics.Stopwatch();
            sh.Start();
            foreach (string key in redisManager.GetAllKeys())
            {
                if (key.StartsWith("PF-"))
                {
                    UserMenuInfo t = cache.GetProtobuf<UserMenuInfo>(key);
                }
            }
            sh.Stop();
            Console.WriteLine(string.Format("CacheName:{0},Get-Milliseconds:{1}", cacheName, sh.ElapsedMilliseconds));

        }


        private static List<UserMenuInfo> GetTestData()
        {
            List<UserMenuInfo> menus = new List<UserMenuInfo>();
            for (int i = 0; i < perUserCacheCount * onlineUsers; i++)
            {
                menus.Add(new UserMenuInfo { PRG_NO = i.ToString("00000000"), PRG_AREA = "SysCore", PRG_NAME = "Amend Purchase Order", UP_PRGNO = "1000000" });
            }

            return menus;
        }
        #endregion


    }

    class MemWatch
    {
        //比較記憶體使用量變化的基準值
        private long _lastTotalMemory = 0;
        //記憶體使用量變化
        public long MemorySizeChange = 0;
        //是否強制GC再測量記憶體用量
        private bool _forceGC = false;
        //可指定測量前是否要先做GC
        //(可排除己不用但尚未回收的記憶體)
        public MemWatch(bool forceGC)
        {
            _forceGC = forceGC;
        }
        public MemWatch() : this(false) { }
        //保留測量開始之基準
        public void Start()
        {
            _lastTotalMemory =
                GC.GetTotalMemory(_forceGC);
        }
        //測量從Start()至今的記憶體變化
        public void Stop()
        {
            MemorySizeChange =
                 GC.GetTotalMemory(_forceGC) - _lastTotalMemory;
        }
        //記憶體使用量變化(以KB計)
        public string MemorySizeChangeInKB
        {
            get
            {
                return string.Format("{0:N0}KB",
                    MemorySizeChange / 1024);
            }
        }
        //記憶體使用量變化(以MB計)
        public string MemorySizeChangeInMB
        {
            get
            {
                return string.Format("{0:N0}MB",
                    MemorySizeChange / 1024 / 1024);
            }
        }
    }


}
