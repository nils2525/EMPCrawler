﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EMPCrawler.Model
{
    public class ProductHistory
    {
        public int ProductCode { get; set; }

        public decimal NormalPrice { get; set; }

        public SaleType? SaleType { get; set; }

        public decimal? SalePrice { get; set; }
        public int? DiscountPercentage { get; set; }

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

        public DateTime Timestamp { get; set; }

        public virtual Product Product { get; set; }
    }
}