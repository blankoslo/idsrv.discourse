using IdentityServer3.Core.Configuration;
using Idsrv.Discourse;
using Idsrv.Discourse.Config;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace Idsrv.Discourse
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var options = new IdentityServerOptions
            {
                Factory = new IdentityServerServiceFactory().UseInMemoryClients(Clients.Get()).UseInMemoryUsers(Users.Get()).UseInMemoryScopes(Scopes.Get()),
                RequireSsl = false
            };

            app.Map("/core", a => a.UseIdentityServer(options));
        }
    }
}
