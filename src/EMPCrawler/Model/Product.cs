using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EMPCrawler.Model
{
    public class Product
    {
        public int ProductCode { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal NormalPrice { get; set; }
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
                if (_availabilityPairs.ContainsKey(value))
                {
                    Availability = _availabilityPairs[value];
                    _availabilityString = value;
                }
                else
                {
                    _availabilityString = null;
                    _availability = ProductAvailability.None;
                }
            }
        }

        private ProductAvailability _availability;
        public ProductAvailability Availability
        {
            get
            {
                return _availability;
            }
            set
            {
                _availability = value;
                _availabilityString = _availabilityPairs.Where(c => c.Value == value).First().Key;
            }
        }


        private static Dictionary<string, ProductAvailability> _availabilityPairs = new Dictionary<string, ProductAvailability>()
        {
            { "is-in-stock", ProductAvailability.InStock},
            { "on-order", ProductAvailability.OnOrder},
            {"notavailable", ProductAvailability.NotAvailable }
        };
    }
}
