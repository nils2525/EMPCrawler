using EMPCrawler.Model;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EMPCrawler
{
    public class Client
    {
        private const string _sessionFileName = "emp.session";

        /// <summary>
        /// EMP Username
        /// </summary>
        private readonly string _username;
        /// <summary>
        /// EMP Password
        /// </summary>
        private readonly string _password;

        /// <summary>
        /// Current cookies, send with every request
        /// </summary>
        private CookieContainer _cookies;

        /// <summary>
        /// Current HTTPClient
        /// </summary>
        private HttpClient _client;
        /// <summary>
        /// HTTPClient Handler
        /// </summary>
        private HttpClientHandler _clientHandler;

        /// <summary>
        /// True = User is loggedd in
        /// </summary>
        public bool IsLoggedIn { get; private set; } = false;

        /// <summary>
        /// Min delay in seconds for every request
        /// </summary>
        public int MinDelaySeconds;
        /// <summary>
        /// Max delay in seconds for every request
        /// </summary>
        public int MaxDelaySeconds;

        /// <summary>
        /// Client with login credentials
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public Client(string username, string password)
        {
            _username = username;
            _password = password;

            InitInstance();
        }

        /// <summary>
        /// Client without login credentials
        /// </summary>
        public Client()
        {
            InitInstance();
        }

        /// <summary>
        /// Called from ctor
        /// </summary>
        private void InitInstance()
        {
            _cookies = new CookieContainer();
            _clientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = _cookies
            };
            _client = new HttpClient(_clientHandler);

            MinDelaySeconds = MaxDelaySeconds = 0;
        }

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="scope">Path to redirect (e.g. "wishlist" to redirect to "emp.de/wishlist" after login </param>
        /// <exception cref="HttpRequestException"></exception>
        /// <returns>HtmlDocument of scope page</returns>
        public async Task<HtmlDocument> Login(string scope = null)
        {
            if (String.IsNullOrWhiteSpace(_username) || String.IsNullOrWhiteSpace(_password))
            {
                return null;
            }

            //Delete possible cookies
            _cookies = new CookieContainer();

            //Init request to generate needed cookies
            var initRequest = await _client.GetAsync("https://www.emp.de/" + (scope ?? "login"));
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

        public bool RestoreLoginCookies()
        {
            if (File.Exists(_sessionFileName))
            {
                try
                {
                    var cookieJson = File.ReadAllText(_sessionFileName);
                    var cookieList = JsonConvert.DeserializeObject<List<Cookie>>(cookieJson);

                    _clientHandler.CookieContainer = _cookies = new CookieContainer();
                    foreach (var cookie in cookieList)
                    {
                        _cookies.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
                    }

                    return IsLoggedIn = true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        public bool StoreLoginCookies()
        {
            try
            {
                var cookies = _clientHandler.CookieContainer.GetCookies(new Uri("https://www.emp.de/"));
                var container = JsonConvert.SerializeObject(GetAllCookies(_clientHandler.CookieContainer));
                File.WriteAllText(_sessionFileName, container);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public async Task<bool> CurrentCookiesAreValid()
        {
            var response = await _client.GetAsync("https://www.emp.de/");
            if (response.IsSuccessStatusCode)
            {
                //Get raw string
                var wishListRaw = await response.Content.ReadAsStringAsync();

                //Init document
                var document = new HtmlDocument();
                document.LoadHtml(wishListRaw);

                return document.DocumentNode.Descendants("a").Where(c => (c.Attributes["class"]?.Value?.Contains("user-account") ?? false) && !(c.Attributes["title"]?.Value?.Contains("Login") ?? false)).Count() > 0;
            }
            else
            {
                return false;
            }
        }


        private static CookieCollection GetAllCookies(CookieContainer cookieJar)
        {
            CookieCollection cookieCollection = new CookieCollection();

            Hashtable table = (Hashtable)cookieJar.GetType().InvokeMember("m_domainTable", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance, null, cookieJar, new object[] { });

            foreach (var tableKey in table.Keys)
            {
                String str_tableKey = (string)tableKey;

                if (str_tableKey[0] == '.')
                {
                    str_tableKey = str_tableKey.Substring(1);
                }

                SortedList list = (SortedList)table[tableKey].GetType().InvokeMember("m_list", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance, null, table[tableKey], new object[] { });

                foreach (var listKey in list.Keys)
                {
                    String url = "https://" + str_tableKey + (string)listKey;
                    cookieCollection.Add(cookieJar.GetCookies(new Uri(url)));
                }
            }

            return cookieCollection;
        }



        /// <summary>
        /// Get all items from wishlist
        /// </summary>
        /// <param name="wishList"></param>
        /// <returns></returns>
        public async Task<List<Product>> GetWishListProductsAsync(HtmlDocument wishList = null)
        {
            Console.WriteLine("Get products from wishlist");

            //Only possible with logged in user
            if (!IsLoggedIn)
            {
                return null;
            }

            //Get Wishlist, if parameter is null
            if (wishList == null)
            {
                WaitForDelay();

                //Get products from wishlist
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

            //Every item row
            var itemRows = wishList.DocumentNode.DescendantsAndSelf("tr").Where(c => c.Attributes.Contains("class") && c.Attributes["class"].Value.Contains("item-row js-product-items")).ToList();

            var products = new List<Product>();
            foreach (var itemRow in itemRows)
            {
                //Generate Product instance
                var product = GetProductFromWishList(itemRow);

                if (product != null)
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
        private Product GetProductFromWishList(HtmlNode itemRow)
        {
            var imageUrl = GetImageLink(itemRow);

            var nameNode = itemRow.DescendantsAndSelf("div").Where(c => c.Attributes["class"]?.Value == "product-list-item").FirstOrDefault();

            var productlink = nameNode.DescendantsAndSelf("a").Where(c => c.Attributes.Contains("href")).FirstOrDefault()?.Attributes["href"].Value;
            var productName = nameNode.DescendantsAndSelf("a").Where(c => c.Attributes.Contains("href")).FirstOrDefault()?.InnerText.Replace("\n", "");
            var productBrand = nameNode.Descendants("span").FirstOrDefault().InnerText;
            var productType = nameNode.Descendants("span").Skip(2).FirstOrDefault().InnerText;

            var productCodeString = nameNode.Descendants("td").Where(c => c.Attributes["class"]?.Value == "dyvalue").FirstOrDefault().InnerText;
            var productCode = Int32.Parse(productCodeString);


            var availabilityString = itemRow.Descendants("ul").Where(c => c.Attributes["class"]?.Value == "product-availability-list").FirstOrDefault()?.Descendants("li")?.Where(c => c.Attributes.Contains("class"))?.FirstOrDefault()?.Attributes["class"]?.Value;

            var priceNode = GetPrices("price", itemRow, out decimal normalPrice, out SaleType? saleType, out decimal? salePrice);

            var discountString = priceNode.Descendants("input").Where(c => c.Attributes["class"]?.Value == "disountPercent")?.FirstOrDefault()?.Attributes["value"]?.Value?.Replace("%", "");

            int? discount = null;
            if (discountString != null)
            {
                discount = Int32.Parse(discountString);
            }

            return new Product()
            {
                Link = "https://www.emp.de" + productlink,
                Name = productName,
                Brand = productBrand,
                Type = productType,
                ProductCode = productCode,
                AvailabilityString = availabilityString,
                NormalPrice = normalPrice,
                SalePrice = salePrice,
                DiscountPercentage = discount,
                ImageUrl = imageUrl,
                SaleType = saleType
            };
        }

        /// <summary>
        /// Get Price info from ItemRow
        /// </summary>
        /// <param name="priceNodeName"></param>
        /// <param name="itemRow"></param>
        /// <param name="normalPrice"></param>
        /// <param name="saleType"></param>
        /// <param name="salePrice"></param>
        /// <returns></returns>
        private static HtmlNode GetPrices(string priceNodeName, HtmlNode itemRow, out decimal normalPrice, out SaleType? saleType, out decimal? salePrice)
        {
            var priceNode = itemRow.Descendants("div").Where(c => c.Attributes["class"]?.Value == priceNodeName).FirstOrDefault();

            var normalPriceString = priceNode.Descendants("span").Where(c => c.Attributes.Count == 0).FirstOrDefault().InnerText;
            normalPrice = Decimal.Parse(Regex.Match(normalPriceString, "[0-9]{1,3},[0-9]{1,2}").Value);
            saleType = null;
            salePrice = null;
            var salePriceString = priceNode.Descendants("span").Where(c => c.Attributes["class"]?.Value == "price-sale price-sales")?.FirstOrDefault()?.InnerText;
            if (salePriceString != null)
            {
                saleType = SaleType.Normal;
                salePrice = Decimal.Parse(Regex.Match(salePriceString, "[0-9]{1,3},[0-9]{1,2}").Value);
            }

            var bscSalePriceString = priceNode.Descendants("span").Where(c => c.Attributes["class"]?.Value == "price-bsc price-sales")?.FirstOrDefault()?.InnerText;
            if (bscSalePriceString != null)
            {
                saleType = SaleType.BSC;
                salePrice = Decimal.Parse(Regex.Match(bscSalePriceString, "[0-9]{1,3},[0-9]{1,2}").Value);
            }

            return priceNode;
        }

        /// <summary>
        /// Get products from product list url
        /// </summary>
        /// <param name="productsUrl"></param>
        /// <param name="pages">pages to get items (null = max available)</param>
        /// <returns></returns>
        public async Task<List<Product>> GetProductsAsync(string productsUrl, int? pages = null)
        {
            if (String.IsNullOrWhiteSpace(productsUrl))
            {
                return null;
            }

            var firstUrl = UpdatePage(productsUrl);
            var firstPage = await _client.GetAsync(firstUrl);
            if (firstPage.IsSuccessStatusCode)
            {
                var firstPageString = await firstPage.Content.ReadAsStringAsync();
                var firstPageDocument = new HtmlDocument();
                firstPageDocument.LoadHtml(firstPageString);

                //Max Pages available
                var maxPages = GetMaxPages(firstPageDocument);

                if (pages == null || pages > maxPages)
                {
                    //Set pages to maxPages
                    pages = maxPages;
                }

                var products = new List<Product>();

                //Get products from first pageDocument
                var firstPageProducts = GetProductsFromPage(firstPageDocument);
                if (firstPageProducts?.Count > 0)
                {
                    products.AddRange(firstPageProducts);
                }


                for (int i = 1; i < pages; i++)
                {
                    //Delay next request...
                    WaitForDelay();

                    //Url for next page
                    var currentPageUrl = UpdatePage(productsUrl, i);

                    //Get page
                    var currentPageRaw = await _client.GetAsync(currentPageUrl);
                    var currentPageString = await currentPageRaw.Content.ReadAsStringAsync();

                    var currentPageDocument = new HtmlDocument();
                    currentPageDocument.LoadHtml(currentPageString);

                    //Get products from html document
                    var currentProducts = GetProductsFromPage(currentPageDocument);
                    if (currentProducts?.Count > 0)
                    {
                        products.AddRange(currentProducts);
                    }
                }
                return products;
            }
            return null;
        }

        /// <summary>
        /// Update page url for next page
        /// </summary>
        /// <param name="url"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        private string UpdatePage(string url, int page = 0)
        {
            if (page < 0)
            {
                page = 0;
            }

            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

            //Products per page
            query["sz"] = "60";
            //Start at n products (every page has 60 items, so we calculate 60 * pages)
            query["start"] = (60 * page).ToString();

            return uri.Scheme + "://" + uri.Host + uri.AbsolutePath + "?" + query.ToString();
        }

        /// <summary>
        /// Get count of max pages
        /// </summary>
        /// <param name="productListPage"></param>
        /// <returns></returns>
        private int GetMaxPages(HtmlDocument productListPage)
        {
            if (productListPage == null)
            {
                return -1;
            }

            var listInfo = productListPage.DocumentNode.Descendants("span").Where(c => c.Attributes["class"]?.Value == "search-result-articles-mobile").FirstOrDefault();
            var pageInfo = listInfo.Descendants("span").Where(c => c.Attributes["class"]?.Value == "bold").ToList();

            var maxPageString = pageInfo?.Skip(1)?.FirstOrDefault()?.InnerText;
            if (Int32.TryParse(maxPageString, out int maxPage))
            {
                return maxPage;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Get products from product-list page
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private List<Product> GetProductsFromPage(HtmlDocument page)
        {
            var products = new List<Product>();
            var rawProductsInPage = page.DocumentNode.Descendants("div").Where(c => c.Attributes["class"]?.Value == "grid-tile").ToList();

            foreach (var rawProduct in rawProductsInPage)
            {
                var product = GetProduct(rawProduct);
                if (product != null)
                {
                    products.Add(product);
                }
            }
            return products;
        }

        /// <summary>
        /// Get product from product-list page entry
        /// </summary>
        /// <param name="productNode"></param>
        /// <returns></returns>
        private Product GetProduct(HtmlNode productNode)
        {
            if (productNode == null)
            {
                return null;
            }

            var productTitleNode = productNode.Descendants("div").Where(c => c.Attributes["class"]?.Value == "product-tile").FirstOrDefault();

            var productCodeString = productTitleNode.Attributes["data-itemid"]?.Value;
            var productCode = Int32.Parse(productCodeString);

            var linkNode = productTitleNode.Descendants("a").Where(c => c.Attributes["class"]?.Value == "product-link thumb-link").FirstOrDefault();
            var link = linkNode.Attributes["href"]?.Value;

            string imageLink = GetImageLink(productTitleNode);

            var priceNode = GetPrices("product-pricing", productNode, out decimal normalPrice, out SaleType? saleType, out decimal? salePrice);

            var discountNode = priceNode.Descendants("div").Where(c => c.Attributes["class"]?.Value == "discount-price-badge").FirstOrDefault();
            var discountString = discountNode?.Descendants("span")?.FirstOrDefault()?.InnerText?.Replace("%", "");

            int? discount = null;
            if (discountString != null)
            {
                discount = Int32.Parse(discountString);
            }

            var nameNode = productNode.Descendants("div").Where(c => c.Attributes["class"]?.Value == "product-name").FirstOrDefault();
            var name = nameNode.Descendants("span").Where(c => c.Attributes["class"]?.Value == "bold").FirstOrDefault()?.InnerText.Replace("\n", "");
            var brand = nameNode.Descendants("span").Where(c => c.Attributes.Count == 0).FirstOrDefault().InnerText.Replace("\n", "");
            var type = nameNode.Descendants("span").Where(c => c.Attributes.Count == 0).Skip(1).FirstOrDefault().InnerText.Replace("\n", "");

            var product = new Product()
            {
                Availability = ProductAvailability.InStock,
                Brand = brand,
                DiscountPercentage = discount,
                ImageUrl = imageLink,
                Link = link,
                Name = name,
                NormalPrice = normalPrice,
                ProductCode = productCode,
                SalePrice = salePrice,
                SaleType = saleType,
                Type = type
            };

            return product;
        }

        /// <summary>
        /// Get image link for item
        /// </summary>
        /// <param name="productTitleNode"></param>
        /// <returns></returns>
        private static string GetImageLink(HtmlNode productTitleNode)
        {
            var imageAttributes = productTitleNode.Descendants("img").Where(c => c.Attributes.Contains("alt")).FirstOrDefault()?.Attributes?.ToList();

            string imageLink = null;
            if (imageAttributes.Any(c => c.Name == "data-src"))
            {
                imageLink = imageAttributes.Where(c => c.Name == "data-src").FirstOrDefault()?.Value;
            }
            else if (imageAttributes.Any(c => c.Name == "src"))
            {
                imageLink = imageAttributes.Where(c => c.Name == "src").FirstOrDefault()?.Value;
            }

            return imageLink;
        }

        /// <summary>
        /// Delay for next request
        /// </summary>
        private void WaitForDelay()
        {
            if (MinDelaySeconds >= 0 && MaxDelaySeconds > 0)
            {
                var randomizer = new Random();
                var randomDelay = randomizer.Next(MinDelaySeconds * 1000, MaxDelaySeconds * 1000);
                Thread.Sleep(randomDelay);
            }
        }
    }
}