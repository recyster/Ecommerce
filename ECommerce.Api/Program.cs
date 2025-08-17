using ECommerce.Infrastructure.Caching;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Messaging;
using ECommerce.Shared.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args); 
 
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Async(a => a.Console())
    .WriteTo.Async(a => a.File("Logs/log-.txt", rollingInterval: RollingInterval.Day))
    .WriteTo.Seq(builder.Configuration["Seq:Url"] ?? "http://localhost:5341")
    .MinimumLevel.Information()
    .CreateLogger();
builder.Host.UseSerilog(); 

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("ECommerce.Infrastructure")));

 
builder.Services.AddSingleton<IRabbitMqPublisher>(sp =>
    new RabbitMqPublisher(builder.Configuration["RabbitMq:HostName"] ?? "localhost"));
builder.Services.AddSingleton<IRedisService>(sp =>
    new RedisService(builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379"));

 
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
var info = new OpenApiInfo()
{
    Title = "Sipariþ API",
    Version = "v1",
    Description = "Bu API, siparis islemi simulasyonu icin gelistirilmistir.",
    Contact = new OpenApiContact()
    {
        Name = "Recep Kýlýç",
        Email = "recepkilic@outlook.com",
    }
};
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", info); 
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    }); 
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        } 
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
