using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MESACCA.Startup))]
namespace MESACCA
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
