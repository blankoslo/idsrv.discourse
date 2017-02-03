using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using IdentityServer3.Core;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.InMemory;

namespace Idsrv.Discourse.Controllers
{
    public class DiscourseController : Controller
    {

        [Route("core/discourse")]
        public ActionResult Index(string username, string password)
        {
            var res = Request.GetOwinContext().Environment.GetIdentityServerFullLoginAsync().GetAwaiter().GetResult();
            if (res != null && res.Claims.Any())
            {
                var user = Users.Get().First(u => u.Username == res.Name.ToString());
                var discourseResponse = CreateCustomDiscourseResponse(user);
                return
                    new RedirectResult("https://localhost:44319/identity/discourse/mock?alreadyauth=1&payload=" +
                                       discourseResponse);
            }
            return View(new {username, password});
        }

        [Route("core/discourse/login")]
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var user = Users.Get().FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                var authLogin = new AuthenticatedLogin
                {
                    AuthenticationMethod = "password",
                    Name = username,
                    Claims = new List<Claim>
                    {
                        new Claim(Constants.ClaimTypes.PreferredUserName, username),
                        new Claim(Constants.ClaimTypes.GivenName,
                            user.Claims.First(c => c.Type == Constants.ClaimTypes.GivenName).Value),
                        new Claim(Constants.ClaimTypes.FamilyName,
                            user.Claims.First(c => c.Type == Constants.ClaimTypes.FamilyName).Value)
                    },
                    Subject = user.Subject,
                    PersistentLogin = true
                };
                Request.GetOwinContext().Environment.IssueLoginCookie(authLogin);
                var discourseResponse = CreateCustomDiscourseResponse(user);
                return new RedirectResult("https://localhost:44319/identity/discourse/mock?payload=" + discourseResponse);
            }
            else
            {
                TempData["error"] = "Wrong username + password";
                return RedirectToAction("Index");
            }
        }

        [Route("core/discourse/mock")]
        public ContentResult Mock(string payload, string alreadyauth)
        {
            return new ContentResult {Content = payload + " - AlreadyAuthenticated?:" + alreadyauth};
        }

        private string CreateCustomDiscourseResponse(InMemoryUser user)
        {
            return string.Format("{0} : {1} ", user.Username, user.Subject);
        }
    }
}