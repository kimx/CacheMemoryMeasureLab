﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
namespace CacheMemoryMeasureLab
{
    class Program
    {
        /// <summary>
        /// 100 menu及2000user 約26mb
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            MeasureMemory(MemoryCache.Default);
            MonitorCache();
            //GetCacheMeasure(MemoryCache.Default);

            //MeasureMemoryBySql();
            Console.Read();
        }

        private static void MeasureMemoryBySql()
        {
            string connStr = "Data Source=KIM-MSI\\KIMSSQLSERVER;Initial Catalog=MVWDataBase;User ID=sa;Password=mis123;MultipleActiveResultSets=True";
            var sqlCache = new SqlCache(connStr);
            MeasureMemory(sqlCache);
            GetCacheMeasure(sqlCache);


        }

        private static void MonitorCache()
        {
            while (true)
            {
                Console.WriteLine(MemoryCacheMonitor.IsInMaintenanceMode());
                System.Threading.Thread.Sleep(1000);
            }
        }

        private static void MeasureMemory(ObjectCache cache)
        {
            System.Diagnostics.Stopwatch sh = new System.Diagnostics.Stopwatch();
            sh.Start();
            int perUserCacheCount = 100;
            int onlineUsers = 20;
            MemWatch mw = new MemWatch();

            List<UserMenuInfo> menus = new List<UserMenuInfo>();
            for (int i = 0; i < perUserCacheCount * onlineUsers; i++)
            {
                menus.Add(new UserMenuInfo { PRG_NO = i.ToString("00000"), PRG_AREA = "SysCore", PRG_NAME = "Amend Purchase Order", UP_PRGNO = "1000000" });
            }
            Console.WriteLine(mw.MemorySizeChangeInKB);
            mw.Start();
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddDays(1));
            foreach (var menu in menus)
            {
                cache.Set(menu.PRG_NO, menu, policy);
            }
            mw.Stop();
            Console.WriteLine(mw.MemorySizeChangeInKB);
            sh.Stop();
            Console.WriteLine("sh.Elapsed.Milliseconds:" + sh.Elapsed.Milliseconds);
        }

        private static void GetCacheMeasure(ObjectCache cache)
        {
            System.Diagnostics.Stopwatch sh = new System.Diagnostics.Stopwatch();
            sh.Start();
            foreach (dynamic item in cache)
            {

            }
            sh.Stop();
            Console.WriteLine("GetCacheMeasure-Milliseconds:" + sh.Elapsed.Milliseconds);
        }
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
