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
        public int ProductCode { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal NormalPrice { get; set; }
        public SaleType? SaleType { get; set; }
        public decimal? SalePrice { get; set; }
        public int? DiscountPercentage { get; set; }
        public string Link { get; set; }
        public string Type { get; set; }
        public string ImageUrl { get; set; }

        private string _availabilityString;
        public string AvailabilityString
        {
            get
            {
                return _availabilityString;
            }
            set
            {
                if (ProductAvailabilityService.AvailabilityPairs.ContainsKey(value))
                {
                    Availability = ProductAvailabilityService.AvailabilityPairs[value];
                    _availabilityString = value;
                }
                else
                {
                    _availabilityString = null;
                    _availability = null;
                }
            }
        }

        private ProductAvailability? _availability;
        public ProductAvailability? Availability
        {
            get
            {
                return _availability;
            }
            set
            {
                _availability = value;

                if (value != null)
                {
                    _availabilityString = ProductAvailabilityService.AvailabilityPairs.Where(c => c.Value == value).First().Key;
                }
                else
                {
                    _availabilityString = null;
                }
            }
        }

        public virtual List<ProductHistory> ProductHistories { get; set; } = new List<ProductHistory>();

        public bool HistorieChanged(Product product)
        {
            var result = false;
            var lastHistoryEntry = ProductHistories.Last();

            var telegramClient = new HttpClient();

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
                telegramClient.PostAsync("https://api.telegram.org/" + Program.TelegramKey + "/sendMessage?chat_id=339666943&text=" + message, null).Wait();
                result = true;
            }
            else if (lastHistoryEntry.DiscountPercentage != product.DiscountPercentage)
            {
                var message = product.Name + " (" + product.ProductCode + ") - " + nameof(DiscountPercentage) + " changed from " + lastHistoryEntry.DiscountPercentage + " to " + product.DiscountPercentage + " (" + product.Link + ")";
                Console.WriteLine(message);
                message = HttpUtility.UrlEncode(message);
                telegramClient.PostAsync("https://api.telegram.org/" + Program.TelegramKey + "/sendMessage?chat_id=339666943&text=" + message, null).Wait();
                result = true;
            }
            else if (lastHistoryEntry.NormalPrice != product.NormalPrice)
            {
                var message = product.Name + " (" + product.ProductCode + ") - " + nameof(NormalPrice) + " changed from " + lastHistoryEntry.NormalPrice + " to " + product.NormalPrice + " (" + product.Link + ")";
                Console.WriteLine(message);
                message = HttpUtility.UrlEncode(message);
                telegramClient.PostAsync("https://api.telegram.org/" + Program.TelegramKey + "/sendMessage?chat_id=339666943&text=" + message, null).Wait();
                result = true;
            }
            else if (lastHistoryEntry.SalePrice != product.SalePrice)
            {
                var message = product.Name + " (" + product.ProductCode + ") - " + nameof(SalePrice) + " changed from " + lastHistoryEntry.SalePrice + " to " + product.SalePrice + " (" + product.Link + ")";
                Console.WriteLine(message);
                message = HttpUtility.UrlEncode(message);
                telegramClient.PostAsync("https://api.telegram.org/" + Program.TelegramKey + "/sendMessage?chat_id=339666943&text=" + message, null).Wait();
                result = true;
            }
            else if (lastHistoryEntry.SaleType != lastHistoryEntry.SaleType)
            {
                var message = product.Name + " (" + product.ProductCode + ") - " + nameof(SaleType) + " changed from " + lastHistoryEntry.SaleType + " to " + product.SaleType + " (" + product.Link + ")";
                Console.WriteLine(message);
                message = HttpUtility.UrlEncode(message);
                telegramClient.PostAsync("https://api.telegram.org/" + Program.TelegramKey + "/sendMessage?chat_id=339666943&text=" + message, null).Wait();
                result = true;
            }

            return result;
        }
    }
}
