using EMPCrawler.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMPCrawler
{
    public static class DBHelper
    {
        public static bool UpdateProducts(List<Product> products)
        {
            using (var context = new SQLiteContext())
            {                
                context.Database.EnsureCreated();

                foreach (var product in products)
                {
                    var dbProduct = context.Products.Include(nameof(Product.ProductHistories)).Where(p => p.ProductCode == product.ProductCode).FirstOrDefault();

                    //add product to db, if not exist
                    if (dbProduct == null)
                    {                        
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
                        if (dbProduct.HistorieChanged(product))
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
