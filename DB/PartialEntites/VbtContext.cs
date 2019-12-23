using DB.Entities;
using DB.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DB.Entities
{
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
