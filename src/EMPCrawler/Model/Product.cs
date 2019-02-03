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
            var lastHistoryEntry = ProductHistories.Last();
            if(lastHistoryEntry == null ||
                lastHistoryEntry.Availability != product.Availability ||
                lastHistoryEntry.DiscountPercentage != product.DiscountPercentage ||
                lastHistoryEntry.NormalPrice != product.NormalPrice ||
                lastHistoryEntry.SalePrice != product.SalePrice ||
                lastHistoryEntry.SaleType != lastHistoryEntry.SaleType)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
