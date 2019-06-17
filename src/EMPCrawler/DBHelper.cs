using EMPCrawler.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EMPCrawler
{
    public static class DBHelper
    {
        public static bool UpdateProducts(List<Product> products, int minDiscount)
        {
            using (var context = new SQLiteContext())
            {                
                //context.Database.EnsureCreated();

                foreach (var product in products)
                {
                    var dbProduct = context.Products.Include(nameof(Product.ProductHistories)).Where(p => p.ProductCode == product.ProductCode).FirstOrDefault() ?? 
                        context.Products.Local.Where(p => p.ProductCode == product.ProductCode).FirstOrDefault();

                    //add product to db, if not exist
                    if (dbProduct == null && (product.DiscountPercentage ?? 0) >= minDiscount)
                    {
                        Console.WriteLine("Found new product in wishlist");
                        var telegramClient = new HttpClient();
                        var message = HttpUtility.UrlEncode("Found new product in wishlist(" + product.Link + ")");
                        telegramClient.PostAsync("https://api.telegram.org/bot324060916:AAEV0ETgaGgSt3IMZ9gpy8X5wWILQClnxHU/sendMessage?chat_id=339666943&text=" + message, null).Wait();

                        product.ProductHistories.Add(new ProductHistory()
                        {
                            Availability = product.Availability,
                            DiscountPercentage = product.DiscountPercentage,
                            NormalPrice = product.NormalPrice,
                            ProductCode = product.ProductCode,
                            SalePrice = product.SalePrice,
                            SaleType = product.SaleType,
                            Timestamp = DateTime.Now
                        });

                        var entry = context.Products.Add(product);
                    }
                    else
                    {
                        //product exist, update history when something changed...
                        if ((product.DiscountPercentage ?? 0) >= minDiscount && dbProduct.HistorieChanged(product))
                        {
                            dbProduct.ProductHistories.Add(new ProductHistory()
                            {
                                Availability = product.Availability,
                                DiscountPercentage = product.DiscountPercentage,
                                NormalPrice = product.NormalPrice,
                                ProductCode = product.ProductCode,
                                SalePrice = product.SalePrice,
                                SaleType = product.SaleType,
                                Timestamp = DateTime.Now
                            });
                        }
                    }
                }
                context.SaveChanges();
            }
            return true;
        }
    }
}
