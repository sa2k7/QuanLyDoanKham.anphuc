using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuanLyDoanKham.API.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. HTTP Client & AI Services
builder.Services.AddHttpClient();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.IGeminiService, QuanLyDoanKham.API.Services.GeminiService>();

// 3. Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// 3. Swagger với bảo mật JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "QuanLyDoanKham API", Version = "v1" });
    
    // Cấu hình Bearer Auth cho Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập Token của bạn. Định dạng: Bearer <token>"
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
            new string[] {}
        }
    });
});

// 4. Cấu hình CORS (Cho phép Vue.js gọi API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// 5. Cấu hình JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration.GetSection("AppSettings:Token").Value ?? "MySuperSecretKeyForGraduationProject2026!")),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration.GetSection("AppSettings:Issuer").Value ?? "QuanLyDoanKham",
            ValidateAudience = true,
            ValidAudience = builder.Configuration.GetSection("AppSettings:Audience").Value ?? "QuanLyDoanKham",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
            // BUG FIX NOTE: JWT handler tự động chấp nhận algorithm HmacSha512 từ token header.
            // Không cần khai báo ValidAlgorithms vì .NET 8 IdentityModel xử lý tự động.
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
// BUG FIX: Bật Swagger ở tất cả môi trường để tiện kiểm thử
app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseCors("AllowVueApp"); // Bật CORS
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
