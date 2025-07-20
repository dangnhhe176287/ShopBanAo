﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using PdfSharp.Fonts;
using EcommerceBackend.API.Configurations;
using EcommerceBackend.API.Hubs;
using EcommerceBackend.BusinessObject.Services;


using EcommerceBackend.DataAccess.Repository.SaleRepository;

using EcommerceBackend.DataAccess.Abstract.BlogAbstract;
using EcommerceBackend.DataAccess.Repository.BlogRepository;


using EcommerceBackend.BusinessObject.Services.UserService;
using EcommerceBackend.DataAccess.Repository.UserRepository;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;

using EcommerceBackend.DataAccess.Repository;
using EcommerceBackend.DataAccess.Models;

using EcommerceBackend.BusinessObject.Services.SaleService;
using EcommerceBackend.DataAccess.Abstract;
using EcommerceBackend.DataAccess.Repository;
using EcommerceBackend.BusinessObject.Abstract.AuthAbstract;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<BlogService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();


//thanhvv
builder.Services.AddDbContext<EcommerceDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<EcommerceBackend.DataAccess.Repository.IProductRepository, EcommerceBackend.DataAccess.Repository.ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

//builder.Services.AddScoped<IOrderRepository, OrderRepository>();
//builder.Services.AddScoped<IOrderService, OrderService>();



//builder.Services.AddScoped<ISaleProductService, SaleProductService>();
builder.Services.AddScoped<ISaleService, SaleService>(); 
builder.Services.AddScoped<EcommerceBackend.DataAccess.Repository.SaleRepository.IProductRepository, EcommerceBackend.DataAccess.Repository.SaleRepository.ProductRepository>();
builder.Services.AddScoped<EcommerceBackend.BusinessObject.Services.SaleService.ICategoryService, EcommerceBackend.BusinessObject.Services.SaleService.CategoryService>();
builder.Services.AddScoped<EcommerceBackend.DataAccess.Repository.SaleRepository.ICategoryRepository, EcommerceBackend.DataAccess.Repository.SaleRepository.CategoryRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<EcommerceBackend.DataAccess.Repository.IReviewRepository, EcommerceBackend.DataAccess.Repository.ReviewRepository>();
builder.Services.AddScoped<EcommerceBackend.BusinessObject.Services.IReviewService, EcommerceBackend.BusinessObject.Services.ReviewService>();
builder.Services.AddScoped<EcommerceBackend.DataAccess.Repository.IRatingRepository, EcommerceBackend.DataAccess.Repository.RatingRepository>();
builder.Services.AddScoped<EcommerceBackend.BusinessObject.Services.IRatingService, EcommerceBackend.BusinessObject.Services.RatingService>();
builder.Services.AddMemoryCache();

// Config Authentication Jwt
JwtConfig.ConfigureJwtAuthentication(builder.Services, builder.Configuration);
JwtConfig.ConfigureSwagger(builder.Services);
// Add Application Services (custom config DI)
builder.Services.AddApplicationServices(builder.Configuration);

//session
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["GoogleKeys:ClientId"];
    options.ClientSecret = builder.Configuration["GoogleKeys:ClientSecret"];
});

builder.Services.AddAuthorization();

//builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<SignalrHub>("/SignalrHub");  
 
app.UseSwagger();
app.UseSwaggerUI();
app.UseSession();

//app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontendApp");
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

app.Run();