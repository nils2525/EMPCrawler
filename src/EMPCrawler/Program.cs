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
            //var client = new Client("", "");
            //var loginResult = await client.Login("wishlist");
            //var wishList = await client.GetWishListProductsAsync(loginResult);
            //DBHelper.UpdateProducts(wishList);

            //var client = new Client();
            //var products = await client.GetProductsAsync("https://www.emp.de/maenner-kleidung/?prefn1=gender&prefn2=productTopic&prefv3=L%7Cone%20size%7CW34L34%7C38%7C146293%7C66%7C120%20cm%7CEU46%7CEU%2043-46%7CStandard%7CL-XL%7C50%20cm&prefv1=3%7C1&prefv2=18759%7C18766%7C18765%7C18760%7C18763%7C25127%7C25123%7C25129%7C33342%7C33334%7C33332%7C51517%7C51532%7C34638%7C33333%7C25128%7C33331%7C51526&prefn3=size");
            //DBHelper.UpdateProducts(products);

            IntervalJobService.InitJobService("", "");
            IntervalJobService.StartWishlistUpdater(60);

            new Task(() => { }).Wait(-1);
        }
    }
}
