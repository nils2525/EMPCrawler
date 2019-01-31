using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EMPCrawler
{
    public class Client
    {
        private readonly string _username;
        private readonly string _password;

        private CookieContainer _cookies;
        private HttpClient _client;
        private HttpClientHandler _clientHandler;

        public bool IsLoggedIn { get; private set; } = false;

        public Client(string username, string password)
        {
            _username = username;
            _password = password;

            _cookies = new CookieContainer();
            _clientHandler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = _cookies
            };
            _client = new HttpClient(_clientHandler);
        }

        public async Task<bool> GetWishList()
        {
            var initRequest = await _client.GetAsync("https://www.emp.de/wishlist");
            var rawInit = await initRequest.Content.ReadAsStringAsync();

            var userNameKey = Regex.Match(rawInit, "dwfrm_login_username_.*(?=\">)");
            var passwordKey = Regex.Match(rawInit, "dwfrm_login_password_.*(?=\">)");
            var csrfToken = Regex.Match(rawInit, $"(?<=name=\"csrf_token\" value=\").*(?=\" \\/>)");

            var bodyContent = new Dictionary<string, string>()
            {
                {userNameKey.Value, _username },
                {passwordKey.Value, _password },
                {"dwfrm_login_rememberme", "true" },
                {"dwfrm_login_login", "Login" },
                {"csrf_token", csrfToken.Value }
            };

            HttpResponseMessage wishListResponse;
            using (var content = new FormUrlEncodedContent(bodyContent))
            {
                wishListResponse = await _client.PostAsync("https://www.emp.de/on/demandware.store/Sites-GLB-Site/de_DE/Login-LoginForm?scope=wishlist", content);
            }

            if (wishListResponse.IsSuccessStatusCode)
            {
                var wishListRaw = await wishListResponse.Content.ReadAsStringAsync();

                var wishListDocument = new HtmlDocument();
                wishListDocument.LoadHtml(wishListRaw);

                var itemRow = wishListDocument.DocumentNode.DescendantsAndSelf("tr").Where(c => c.Attributes.Contains("class") && c.Attributes["class"].Value.Contains("item-row js-product-items")).ToList();

            }

            return false;
        }
    }
}
