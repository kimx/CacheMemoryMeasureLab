using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CacheMemoryMeasureLab.Web.Startup))]
namespace CacheMemoryMeasureLab.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
