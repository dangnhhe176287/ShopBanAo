﻿using EcommerceBackend.DataAccess.Abstract.BlogAbstract;
using EcommerceBackend.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceBackend.DataAccess.Repository.BlogRepository
{
    public class BlogRepository : IBlogRepository
    {
        private readonly EcommerceDBContext _context;

        public BlogRepository(EcommerceDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Blog>> GetAllAsync() =>
            await _context.Blogs.Include(b => b.BlogCategory).ToListAsync();

        public async Task<Blog?> GetByIdAsync(int id) =>
            await _context.Blogs.Include(b => b.BlogCategory).FirstOrDefaultAsync(b => b.BlogId == id);

        public async Task AddAsync(Blog blog)
        {
            // Đảm bảo các trường mới được lưu
            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Blog blog)
        {
            // Đảm bảo các trường mới được cập nhật
            _context.Blogs.Update(blog);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var blog = await GetByIdAsync(id);
            if (blog != null)
            {
                _context.Blogs.Remove(blog);
                await _context.SaveChangesAsync();
            }
        }

        public Task<Blog> CreateAsync(Blog blog)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Blog blog)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<Blog>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.Blogs
                .OrderByDescending(b => b.BlogId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task IncreaseViewCountAsync(int id)
        {
            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.BlogId == id);
            if (blog != null)
            {
                blog.ViewCount++;
                await _context.SaveChangesAsync();
            }
        }
    }
}
