using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CacheMemoryMeasureLab.Web.Controllers
{
    public class RedisLabController : Controller
    {
        ICacheManager RedisCacheManager;
        ICacheManager PremiseRedisCacheManager;
        PubSubMemoryCacheManager PubSubMemoryCacheManager;
        string dbPath = "";
        public RedisLabController()
        {
            RedisCacheManager = new RedisCacheManager();
            PremiseRedisCacheManager = new PremiseRedisCacheManager();
            PubSubMemoryCacheManager = new PubSubMemoryCacheManager();
        }

        // GET: RedisLab
        public ActionResult Index()
        {
            ViewBag.Value = TempData["Value"];
            return View();
        }

        public ActionResult GetValue()
        {
            TempData["Value"] = RedisCacheManager.Get<DateTime>("Value");
            return RedirectToAction("Index");
        }

        public ActionResult SetValue()
        {
            RedisCacheManager.Set("Value", DateTime.Now, 10);
            RedisCacheManager.Set("MovieModel", new Models.MovieModel(), 10);
            TempData["Value"] = DateTime.Now;
            return RedirectToAction("Index");
        }

        public ActionResult Premise()
        {
            ViewBag.Premise = TempData["Premise"];
            ViewBag.PremisScDateStamp = RedisCacheManager.Get<DateTime>("Premise.ScDateStamp");
            return View();
        }

        public ActionResult PremiseGetValue()
        {
            TempData["Premise"] = PremiseRedisCacheManager.Get<DateTime>("Premise", () =>
            {
                dbPath = Server.MapPath("~/App_Data/db.txt");
                var v = System.IO.File.ReadAllText(dbPath);
                return Convert.ToDateTime(v);
            });
            return RedirectToAction("Premise");
        }

        public ActionResult PremiseSetValue()
        {
            var v = DateTime.Now;
            //PremiseRedisCacheManager.Update("Premise", v, 10);
            //TempData["Premise"] = v;
            PremiseRedisCacheManager.Remove("Premise");

            dbPath = Server.MapPath("~/App_Data/db.txt");
            System.IO.File.WriteAllText(dbPath, v.ToString("yyyy/MM/dd HH:mm:ss"));
            return RedirectToAction("Premise");
        }

        public ActionResult PubSub()
        {
            ViewBag.PubSub = TempData["PubSub"];
            return View();
        }

        public ActionResult PubSubGetValue()
        {
            TempData["PubSub"] = PubSubMemoryCacheManager.Get<DateTime>("PubSub", () =>
            {
                dbPath = Server.MapPath("~/App_Data/db.txt");
                var v = System.IO.File.ReadAllText(dbPath);
                return Convert.ToDateTime(v);
            });
            return RedirectToAction("PubSub");
        }

        public ActionResult PubSubSetValue()
        {
            var v = DateTime.Now;
            PubSubMemoryCacheManager.Remove("PubSub");//照目前底層架構寫法,不更新,用移除的,讓取的時候重讀
        //    PubSubMemoryCacheManager.Update("PubSub", v, 10);
          //  TempData["PubSub"] = v;

            dbPath = Server.MapPath("~/App_Data/db.txt");
            System.IO.File.WriteAllText(dbPath, v.ToString("yyyy/MM/dd HH:mm:ss"));
            return RedirectToAction("PubSub");
        }

        public ActionResult PubSubClear()
        {
            PubSubMemoryCacheManager.Clear();
            return RedirectToAction("PubSub");
        }
    }
}