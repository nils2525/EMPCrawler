using EMPCrawler.Model.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMPCrawler.Model
{
    public class SQLiteContext : DbContext
    {
        private static bool _initialized;

        public SQLiteContext()
        {
            if (!_initialized)
            {
                ///Create database and tables if not exist
                Database.EnsureCreated();
                _initialized = true;
            }
        }

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
