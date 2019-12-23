using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Filters.Customers
{
    public class CustomerFilter : BaseFilter
    {
        public string CustomerID { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string Country { get; set; }
    }
}
