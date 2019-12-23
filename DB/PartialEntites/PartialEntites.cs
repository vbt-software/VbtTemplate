using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core;
using DB.PartialEntites;

namespace DB.Entities
{
    class PartialEntites
    {
    }
    public partial class Categories : BaseEntity, ISoftDeletable { }
    public partial class Customers : BaseEntity, ISoftDeletable { }
    public partial class Employees : BaseEntity { }
    public partial class Territories : BaseEntity { }
    public partial class EmployeeTerritories : BaseEntity { }
    public partial class Region : BaseEntity { }
    public partial class Products : BaseEntity { }
    public partial class Orders : BaseEntity { }
    public partial class Products : BaseEntity { }

    #region Views 
    [Table("Sales by Category")]
    public class VwSalesbyCategory : BaseEntity
    {
        [Key]
        public int? CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string ProductName { get; set; }
        public decimal ProductSales { get; set; }

    }
    [Table("Sales Totals by Amount")]
    public class VwSalesTotalsbyAmount : BaseEntity
    {
        [Key]
        public int OrderID { get; set; }
        public decimal SaleAmount { get; set; }
        public string CompanyName { get; set; }
        public DateTime ShippedDate { get; set; }
    }

    public partial class VwCustomerProducts : BaseEntity { }

    //[Table("VwCustomerProducts")]
    //public class VwCustomerProducts : BaseEntity
    //{
    //    [Key]
    //    public string CustomerID { get; set; }
    //    public string ProductName { get; set; }
    //    public string ShipCountry { get; set; }
    //    public decimal UnitPrice { get; set; }
    //    public int Quantity { get; set; }
    //    public decimal Total { get; set; }
    //}
    #endregion
}
