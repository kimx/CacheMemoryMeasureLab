using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace CacheMemoryMeasureLab
{
    /// <summary>
    /// http://www.codeproject.com/Articles/167282/NET-4-0-MemoryCache-with-SqlChangeMonitor
    /// http://blog.csdn.net/alex_xfboy/article/details/10387057
    /// http://benfoster.io/blog/monitoring-files-in-azure-blob-storage
    /// ALTER DATABASE database_name SET TRUSTWORTHY ON WITH ROLLBACK IMMEDIATE
    /// ALTER DATABASE database_name SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE
    /// ALTER AUTHORIZATION ON DATABASE::database_name TO sa
    /// update MaintenanceMode set MaintenanceMode=IIF(MaintenanceMode=0,1,0);
    /// </summary>
    public class MemoryCacheMonitor
    {
        public static bool IsInMaintenanceMode()
        {
            bool inMaintenanceMode;
            string connStr = "Data Source=KIM-MSI\\KIMSSQLSERVER;Initial Catalog=MVWDataBase;User ID=sa;Password=mis123;MultipleActiveResultSets=True";
            if (MemoryCache.Default["MaintenanceMode"] == null)
            {
                Console.WriteLine("Hitting the database...");
                CacheItemPolicy policy = new CacheItemPolicy();
                SqlDependency.Start(connStr);
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    using (SqlCommand command = new SqlCommand("Select MaintenanceMode From dbo.MaintenanceMode", conn))
                    {
                        command.Notification = null;
                        SqlDependency dep = new SqlDependency();
                        dep.AddCommandDependency(command);
                        conn.Open();
                        inMaintenanceMode = (bool)command.ExecuteScalar();
                        SqlChangeMonitor monitor = new SqlChangeMonitor(dep);
                        policy.ChangeMonitors.Add(monitor);
                        dep.OnChange += Dep_OnChange;
                    }
                }

                MemoryCache.Default.Add("MaintenanceMode", inMaintenanceMode, policy);
            }
            else
            {
                inMaintenanceMode = (bool)MemoryCache.Default.Get("MaintenanceMode");
            }

            return inMaintenanceMode;
        }

        private static void Dep_OnChange(object sender, SqlNotificationEventArgs e)
        {
            Console.WriteLine("UpdateCallback-Info:" + e.Info);
            Console.WriteLine("UpdateCallback-Source:" + e.Source);
            //TODO:Clear all of the relative caches
            List<string> keys = new List<string>();

            var cache = MemoryCache.Default;
            Console.WriteLine("cacheClear-before:" + cache.Count());

            foreach (var item in cache)
            {
                keys.Add(item.Key);

            }

            foreach (var key in keys)
            {
                cache.Remove(key);
            }
            Console.WriteLine("cacheClear-after:" + cache.Count());
        }
    }
}
