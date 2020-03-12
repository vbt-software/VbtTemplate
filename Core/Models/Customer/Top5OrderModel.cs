using Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB.Entities
{
    public class Top5OrderModel : BaseEntity
    {
        public string CustomerId { get; set; }
        public string CompanyName { get; set; }
        public string ShipCountry { get; set; }
        public int Total { get; set; }
    }
}
