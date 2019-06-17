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
        static async Task Main(string[] args)
        {
            if(args == null)
            {
                args = new string[2];
                args[0] = "";
                args[1] = "45";
            }

            if (Data.ConfigService.LoadConfig())
            {
                var client = new Client(Data.ConfigService.Instance.Mailaddress, Data.ConfigService.Instance.Password);
                int minDiscount = 0;

                List<Model.Product> products;

                //If no valid cookies saved
                if (!client.RestoreLoginCookies() || !await client.CurrentCookiesAreValid())
                {
                    //Login
                    await client.Login();
                }

                if (String.IsNullOrWhiteSpace(args?[0]))
                {
                    //Get wishlist
                    products = await client.GetWishListProductsAsync();
                }
                else
                {
                    products = await client.GetProductsAsync(args[0]);

                    if (!String.IsNullOrWhiteSpace(args[1]))
                    {
                        minDiscount = Int32.Parse(args[1]);
                    }
                }

                //Store cookies
                client.StoreLoginCookies();

                //Update products
                DBHelper.UpdateProducts(products, minDiscount);
            }
        }
    }
}
