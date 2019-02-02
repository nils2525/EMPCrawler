using EMPCrawler.Model;
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

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="scope">Path to redirect (e.g. "wishlist" to redirect to "emp.de/wishlist" after login </param>
        /// <exception cref="HttpRequestException"></exception>
        /// <returns>HtmlDocument of scope page</returns>
        public async Task<HtmlDocument> Login(string scope = null)
        {
            if (String.IsNullOrWhiteSpace(scope))
            {
                scope = "";
            }

            //Delete possible cookies
            _cookies = new CookieContainer();

            //Init request to generate needed cookies
            var initRequest = await _client.GetAsync("https://www.emp.de/" + scope);
            var rawInit = await initRequest.Content.ReadAsStringAsync();

            //Get parameter keys for login (changed every login...)
            var userNameKey = Regex.Match(rawInit, "dwfrm_login_username_.*(?=\">)");
            var passwordKey = Regex.Match(rawInit, "dwfrm_login_password_.*(?=\">)");
            var csrfToken = Regex.Match(rawInit, $"(?<=name=\"csrf_token\" value=\").*(?=\" \\/>)");

            //generate body content for login request
            var bodyContent = new Dictionary<string, string>()
            {
                {userNameKey.Value, _username },
                {passwordKey.Value, _password },
                {"dwfrm_login_rememberme", "true" },
                {"dwfrm_login_login", "Login" },
                {"csrf_token", csrfToken.Value }
            };

            HttpResponseMessage loginResponse;
            using (var content = new FormUrlEncodedContent(bodyContent))
            {
                //Login, redirect to scope
                loginResponse = await _client.PostAsync("https://www.emp.de/on/demandware.store/Sites-GLB-Site/de_DE/Login-LoginForm?scope=" + scope, content);
            }

            if (loginResponse.IsSuccessStatusCode)
            {
                //Get response as string
                var scopeRaw = await loginResponse.Content.ReadAsStringAsync();

                //Load response as HtmlDocument
                var scopeDocument = new HtmlDocument();
                scopeDocument.LoadHtml(scopeRaw);


                IsLoggedIn = true;
                return scopeDocument;
            }
            else
            {
                IsLoggedIn = false;
                return null;
            }
        }

        /// <summary>
        /// Get all items from wishlist
        /// </summary>
        /// <param name="wishList"></param>
        /// <returns></returns>
        public async Task<List<Product>> GetWishListProducts(HtmlDocument wishList = null)
        {
            if (!IsLoggedIn)
            {
                return null;
            }

            //Get Wishlist, if parameter is null
            if (wishList == null)
            {
                var wishListResponse = await _client.GetAsync("https://www.emp.de/wishlist");
                if (wishListResponse.IsSuccessStatusCode)
                {
                    //Get raw string
                    var wishListRaw = await wishListResponse.Content.ReadAsStringAsync();

                    //Init document
                    wishList = new HtmlDocument();
                    wishList.LoadHtml(wishListRaw);
                }
            }

            var itemRows = wishList.DocumentNode.DescendantsAndSelf("tr").Where(c => c.Attributes.Contains("class") && c.Attributes["class"].Value.Contains("item-row js-product-items")).ToList();


            var products = new List<Product>();

            foreach (var itemRow in itemRows)
            {
                //Generate Product instance
                var product = GetProduct(itemRow);

                if(product != null)
                {
                    products.Add(product);
                }
            }

            return products;
        }

        /// <summary>
        /// Parse Product instance by HtmlNode
        /// </summary>
        /// <param name="itemRow">"item-row js-product-items" instance</param>
        /// <returns></returns>
        private static Product GetProduct(HtmlNode itemRow)
        {
            var imageUrl = itemRow.Descendants("img").FirstOrDefault().Attributes["src"]?.Value;


            var nameNode = itemRow.DescendantsAndSelf("div").Where(c => c.Attributes["class"]?.Value == "product-list-item").FirstOrDefault();

            var productlink = nameNode.DescendantsAndSelf("a").Where(c => c.Attributes.Contains("href")).FirstOrDefault()?.Attributes["href"].Value;
            var productName = nameNode.DescendantsAndSelf("a").Where(c => c.Attributes.Contains("href")).FirstOrDefault()?.InnerText.Replace("\n", "");
            var productBrand = nameNode.Descendants("span").FirstOrDefault().InnerText;
            var productType = nameNode.Descendants("span").Skip(2).FirstOrDefault().InnerText;

            var productCodeString = nameNode.Descendants("td").Where(c => c.Attributes["class"]?.Value == "dyvalue").FirstOrDefault().InnerText;
            var productCode = Int32.Parse(productCodeString);


            var availabilityString = itemRow.Descendants("ul").Where(c => c.Attributes["class"]?.Value == "product-availability-list").FirstOrDefault()?.Descendants("li")?.Where(c => c.Attributes.Contains("class"))?.FirstOrDefault()?.Attributes["class"]?.Value;


            var priceNode = itemRow.Descendants("div").Where(c => c.Attributes["class"]?.Value == "price").FirstOrDefault();

            var normalPriceString = priceNode.Descendants("span").Where(c => c.Attributes.Count == 0).FirstOrDefault().InnerText;
            var normalPrice = Decimal.Parse(Regex.Match(normalPriceString, "[0-9]{1,3},[0-9]{1,2}").Value);

            var salePriceString = priceNode.Descendants("span").Where(c => c.Attributes["class"]?.Value == "price-sale price-sales")?.FirstOrDefault()?.InnerText;

            decimal? salePrice = null;
            if (salePriceString != null)
            {
                salePrice = Decimal.Parse(Regex.Match(salePriceString, "[0-9]{1,3},[0-9]{1,2}").Value);
            }

            var discountString = priceNode.Descendants("input").Where(c => c.Attributes["class"]?.Value == "disountPercent")?.FirstOrDefault()?.Attributes["value"]?.Value?.Replace("%", "");

            int? discount = null;
            if (discountString != null)
            {
                discount = Int32.Parse(discountString);
            }

            return new Product()
            {
                Link = productlink,
                Name = productName,
                Brand = productBrand,
                Type = productType,
                ProductCode = productCode,
                AvailabilityString = availabilityString,
                NormalPrice = normalPrice,
                SalePrice = salePrice,
                DiscountPercentage = discount,
                ImageUrl = imageUrl
            };
        }
    }
}