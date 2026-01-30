using System;

namespace CodePulse.ProductService.Domain.Entities
{
    public class ProductReview
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        public Guid? UserId { get; set; } // Map to logic across microservice
        
        public int Rating { get; set; } // 1-5
        public string? Content { get; set; }
        public string? MediaUrls { get; set; } // JSON or array
        
        public bool IsAnonymous { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
