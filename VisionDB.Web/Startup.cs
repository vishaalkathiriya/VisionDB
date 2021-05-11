using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(VisionDB.Startup))]
namespace VisionDB
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
