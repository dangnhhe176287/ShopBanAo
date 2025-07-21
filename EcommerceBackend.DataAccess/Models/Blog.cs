using System;
using System.Collections.Generic;

namespace EcommerceBackend.DataAccess.Models
{
    public partial class Blog
    {
        public int BlogId { get; set; }
        public int? BlogCategoryId { get; set; }
        public string? BlogTittle { get; set; }
        public string? BlogContent { get; set; }
        public string? Summary { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Tags { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Status { get; set; }
        public int ViewCount { get; set; }

        public virtual BlogCategory? BlogCategory { get; set; }
    }
}
