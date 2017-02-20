using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace CacheMemoryMeasureLab.Web.Controllers
{
    public class HomeController : Controller
    {
        public static int IndexCount = 0;
        public ActionResult Index()
        {
            IndexCount += 1;
            ViewBag.IndexCount = IndexCount;
            return View();
        }

        public ActionResult About()
        {
            //http://stackoverflow.com/questions/9470132/how-can-i-programmatically-recycle-a-net-web-apps-own-apppool
            //Pooling要使用較高的權限,例:Local System
            //將Pooling加入相關資料夾 C:\Windows\System32\inetsrv\config-->沒用  http://stackoverflow.com/questions/9470132/how-can-i-programmatically-recycle-a-net-web-apps-own-apppool
            using (ServerManager iisManager = new ServerManager())
            {
                SiteCollection sites = iisManager.Sites;
                foreach (Site site in sites)
                {
                    if (site.Name == HostingEnvironment.ApplicationHost.GetSiteName())
                    {
                        iisManager.ApplicationPools[site.Applications["/"].ApplicationPoolName].Recycle();
                        break;
                    }
                }
            }

            return Redirect("Index");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}