using EMPCrawler.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EMPCrawler
{
    public static class IntervalJobService
    {
        private static Client _empClient;

        public static void InitJobService(string username, string password)
        {
            _empClient = new Client(username, password);
        }

        public static void InitJobService()
        {
            _empClient = new Client();
        }


        private static Timer _wishListTimer;
        public static void StartWishlistUpdater(int minutes)
        {
            _empClient.MinDelaySeconds = 10;
            _empClient.MaxDelaySeconds = 30;


            Console.WriteLine("Starting interval updater for wishlist");
            _wishListTimer = new Timer(async (e) =>
            {
                await UpdateWishlistProductsAsync();
            }, null, 0, minutes * 60 * 1000);
        }

        private static async Task UpdateWishlistProductsAsync()
        {
            List<Product> products;

            if (!_empClient.IsLoggedIn)
            {
                var loginResult = await _empClient.Login("wishlist");
                products = await _empClient.GetWishListProductsAsync(loginResult);
            }
            else
            {
                products = await _empClient.GetWishListProductsAsync();
            }

            DBHelper.UpdateProducts(products);
        }
    }
}
