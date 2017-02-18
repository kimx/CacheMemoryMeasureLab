using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CacheMemoryMeasureLab.Web.Controllers
{
    public class RedisPerRequestController : Controller
    {
        ICacheManager RedisPerRequestCacheManager;
        public RedisPerRequestController()
        {
            RedisPerRequestCacheManager = new RedisPerRequestCacheManager();
        }


        public ActionResult Index()
        {
            ViewBag.Value = TempData["Value"];
            return View();
        }

        public ActionResult GetValue()
        {
            TempData["Value"] = RedisPerRequestCacheManager.Get<DateTime>("Value", () => DateTime.Now);
            System.Threading.Thread.Sleep(1000);
            TempData["Value"] = RedisPerRequestCacheManager.Get<DateTime>("Value", () => DateTime.Now);
            return RedirectToAction("Index");
        }

        public ActionResult SetValue()
        {
            RedisPerRequestCacheManager.Set("Value", DateTime.Now, 130);
            TempData["Value"] = DateTime.Now;
            return RedirectToAction("Index");
        }
    }
}