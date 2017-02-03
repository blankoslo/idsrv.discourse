using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Idsrv.Discourse.Config
{
    public class Mocks
    {
        private const string DISCOURSE_SECRET = "asd";

        public static IncomingDiscourseRequestMock GenerateFakeDiscoursceIncomingRequest()
        {
            var ssoDictionary = new Dictionary<string, string>
            {
                {"nonce", "something"},
                {"return_sso_url", HttpUtility.UrlEncode("http://discourse_site/session/sso_login")},
            };

            var returnsso = CreatessoQueryString(ssoDictionary);

            var returnssoEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(returnsso));
            var returnSig = Hash(DISCOURSE_SECRET, returnssoEncoded);
            return new IncomingDiscourseRequestMock { Sso = returnssoEncoded, Sig = returnSig };
        }

        private static string CreatessoQueryString(Dictionary<string, string> dictionary)
        {
            return string.Join("&", dictionary.Select(x => $"{x.Key}={x.Value}"));
        }

        private static string Hash(string secret, string sso)
        {
            var hasher = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(sso));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public class IncomingDiscourseRequestMock
        {
            public string Sso;
            public string Sig;
        }
    }
}