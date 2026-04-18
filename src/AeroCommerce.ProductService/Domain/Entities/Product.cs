using System;
using System.Collections.Generic;

namespace AeroCommerce.ProductService.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        
        public string? WholesalePricing { get; set; } 
        public string? OriginLocation { get; set; }
        public int SoldCount { get; set; } = 0;
        
        public string? ThumbnailUrl { get; set; }
        public string? MediaUrls { get; set; } // Can be JSON string or array of strings mapped properly

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    }
}
