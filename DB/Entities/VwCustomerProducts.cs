using System;
using System.Collections.Generic;

namespace DB.Entities
{
    public partial class VwCustomerProducts
    {
        public string CustomerId { get; set; }
        public string ProductName { get; set; }
        public string ShipCountry { get; set; }
        public decimal UnitPrice { get; set; }
        public short Quantity { get; set; }
        public decimal? Total { get; set; }
    }
}
