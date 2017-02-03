using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Idsrv.Discourse.Startup))]

namespace Idsrv.Discourse
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            
        }
    }
}
