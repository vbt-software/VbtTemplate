using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class CustomerListModel : BaseModel
    {
        public string CustomerID { get; set; }
        public string ProductName { get; set; }
        public string ShipCountry { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
        public bool IsDeleted { get; set; }

        public CustomerListModel() { }
    }
}
