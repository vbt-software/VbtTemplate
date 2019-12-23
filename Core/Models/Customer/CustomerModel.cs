using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class CustomerModel : BaseModel
    {
        public int Id { get; set; }
        public string CustomerID { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string MobilePhone { get; set; }
        public virtual ICollection<OrderModel> Orders { get; set; }
        public CustomerModel() { }
    }
}
