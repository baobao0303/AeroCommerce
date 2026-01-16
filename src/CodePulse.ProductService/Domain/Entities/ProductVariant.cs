using System;

namespace CodePulse.ProductService.Domain.Entities
{
    public class ProductVariant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        public string SKU { get; set; } = null!;
        public string VariantName { get; set; } = null!;
        
        public decimal? PriceOverride { get; set; }
        public int StockQuantity { get; set; } = 0;
    }
}
