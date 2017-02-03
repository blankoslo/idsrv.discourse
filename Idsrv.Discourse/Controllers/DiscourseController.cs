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
        // Redirected from Discourse
        [Route("core/discourse")]
        public ActionResult Index([Bind(Prefix = "sso")]string payload, [Bind(Prefix = "sig")]string signature)
        {
            if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signature))
            {
                throw new ArgumentException("Missing input parameters");
            }

            if (!ValidatePayload(payload, signature))
            {
                throw new SecurityException("Payload signature not valid");
            }

            var res = Request.GetOwinContext().Environment.GetIdentityServerFullLoginAsync().GetAwaiter().GetResult();

            if (res == null || !res.Claims.Any())
            {
                // Not authenticated, returning login page
                TempData["payload"] = payload;
                return View();
            }

            // User authenticated, getting user from db, generating payload and redirecting back to Discourse
            var user = _userService.GetUser(res.Name);

            if (user == null)
            {
                throw new UserNotFoundException($"Could not get user with username '{res.Name}' from db");
            }

            var redirectUrl = CreateDiscourseRedirectUrl(user, payload);
            return new RedirectResult(redirectUrl);
        }

        [Route("core/discourse/login")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Login login)
        {
            if (_authenticationService.Authenticate(login.Email, login.Password))
            {
                var user = _userService.GetUser(login.Email);
                if (user == null)
                {
                    throw new UserNotFoundException($"Could not get user with username '{login.Email}' from db");
                }

                var authLogin = new AuthenticatedLogin
                {
                    AuthenticationMethod = "password",
                    Name = user.Email,
                    Subject = user.Email,
                    PersistentLogin = true
                };

                Request.GetOwinContext().Environment.IssueLoginCookie(authLogin);

                var payload = TempData["payload"] as string;
                if (payload == null)
                {
                    throw new Exception("Unable to retrieve Discourse payload from memory");
                }

                TempData["payload"] = null;

                var redirectUrl = CreateDiscourseRedirectUrl(user, payload);
                return new RedirectResult(redirectUrl);
            }

            TempData["error"] = "Wrong username or password";
            return View("Index");
        }

        [Route("identity/discourse/logout")]
        public ActionResult Logout()
        {
            Request.GetOwinContext().Authentication.SignOut();
            return Redirect(AppSettings.DiscoursePostLogoutRedirectUri());
        }




        public static bool ValidatePayload(string encodedPayload, string signature)
        {
            return Hash(AppSettings.DiscourseSecret(), encodedPayload) == signature;
        }

        public static string CreateDiscourseRedirectUrl(User user, string originalEncodedPayload)
        {
            var urlParameters = ParsePayload(originalEncodedPayload);
            var nonce = urlParameters.Get("nonce");
            var returnUrl = urlParameters.Get("return_sso_url");
            
            var payloadDictionary = new Dictionary<string, string>
            {
                {"nonce", nonce},
                {"email", HttpUtility.UrlEncode(user.Email)},
                {"external_id", HttpUtility.UrlEncode(user.Id.ToString())},
                {"username", HttpUtility.UrlEncode(user.Email)},
                {"name", HttpUtility.UrlEncode(user.Name)},
                {"avatar_url", HttpUtility.UrlEncode(user.ImageUrl)},
                {"bio", HttpUtility.UrlEncode(user.Description)},
                {"suppress_welcome_message", "true"}
            };

            if (user.CompanyUser)
            {
                payloadDictionary.Add("moderator", "true");
            }
            if (user.Administrator)
            {
                payloadDictionary.Add("admin", "true");
            }

            var returnPayload = CreatePayloadQueryString(payloadDictionary);

            var returnPayloadEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(returnPayload));
            var returnSig = Hash(AppSettings.DiscourseSecret(), returnPayloadEncoded);

            return $"{returnUrl}?sso={returnPayloadEncoded}&sig={returnSig}";
        }

        private static string Hash(string key, string payload)
        {
            var hasher = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private static NameValueCollection ParsePayload(string encodedPayload)
        {
            var queryString = Encoding.UTF8.GetString(Convert.FromBase64String(encodedPayload));
            return HttpUtility.ParseQueryString(queryString);
        }

        private static string CreatePayloadQueryString(Dictionary<string, string> dictionary)
        {

            return string.Join("&", dictionary.Select(x => $"{x.Key}={x.Value}"));
        }
    }
}