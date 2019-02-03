using EMPCrawler.Model.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMPCrawler.Model
{
    public class SQLiteContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=database.db");            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
            modelBuilder.ApplyConfiguration(new ProductHistoryConfiguration());
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductHistory> ProductHistories { get; set; }
    }
}
