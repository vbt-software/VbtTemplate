using DB.Entities;
using DB.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB.Entities
{
    /*Run This Command :
     * dotnet ef dbcontext scaffold "Server=localhost\SQLEXPRESS;Database=Northwind;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer --output-dir Entities --force*/
    public class VbtContext : NorthwindContext
    {
        public VbtContext()
        {
        }

        public VbtContext(DbContextOptions<NorthwindContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddGlobalFilter();
        } 
    }
}
