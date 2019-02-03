using System.Collections.Generic;

namespace EMPCrawler.Model
{
    public enum ProductAvailability
    {
        InStock,
        OnOrder,
        NotAvailable
    }

    public static class ProductAvailabilityService
    {
        public static readonly Dictionary<string, ProductAvailability> AvailabilityPairs = new Dictionary<string, ProductAvailability>()
        {
            { "is-in-stock", ProductAvailability.InStock},
            { "on-order", ProductAvailability.OnOrder},
            {"notavailable", ProductAvailability.NotAvailable }
        };
    }
}
