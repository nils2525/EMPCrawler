using EMPCrawler.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EMPCrawler.Model
{
    public class Product
    {
        public string ProductCode { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal NormalPrice { get; set; }
        public SaleType? SaleType { get; set; } = Model.SaleType.None;
        public decimal? SalePrice { get; set; } = 0;
        public int? DiscountPercentage { get; set; } = 0;
        public string Link => !String.IsNullOrWhiteSpace(ProductCode) ? $"https://www.emp.de/p/{ProductCode}.html" : null; 
        public string Type { get; set; }
        public string ImageUrl { get; set; }
        public ProductAvailability? Availability { get; set; }

        public virtual List<ProductHistory> ProductHistories { get; set; } = new List<ProductHistory>();

        public bool HistorieChanged(Product product)
        {
            var result = false;
            var lastHistoryEntry = ProductHistories.Last();

            var telegramClient = new HttpClient();

            var key = Data.ConfigService.Instance.Telegram.Apikey;
            var chat = Data.ConfigService.Instance.Telegram.ChatId;

            if (lastHistoryEntry == null)
            {
                //No entry found in history
                result = true;
            }
            else if (lastHistoryEntry.Availability != product.Availability)
            {
                var message = product.Name + " (" + product.ProductCode + ") - " + nameof(Availability) + " changed from " + lastHistoryEntry.Availability + " to " + product.Availability + " (" + product.Link + ")";
                Console.WriteLine(message);
                message = HttpUtility.UrlEncode(message);
                telegramClient.PostAsync("https://api.telegram.org/" + key + "/sendMessage?chat_id=" + chat + "&text=" + message, null).Wait();
                result = true;
            }
            else if (lastHistoryEntry.DiscountPercentage != product.DiscountPercentage)
            {
                var message = product.Name + " (" + product.ProductCode + ") - " + nameof(DiscountPercentage) + " changed from " + lastHistoryEntry.DiscountPercentage + " to " + product.DiscountPercentage + " (" + product.Link + ")";
                Console.WriteLine(message);
                message = HttpUtility.UrlEncode(message);
                telegramClient.PostAsync("https://api.telegram.org/" + key + "/sendMessage?chat_id=" + chat + "&text=" + message, null).Wait();
                result = true;
            }
            else if (lastHistoryEntry.NormalPrice != product.NormalPrice)
            {
                var message = product.Name + " (" + product.ProductCode + ") - " + nameof(NormalPrice) + " changed from " + lastHistoryEntry.NormalPrice + " to " + product.NormalPrice + " (" + product.Link + ")";
                Console.WriteLine(message);
                message = HttpUtility.UrlEncode(message);
                telegramClient.PostAsync("https://api.telegram.org/" + key + "/sendMessage?chat_id=" + chat + "&text=" + message, null).Wait();
                result = true;
            }
            else if (lastHistoryEntry.SalePrice != product.SalePrice)
            {
                var message = product.Name + " (" + product.ProductCode + ") - " + nameof(SalePrice) + " changed from " + lastHistoryEntry.SalePrice + " to " + product.SalePrice + " (" + product.Link + ")";
                Console.WriteLine(message);
                message = HttpUtility.UrlEncode(message);
                telegramClient.PostAsync("https://api.telegram.org/" + key + "/sendMessage?chat_id=" + chat + "&text=" + message, null).Wait();
                result = true;
            }
            else if (lastHistoryEntry.SaleType != lastHistoryEntry.SaleType)
            {
                var message = product.Name + " (" + product.ProductCode + ") - " + nameof(SaleType) + " changed from " + lastHistoryEntry.SaleType + " to " + product.SaleType + " (" + product.Link + ")";
                Console.WriteLine(message);
                message = HttpUtility.UrlEncode(message);
                telegramClient.PostAsync("https://api.telegram.org/" + key + "/sendMessage?chat_id=" + chat + "&text=" + message, null).Wait();
                result = true;
            }

            return result;
        }
    }
}
