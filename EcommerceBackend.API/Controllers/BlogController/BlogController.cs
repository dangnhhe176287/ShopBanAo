﻿using EcommerceBackend.BusinessObject.Abstract.BlogAbstract;
using EcommerceBackend.BusinessObject.dtos.BlogDto;
using EcommerceBackend.BusinessObject.Services;
using EcommerceBackend.DataAccess.Abstract.BlogAbstract;
using EcommerceBackend.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceBackend.API.Controllers.BlogController
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly BlogService _service;

        public BlogController(BlogService service)
        {
            _service = service;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var blogs = await _service.GetAllAsync();
            return Ok(blogs);
        }

        [HttpGet("load")]
        public async Task<IActionResult> LoadBlogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var blogs = await _service.LoadBlogsAsync(page, pageSize);
            return Ok(blogs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var blog = await _service.GetByIdAsync(id);
            if (blog == null) return NotFound();
            return Ok(blog);
        }

        [HttpPost]
        public async Task<IActionResult> Create(BlogDto dto)
        {
            await _service.AddAsync(dto);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, BlogDto dto)
        {
            if (id != dto.BlogId) return BadRequest();
            await _service.UpdateAsync(dto);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] bool confirm = false)
        {
            if (!confirm) return BadRequest("Delete not confirmed");
            await _service.DeleteAsync(id);
            return Ok();
        }
    }
}
