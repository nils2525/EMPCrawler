using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMPCrawler.Model.Configuration
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.ProductCode);

            builder.Property(p => p.ProductCode)
                .IsRequired();

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Brand)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Type)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Link)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.ImageUrl)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Ignore(p => p.AvailabilityString);
            builder.Ignore(p => p.Availability);
            builder.Ignore(p => p.NormalPrice);
            builder.Ignore(p => p.SalePrice);
            builder.Ignore(p => p.DiscountPercentage);
            builder.Ignore(p => p.SaleType);

            builder.HasMany(p => p.ProductHistories).WithOne(p => p.Product).HasForeignKey(k => k.ProductCode);
        }
    }
}
