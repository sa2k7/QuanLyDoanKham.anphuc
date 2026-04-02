using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Middleware;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Services.Auth;
using QuanLyDoanKham.API.Services.MedicalGroups;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ================================================================
// 1. DATABASE
// ================================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ================================================================
// 2. SERVICES
// ================================================================
builder.Services.AddHttpClient();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.IGeminiService, QuanLyDoanKham.API.Services.GeminiService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMedicalGroupAutoAssignmentService, MedicalGroupAutoAssignmentService>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.IReportingService, QuanLyDoanKham.API.Services.ReportingService>();

// ================================================================
// 3. PERMISSION AUTHORIZATION
// ================================================================
// Đăng ký handler granular permission
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Danh sách tất cả permission keys - map mỗi key thành một Policy
var allPermissionKeys = new[]
{
    // HopDong
    "HopDong.View", "HopDong.Create", "HopDong.Edit", "HopDong.Approve", "HopDong.Reject", "HopDong.Upload",
    // DoanKham
    "DoanKham.View", "DoanKham.Create", "DoanKham.Edit", "DoanKham.SetPosition", "DoanKham.AssignStaff", "DoanKham.ManageOwn",
    // LichKham
    "LichKham.ViewOwn", "LichKham.ViewAll",
    // ChamCong
    "ChamCong.QR", "ChamCong.CheckInOut", "ChamCong.ViewAll",
    // Kho
    "Kho.View", "Kho.Import", "Kho.Export",
    // Luong
    "Luong.View", "Luong.Manage",
    // NhanSu
    "NhanSu.View", "NhanSu.Manage",
    // BaoCao
    "BaoCao.View", "BaoCao.Export",
    // HeThong
    "HeThong.UserManage", "HeThong.RoleManage"
};

builder.Services.AddAuthorization(options =>
{
    // Tạo policy cho mỗi permission key
    foreach (var key in allPermissionKeys)
    {
        options.AddPolicy(key, policy =>
            policy.Requirements.Add(new PermissionRequirement(key)));
    }

    // Policy tiện lợi: chỉ cần đăng nhập
    options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());

    // Policy mặc định: phải đăng nhập
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// ================================================================
// 4. RATE LIMITING
// ================================================================
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("LoginPolicy", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ================================================================
// 5. CONTROLLERS & JSON
// ================================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ================================================================
// 6. SWAGGER
// ================================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "QuanLyDoanKham API",
        Version = "v1",
        Description = "API Quản lý Đoàn Khám - Hệ thống toàn diện"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token. Định dạng: Bearer <token>"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ================================================================
// 7. CORS
// ================================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy => policy
            .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// ================================================================
// 8. JWT AUTHENTICATION
// ================================================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration.GetSection("AppSettings:Token").Value
                ?? throw new InvalidOperationException("CRITICAL: JWT Secret key is missing."))),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration.GetSection("AppSettings:Issuer").Value ?? "QuanLyDoanKham",
            ValidateAudience = true,
            ValidAudience = builder.Configuration.GetSection("AppSettings:Audience").Value ?? "QuanLyDoanKham",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// ================================================================
// BUILD
// ================================================================
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowVueApp");

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
