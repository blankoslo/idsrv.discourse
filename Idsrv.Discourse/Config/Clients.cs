using System.Collections.Generic;
using IdentityServer3.Core.Models;

namespace Idsrv.Discourse.Config
{
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
}