using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMPCrawler.Model.Configuration
{
    public class ProductHistoryConfiguration : IEntityTypeConfiguration<ProductHistory>
    {
        public void Configure(EntityTypeBuilder<ProductHistory> builder)
        {
            builder.HasKey(p => new { p.ProductCode, p.Timestamp });

            builder.Property(p => p.ProductCode)
                .IsRequired();

            builder.Property(p => p.Availability)
                .HasMaxLength(25);

            builder.Property(p => p.NormalPrice)
                 .IsRequired();
            builder.Property(p => p.SalePrice)
                .IsRequired(false);
            builder.Property(p => p.DiscountPercentage)
                .IsRequired(false);
            builder.Property(p => p.SaleType)
                .IsRequired(false);

            builder.Property(p => p.Timestamp)
                .IsRequired(true);
        }
    }
}
