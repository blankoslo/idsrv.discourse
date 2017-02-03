using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using IdentityServer3.Core;
using IdentityServer3.Core.Services.InMemory;

namespace Idsrv.Discourse.Config
{
    public static class Users
    {
        public static List<InMemoryUser> Get()
        {
            var users = new List<InMemoryUser>
            {
                new InMemoryUser
                {
                    Subject = "88421113",
                    Username = "bob",
                    Password = "bob",
                    Claims = new[]
                    {
                        new Claim(Constants.ClaimTypes.Name, "Bob Smith"),
                        new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                        new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                        new Claim(Constants.ClaimTypes.Email, "bob@bob.com"),
                    }
                },
            };

            return users;
        }

        public static InMemoryUser GetUser(string username)
        {
            return Get().FirstOrDefault(u => u.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase));
        }

        public static bool PasswordMatch(string username, string password)
        {
            return GetUser(username).Password.Equals(password);
        }
    }

    public static class InMemoryUserExtensions
    {
        public static string GetClaim(this InMemoryUser user, string claimType)
        {
            return user.Claims.First(c => c.Type == claimType).Value;
        }
    }
}