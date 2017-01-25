using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TGVL.Startup))]
namespace TGVL
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
