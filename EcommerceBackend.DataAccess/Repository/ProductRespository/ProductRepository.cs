using DocumentFormat.OpenXml.InkML;
using EcommerceBackend.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EcommerceBackend.DataAccess.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly EcommerceDBContext _context;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(EcommerceDBContext context, ILogger<ProductRepository> logger)
        {
            _context = context;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<List<Product>> GetAllProductsAsync(int page, int pageSize)
        {
            return await _context.Products
                .Where(p => !p.IsDelete)
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductImages)
                .Include(p => p.Variants)
                .OrderByDescending(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductImages)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.ProductId == productId && !p.IsDelete);
        }

        public async Task<List<Product>> SearchProductsAsync(
      string? name = null,
      string? category = null,
      Dictionary<string, string>? attributes = null,
      decimal? minPrice = null,
      decimal? maxPrice = null,
      int page = 1,
      int pageSize = 10)
        {
            var query = _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.Variants)
                .Where(p => !p.IsDelete);

            // Lọc theo tên sản phẩm
            if (!string.IsNullOrEmpty(name))
            {
                var lowerName = name.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(lowerName));
            }

            // Lọc theo danh mục
            if (!string.IsNullOrEmpty(category))
            {
                var lowerCategory = category.ToLower();
                query = query.Where(p => p.ProductCategory != null &&
                    p.ProductCategory.ProductCategoryTitle.ToLower().Contains(lowerCategory));
            }

            // Lọc theo giá
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice != null && p.BasePrice >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice != null && p.BasePrice <= maxPrice.Value);
            }

          
            query = query.OrderByDescending(p => p.ProductId)
                         .Skip((page - 1) * pageSize)
                         .Take(pageSize);

           
            var products = await query.ToListAsync();

            // Lọc theo attributes nếu có
            //if (attributes != null && attributes.Any())
            //{
            //    products = products.Where(p => p.Variants.Any(variant =>
            //    {
            //        try
            //        {
            //            if (string.IsNullOrEmpty(variant.Attributes)) return false;
            //            var variantAttributes = JsonSerializer.Deserialize<Dictionary<string, string>>(variant.Attributes);
            //            return attributes.All(attr =>
            //            {
            //                if (!variantAttributes.TryGetValue(attr.Key, out var value))
            //                {
            //                    // Nếu khóa không tồn tại, chấp nhận nếu giá trị trong attributes là null hoặc rỗng
            //                    return false;
            //                }
            //                // So sánh giá trị, chấp nhận nếu cả hai đều null hoặc giá trị khớp
            //                return value == attr.Value || (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(attr.Value));
            //            });
            //        }
            //        catch (JsonException)
            //        {
            //            return false;
            //        }
            //    })).ToList();
            //}

            return products;
        }

        // Hàm kiểm tra JSON hợp lệ
        private bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput))
                return false;
            try
            {
                System.Text.Json.JsonDocument.Parse(strInput);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Helper method để deserialize variant values một cách an toàn
        private List<Dictionary<string, object>> DeserializeVariantValues(string? jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString) || !IsValidJson(jsonString))
                return new List<Dictionary<string, object>>();

            try
            {
                return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonString, _jsonOptions) 
                    ?? new List<Dictionary<string, object>>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize variant values: {JsonString}", jsonString);
                return new List<Dictionary<string, object>>();
            }
        }

        // Helper method để serialize variant values
        private string SerializeVariantValues(List<Dictionary<string, object>> variantValues)
        {
            try
            {
                return JsonSerializer.Serialize(variantValues, _jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to serialize variant values");
                return "[]";
            }
        }

        public async Task<bool> UpdateProductAttributesAsync(int productId, string availableAttributes)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) 
                {
                    _logger.LogWarning("Product not found: ProductId={ProductId}", productId);
                    return false;
                }

                product.AvailableAttributes = availableAttributes;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully updated product attributes: ProductId={ProductId}", productId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product attributes: ProductId={ProductId}", productId);
                return false;
            }
        }

        // So sánh toàn bộ key-value giữa hai biến thể
        private bool IsDuplicateVariant(Dictionary<string, object> a, Dictionary<string, object> b)
        {
            if (a.Count != b.Count) return false;
            foreach (var key in a.Keys)
            {
                if (!b.ContainsKey(key) || (a[key]?.ToString() ?? "") != (b[key]?.ToString() ?? ""))
                    return false;
            }
            return true;
        }

        public async Task<bool> AddProductVariantAsync(ProductVariant variant)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Variants)
                    .FirstOrDefaultAsync(p => p.ProductId == variant.ProductId && !p.IsDelete);

                if (product == null) 
                {
                    _logger.LogWarning("Product not found for variant: ProductId={ProductId}", variant.ProductId);
                    return false;
                }

                // Get current variants
                var variants = product.Variants?.ToList() ?? new List<ProductVariant>();

                // Deserialize biến thể mới
                var newVariantDict = DeserializeVariantValues(variant.Attributes);

                // Check duplicate: so sánh toàn bộ key-value
                bool isDuplicate = variants.Any(existing =>
                {
                    var existingDict = DeserializeVariantValues(existing.Attributes);
                    return existingDict.Any(existingVariant => IsDuplicateVariant(existingVariant, newVariantDict.FirstOrDefault() ?? new()));
                });
                
                if (isDuplicate)
                {
                    _logger.LogWarning("Duplicate variant detected: {Attributes}", variant.Attributes);
                    return false;
                }

                // Thêm biến thể mới
                variants.Add(variant);
                product.Variants = variants;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully added variant for product: ProductId={ProductId}, VariantId={VariantId}", 
                    variant.ProductId, variant.VariantId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product variant: ProductId={ProductId}", variant.ProductId);
                return false;
            }
        }

        public async Task<bool> UpdateProductVariantAsync(ProductVariant variant, bool skipValidation = false)
        {
            try
            {
                var existingVariant = await _context.ProductVariants
                    .Include(v => v.Product)
                    .FirstOrDefaultAsync(v => v.VariantId == variant.VariantId);

                if (existingVariant == null) 
                {
                    _logger.LogWarning("Variant not found: VariantId={VariantId}", variant.VariantId);
                    return false;
                }

                // Cập nhật thông tin cơ bản của variant
                existingVariant.Attributes = variant.Attributes;
                // Parse price và stock từ variant.Variants (JSON)
                var variantList = DeserializeVariantValues(variant.Variants);
                if (variantList != null && variantList.Count > 0)
                {
                    var firstVariant = variantList[0];
                    var price = firstVariant.ContainsKey("price") ? Convert.ToDecimal(firstVariant["price"]) : 0;
                    var stock = firstVariant.ContainsKey("stock") ? Convert.ToInt32(firstVariant["stock"]) : 0;
                    // Nếu cần sử dụng price/stock, dùng biến này
                }
                // Không còn truy cập existingVariant.Price hay existingVariant.StockQuantity
                existingVariant.Variants = variant.Variants;
                existingVariant.UpdatedAt = DateTime.UtcNow;
                
                if (existingVariant.Product != null)
                {
                    existingVariant.Product.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully updated variant: VariantId={VariantId}", variant.VariantId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product variant: VariantId={VariantId}", variant.VariantId);
                return false;
            }
        }

        public async Task<bool> DeleteProductVariantAsync(int variantId)
        {
            try
            {
                var variant = await _context.ProductVariants.FindAsync(variantId);
                if (variant == null) 
                {
                    _logger.LogWarning("Variant not found for deletion: VariantId={VariantId}", variantId);
                    return false;
                }

                _context.ProductVariants.Remove(variant);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully deleted variant: VariantId={VariantId}", variantId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product variant: VariantId={VariantId}", variantId);
                return false;
            }
        }

        public async Task<bool> AddProductAttributeAsync(int productId, string attributeName, List<string> attributeValues)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) return false;

                var availableAttributes = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(
                    product.AvailableAttributes, _jsonOptions) ?? new Dictionary<string, List<string>>();

                if (availableAttributes.ContainsKey(attributeName))
                {
                    return false; // Attribute already exists
                }

                availableAttributes[attributeName] = attributeValues;
                product.AvailableAttributes = JsonSerializer.Serialize(availableAttributes, _jsonOptions);
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateProductAttributeAsync(int productId, string attributeName, List<string> attributeValues)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) return false;

                // var product = await _context.Product.Find(productId)
                // if product == null, return fasle;
                // var availableatt = jsonSerial.Deserial Dictionary, sitrng, list string
                // availabelatt[attributeName] = List
                // context.savechagne
                var availableAttributes = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(
                    product.AvailableAttributes, _jsonOptions) ?? new Dictionary<string, List<string>>();

                if (!availableAttributes.ContainsKey(attributeName))
                {
                    return false; // Attribute doesn't exist
                }

                availableAttributes[attributeName] = attributeValues;
                product.AvailableAttributes = JsonSerializer.Serialize(availableAttributes, _jsonOptions);
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteProductAttributeAsync(int productId, string attributeName)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) return false;

                var availableAttributes = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(
                    product.AvailableAttributes, _jsonOptions) ?? new Dictionary<string, List<string>>();

                if (!availableAttributes.ContainsKey(attributeName))
                {
                    return false; // Attribute doesn't exist
                }

                availableAttributes.Remove(attributeName);
                product.AvailableAttributes = JsonSerializer.Serialize(availableAttributes, _jsonOptions);
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Dictionary<string, List<string>>> GetProductAttributesAsync(int productId)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) return new Dictionary<string, List<string>>();

                return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(
                    product.AvailableAttributes, _jsonOptions) ?? new Dictionary<string, List<string>>();
            }
            catch
            {
                return new Dictionary<string, List<string>>();
            }
        }

        public async Task<bool> AddVariantValueAsync(int variantId, Dictionary<string, object> variantValue)
        {
            try
            {
                var variant = await _context.ProductVariants
                    .Include(v => v.Product)
                    .FirstOrDefaultAsync(v => v.VariantId == variantId);

                if (variant == null) 
                {
                    _logger.LogWarning("Variant not found: VariantId={VariantId}", variantId);
                    return false;
                }

                // Get current variant values
                var variants = DeserializeVariantValues(variant.Variants);

                // Check for duplicate
                if (variants.Any(v => IsDuplicateVariant(v, variantValue)))
                {
                    _logger.LogWarning("Duplicate variant value detected: VariantId={VariantId}", variantId);
                    return false;
                }

                // Add new variant value
                variants.Add(variantValue);
                variant.Variants = SerializeVariantValues(variants);
                variant.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully added variant value: VariantId={VariantId}", variantId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding variant value: VariantId={VariantId}", variantId);
                return false;
            }
        }

        public async Task<bool> UpdateVariantValueAsync(int variantId, int valueIndex, Dictionary<string, object> variantValue)
        {
            try
            {
                var variant = await _context.ProductVariants
                    .Include(v => v.Product)
                    .FirstOrDefaultAsync(v => v.VariantId == variantId);

                if (variant == null) 
                {
                    _logger.LogWarning("Variant not found: VariantId={VariantId}", variantId);
                    return false;
                }

                // Get current variant values
                var variants = DeserializeVariantValues(variant.Variants);

                if (valueIndex < 0 || valueIndex >= variants.Count)
                {
                    _logger.LogWarning("Invalid value index: VariantId={VariantId}, Index={Index}", variantId, valueIndex);
                    return false;
                }

                // Update variant value
                variants[valueIndex] = variantValue;
                variant.Variants = SerializeVariantValues(variants);
                variant.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully updated variant value: VariantId={VariantId}, Index={Index}", variantId, valueIndex);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating variant value: VariantId={VariantId}, Index={Index}", variantId, valueIndex);
                return false;
            }
        }

        public async Task<bool> DeleteVariantValueAsync(int variantId, int valueIndex)
        {
            try
            {
                var variant = await _context.ProductVariants.FindAsync(variantId);
                if (variant == null) 
                {
                    _logger.LogWarning("Variant not found: VariantId={VariantId}", variantId);
                    return false;
                }

                // Get current variant values
                var variants = DeserializeVariantValues(variant.Variants);

                if (valueIndex < 0 || valueIndex >= variants.Count)
                {
                    _logger.LogWarning("Invalid value index: VariantId={VariantId}, Index={Index}", variantId, valueIndex);
                    return false;
                }

                variants.RemoveAt(valueIndex);
                variant.Variants = SerializeVariantValues(variants);
                variant.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully deleted variant value: VariantId={VariantId}, Index={Index}", variantId, valueIndex);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting variant value: VariantId={VariantId}, Index={Index}", variantId, valueIndex);
                return false;
            }
        }

        public async Task<List<Dictionary<string, object>>> GetVariantValuesAsync(int variantId)
        {
            try
            {
                var variant = await _context.ProductVariants.FindAsync(variantId);
                if (variant == null) 
                {
                    _logger.LogWarning("Variant not found: VariantId={VariantId}", variantId);
                    return new List<Dictionary<string, object>>();
                }

                return DeserializeVariantValues(variant.Variants);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting variant values: VariantId={VariantId}", variantId);
                return new List<Dictionary<string, object>>();
            }
        }

        public async Task<ProductVariant?> GetProductVariantByIdAsync(int variantId)
        {
            return await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.VariantId == variantId);
        }

        public async Task<int> GetTotalProductsCountAsync(
            string? name = null,
            string? category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null)
        {
            var query = _context.Products.Where(p => !p.IsDelete);

            if (!string.IsNullOrEmpty(name))
                query = query.Where(p => p.Name.Contains(name));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(p => p.ProductCategory.ProductCategoryTitle.Contains(category));

            if (minPrice.HasValue)
                query = query.Where(p => p.BasePrice >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.BasePrice <= maxPrice.Value);

            return await query.CountAsync();
        }

        public async Task<bool> CreateProductAsync(Product product)
        {
            try
            {
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            try
            {
                var existing = await _context.Products.FindAsync(product.ProductId);
                if (existing == null) return false;
                existing.Name = product.Name;
                existing.Description = product.Description;
                existing.Brand = product.Brand;
                existing.BasePrice = product.BasePrice;
                existing.AvailableAttributes = product.AvailableAttributes;
                existing.ProductCategoryId = product.ProductCategoryId;
                existing.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            try
            {
                var existing = await _context.Products.FindAsync(productId);
                if (existing == null) return false;
                existing.IsDelete = true;
                existing.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<ProductVariant>> GetVariantsByProductIdAsync(int productId)
        {
            return await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .ToListAsync();
        }

        public async Task<bool> UpdateProductVariantAsync(ProductVariant variant)
        {
            return await UpdateProductVariantAsync(variant, false);
        }
    }
}