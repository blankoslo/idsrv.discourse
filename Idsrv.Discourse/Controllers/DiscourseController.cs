using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.InMemory;
using Idsrv.Discourse.Config;

namespace Idsrv.Discourse.Controllers
{
    public class DiscourseController : Controller
    {
        const string DISCOURSE_SECRET = "asd";
        
        // Redirected from Discourse
        [Route("core/discourse")]
        public ActionResult Index(string sso, string sig)
        {
            if (string.IsNullOrEmpty(sso) || string.IsNullOrEmpty(sig))
            {
                throw new ArgumentException("Missing input parameters");
            }

            if (!Validatesso(sso, sig))
            {
                throw new SecurityException("sso sig not valid");
            }

            var res = Request.GetOwinContext().Environment.GetIdentityServerFullLoginAsync().GetAwaiter().GetResult();

            if (res == null || !res.Claims.Any())
            {
                // Not authenticated, returning login page
                TempData["sso"] = sso;
                return View();
            }

            // User authenticated, getting user, generating sso and redirecting back to Discourse
            var user = Users.GetUser(res.Name);

            var redirectUrl = CreateDiscourseRedirectUrl(user, sso);
            return new RedirectResult(redirectUrl);
        }

        [Route("core/discourse/login")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            if (Users.PasswordMatch(username, password))
            {
                var user = Users.GetUser(username);

                var authLogin = new AuthenticatedLogin
                {
                    AuthenticationMethod = "password",
                    Name = user.GetClaim("name"),
                    Subject = user.Subject,
                    PersistentLogin = true
                };

                Request.GetOwinContext().Environment.IssueLoginCookie(authLogin);

                var sso = TempData["sso"] as string;
                if (sso == null)
                {
                    throw new Exception("Unable to retrieve Discourse sso from memory");
                }

                TempData["sso"] = null;

                var redirectUrl = CreateDiscourseRedirectUrl(user, sso);
                return new RedirectResult(redirectUrl);
            }

            TempData["error"] = "Wrong username or password";
            return View("Index");
        }

        [Route("identity/discourse/logout")]
        public void Logout()
        {
            Request.GetOwinContext().Authentication.SignOut();
        }

        public static bool Validatesso(string encodedsso, string sig)
        {
            return Hash(DISCOURSE_SECRET, encodedsso) == sig;
        }

        public static string CreateDiscourseRedirectUrl(InMemoryUser user, string originalEncodedsso)
        {
            var urlParameters = Parsesso(originalEncodedsso);
            var nonce = urlParameters.Get("nonce");
            var returnUrl = urlParameters.Get("return_sso_url");
            
            var ssoDictionary = new Dictionary<string, string>
            {
                {"nonce", nonce},
                {"email", HttpUtility.UrlEncode(user.GetClaim("email"))},
                {"external_id", HttpUtility.UrlEncode(user.Subject)},
                {"username", HttpUtility.UrlEncode(user.Username)},
                {"name", HttpUtility.UrlEncode(user.GetClaim("name"))}
            };

            var returnsso = CreatessoQueryString(ssoDictionary);

            var returnssoEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(returnsso));
            var returnSig = Hash(DISCOURSE_SECRET, returnssoEncoded);

            return $"{returnUrl}?sso={returnssoEncoded}&sig={returnSig}";
        }

        private static string Hash(string secret, string sso)
        {
            var hasher = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(sso));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private static NameValueCollection Parsesso(string encodedsso)
        {
            var queryString = Encoding.UTF8.GetString(Convert.FromBase64String(encodedsso));
            return HttpUtility.ParseQueryString(queryString);
        }

        private static string CreatessoQueryString(Dictionary<string, string> dictionary)
        {

            return string.Join("&", dictionary.Select(x => $"{x.Key}={x.Value}"));
        }
    }
}