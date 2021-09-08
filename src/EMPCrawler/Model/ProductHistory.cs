using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EMPCrawler.Model
{
    public class ProductHistory
    {
        public string ProductCode { get; set; }

        public decimal NormalPrice { get; set; }

        public SaleType? SaleType { get; set; } = Model.SaleType.None;

        public decimal? SalePrice { get; set; } = 0;
        public int? DiscountPercentage { get; set; } = 0;
        public ProductAvailability? Availability { get; set; }

        public DateTime Timestamp { get; set; }

        public virtual Product Product { get; set; }
    }
}
