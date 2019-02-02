using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EMPCrawler
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new Client("", "");

            var loginResult = await client.Login("wishlist");
            var wishList = await client.GetWishListProducts(loginResult);
        }
    }
}
