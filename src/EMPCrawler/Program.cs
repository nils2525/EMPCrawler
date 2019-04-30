using Microsoft.Extensions.Options;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace EMPCrawler
{
    class Program
    {
        internal static string TelegramKey { get; private set; }
        static async Task Main(string[] args)
        {
            TelegramKey = args[2];

            var client = new Client(args[0], args[1]);

            //If no valid cookies saved
            if (!client.RestoreLoginCookies() || !await client.CurrentCookiesAreValid())
            {
                //Login
                var loginResult = await client.Login("wishlist");

                //Get wishlist
                var wishList = await client.GetWishListProductsAsync(loginResult);

                //Store cookies
                client.StoreLoginCookies();

                //Update products
                DBHelper.UpdateProducts(wishList);
            }
            else
            {
                //Has already valid cookies, get wishlist
                var wishList = await client.GetWishListProductsAsync();

                //Store updated cookies
                client.StoreLoginCookies();

                //Update products
                DBHelper.UpdateProducts(wishList);
            }

            //IntervalJobService.InitJobService(args[0], args[1]);
            //IntervalJobService.StartWishlistUpdater(60);

            //new Task(() => { }).Wait(-1);
        }
    }
}
