using System;
using System.Collections.Generic;
using System.Text;

namespace EMPCrawler.Model
{
    public class Product
    {
        public int ProductCode { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal NormalPrice { get; set; }
        public decimal SalePrice { get; set; }
        public int DiscountPercentage { get; set; }
        public string Link { get; set; }
        public string Type { get; set; }
        public string AvailabilityString { get; set; }
        public ProductAvailability Availability { get; set; }
    }
}
