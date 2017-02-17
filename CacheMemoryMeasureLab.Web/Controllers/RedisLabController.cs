﻿using System;
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
        PubSubRedisCacheManager PubSubRedisCacheManager;
        string dbPath = "";
        public RedisLabController()
        {
            RedisCacheManager = new RedisCacheManager();
            PremiseRedisCacheManager = new PremiseRedisCacheManager();
            PubSubRedisCacheManager = new PubSubRedisCacheManager();
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
            PremiseRedisCacheManager.Update("Premise", v, 10);
            TempData["Premise"] = v;

            dbPath = Server.MapPath("~/App_Data/db.txt");
            System.IO.File.WriteAllText(dbPath, v.ToString("yyyy/MM/dd HH:mm:ss"));
            return RedirectToAction("Premise");
        }

        public ActionResult PubSub()
        {
            ViewBag.PubSub = TempData["PubSub"];
            ViewBag.SubscribeNoticeCount = PubSubRedisCacheManager.SubscribeNoticeCount;
            ViewBag.SubscribePid = PubSubRedisCacheManager.SubscribePid;
            return View();
        }

        public ActionResult PubSubGetValue()
        {
            TempData["PubSub"] = PubSubRedisCacheManager.Get<DateTime>("PubSub", () =>
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
            PubSubRedisCacheManager.Update("PubSub", v, 10);
            TempData["PubSub"] = v;

            dbPath = Server.MapPath("~/App_Data/db.txt");
            System.IO.File.WriteAllText(dbPath, v.ToString("yyyy/MM/dd HH:mm:ss"));
            return RedirectToAction("PubSub");
        }

        public ActionResult PubSubClear()
        {
            PubSubRedisCacheManager.Clear();
            return RedirectToAction("PubSub");
        }
    }
}