using MootechPic.API.Data;
using MootechPic.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MootechPic.API.Mappings;
using MootechPic.API.Profiles.Resolvers;
using System.Security.Claims;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Database connection string from environment variables
var host = builder.Configuration["DB_HOST"];
var port = builder.Configuration["DB_PORT"];
var dbName = builder.Configuration["DB_NAME"];
var dbUser = builder.Configuration["DB_USER"];
var dbPass = builder.Configuration["DB_PASS"];
var connectionString = $"Host={host};Port={port};Database={dbName};Username={dbUser};Password={dbPass};sslmode=require";

// Configure Entity Framework Core with Npgsql
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
           .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, LogLevel.Information)
);

// Add application services
builder.Services.AddSingleton<TokenService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
builder.Services.AddTransient<CartItemDetailsResolver>();
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var cloudinaryAccount = new CloudinaryDotNet.Account(
        config["Cloudinary:CloudName"],
        config["Cloudinary:ApiKey"],
        config["Cloudinary:ApiSecret"]
    );
    return new CloudinaryDotNet.Cloudinary(cloudinaryAccount);
});

// JWT authentication setup
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key is missing in configuration.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey))
    };
});

// CORS policy - allow local dev servers and Expo
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",  // Vite web
                "http://localhost:8081",  // Expo Metro
                "http://localhost:19006",  // Expo web
                "https://mootech-pic-admin-panel.vercel.app"
             )
             .AllowAnyHeader()
             .AllowAnyMethod();
    });
});

// Configure Kestrel to listen on port 80 for Fly.io
builder.WebHost.UseUrls("http://0.0.0.0:80");
var app = builder.Build();

// Enable Swagger UI in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware pipeline
app.UseStaticFiles();
app.UseCors("AllowLocalFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Listen on port 80 for Fly.io
app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:80");

await app.RunAsync();
