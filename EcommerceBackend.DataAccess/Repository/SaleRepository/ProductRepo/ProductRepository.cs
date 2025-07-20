﻿using EcommerceBackend.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceBackend.DataAccess.Repository.SaleRepository.ProductRepo
{
    public class ProductRepository : IProductRepository
    {
        readonly EcommerceDBContext _context;
        public ProductRepository(EcommerceDBContext context)
        {
            _context = context;
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductImages)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.ProductId == id && !p.IsDelete && p.Status == 1);
            return product;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductImages)
                .Include(p => p.Variants)
                .ToListAsync();
        }
        public async Task AddProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsDelete = true;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
        public void UpdateProductImages(Product product, List<ProductImage> images)
        {
            _context.ProductImages.RemoveRange(product.ProductImages);
            if (images != null)
            {
                product.ProductImages = images;
                _context.ProductImages.AddRange(images);
            }
        }

        public void UpdateProductVariants(Product product, List<ProductVariant> variants)
        {
            _context.ProductVariants.RemoveRange(product.Variants);
            if (variants != null)
            {
                product.Variants = variants;
                _context.ProductVariants.AddRange(variants);
            }
        }

        public async Task<ProductVariant> GetProductVariantAsync(int productId, string variantId)
        {
            return await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.ProductId == productId && v.VariantId.ToString() == variantId);
        }
    }
}
