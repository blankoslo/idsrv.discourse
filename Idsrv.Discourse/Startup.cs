using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.InMemory;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Idsrv.Discourse.Startup))]

namespace Idsrv.Discourse
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var options = new IdentityServerOptions
            {
                Factory =
                    new IdentityServerServiceFactory().UseInMemoryClients(Clients.Get()).UseInMemoryUsers(Users.Get())
            };

            app.Map("/oauth2", a => a.UseIdentityServer(options));
        }

        public class Clients
        {
            public static IEnumerable<Client> Get()
            {
                return new[]
                {
                    new Client
                    {
                        ClientId = "Client1",
                        RedirectUris = new List<string> {"http://localhost:1982/"}
                    }
                };
            }
        }

        static class Users
        {
            public static List<InMemoryUser> Get()
            {
                var users = new List<InMemoryUser>
                {
                    new InMemoryUser{Subject = "88421113", Username = "bob", Password = "bob",
                        Claims = new[]
                        {
                            new Claim(Constants.ClaimTypes.Name, "Bob Smith"),
                            new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                            new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                        }
                    },
                };

                return users;
            }
        }
    }
}
