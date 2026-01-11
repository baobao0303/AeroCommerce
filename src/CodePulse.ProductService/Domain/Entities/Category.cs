using System;
using System.Collections.Generic;

namespace CodePulse.ProductService.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        
        public Guid? ParentId { get; set; }
        public Category? Parent { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();
        
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        
        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    }
}
