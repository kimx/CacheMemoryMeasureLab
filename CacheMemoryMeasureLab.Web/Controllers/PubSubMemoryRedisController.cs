using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CacheMemoryMeasureLab.Web.Controllers
{
    public class PubSubMemoryRedisController : Controller
    {
        ICacheManager PubSubMemoryRedisCacheManager;
        public PubSubMemoryRedisController()
        {
            PubSubMemoryRedisCacheManager = new PubSubMemoryRedisCacheManager();
        }


        public ActionResult Index()
        {
            ViewBag.Value = TempData["Value"];
            return View();
        }

        public ActionResult GetValue()
        {
            TempData["Value"] = PubSubMemoryRedisCacheManager.Get<DateTime>("PubSubMemoryRedis", () => DateTime.Now);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// 此方法目前多台site會不同步，因此patten若設了Set 也要publish會連本地都通知,若要連Set也要同步,
        /// 此Pattern並不適用，因為多台主機可能同時會互相通知到,造成無窮迴圈
        /// 重點提醒:只能用Remove通知其他台清除,並作更新動作
        /// </summary>
        /// <returns></returns>
        public ActionResult SetValue()
        {
            PubSubMemoryRedisCacheManager.Set("PubSubMemoryRedis", DateTime.Now, 130);
            TempData["Value"] = DateTime.Now;
            return RedirectToAction("Index");
        }

        public ActionResult Clear()
        {
            PubSubMemoryRedisCacheManager.Clear();
            return RedirectToAction("Index");
        }
    }
}