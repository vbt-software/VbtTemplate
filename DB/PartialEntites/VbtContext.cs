using DB.Entities;
using DB.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            base.OnConfiguring(optionsBuilder.UseLoggerFactory(VbtLoggerFactory));
#endif
#if RELEASE
            base.OnConfiguring(optionsBuilder);
#endif
        }
        public static readonly ILoggerFactory VbtLoggerFactory
         = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter((category, level) =>
                        category == DbLoggerCategory.Database.Command.Name
                        && level == LogLevel.Information)
                    .AddDebug();
            });
    }
}
