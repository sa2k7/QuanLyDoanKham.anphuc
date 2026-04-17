using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Services.Auth;
using QuanLyDoanKham.API.Services.MedicalGroups;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

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
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMedicalGroupAutoAssignmentService, MedicalGroupAutoAssignmentService>();
builder.Services.AddScoped<IGroupLifecycleService, GroupLifecycleService>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.IReportingService, QuanLyDoanKham.API.Services.ReportingService>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.IReportingServiceEnhanced, QuanLyDoanKham.API.Services.ReportingServiceEnhanced>();

builder.Services.AddHttpContextAccessor(); // Bắt buộc để lấy IP và Username
builder.Services.AddScoped<QuanLyDoanKham.API.Services.Auth.IAuditLogService, QuanLyDoanKham.API.Services.Auth.AuditLogService>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.MedicalRecords.ISmartQueueService, QuanLyDoanKham.API.Services.MedicalRecords.SmartQueueService>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.QrService>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.TimeSheetService>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.Reports.FinancialReportService>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.Reports.IReportExportService, QuanLyDoanKham.API.Services.Reports.ReportExportService>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.Settlement.ICostCalculationService, QuanLyDoanKham.API.Services.Settlement.CostCalculationService>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.Settlement.ISettlementService, QuanLyDoanKham.API.Services.Settlement.SettlementService>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.Settlement.IFinancialCalculatorEngine, QuanLyDoanKham.API.Services.Settlement.FinancialCalculatorEngine>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.MedicalRecords.IMedicalRecordService, QuanLyDoanKham.API.Services.MedicalRecords.MedicalRecordService>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.MedicalRecords.IMedicalRecordStateMachine, QuanLyDoanKham.API.Services.MedicalRecords.MedicalRecordStateMachine>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.MedicalRecords.ICheckInService, QuanLyDoanKham.API.Services.MedicalRecords.CheckInService>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.MedicalRecords.IMedicalReportPdfGenerator, QuanLyDoanKham.API.Services.MedicalRecords.MedicalReportPdfGenerator>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.MedicalRecords.IExamService, QuanLyDoanKham.API.Services.MedicalRecords.ExamService>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.PayrollService>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.Contracts.IHealthContractService, QuanLyDoanKham.API.Services.Contracts.HealthContractService>();
builder.Services.AddScoped<QuanLyDoanKham.API.Services.IGroupAnalyticsService, QuanLyDoanKham.API.Services.GroupAnalyticsService>();

builder.Services.AddSignalR();

// ================================================================
// 3. PERMISSION AUTHORIZATION
// ================================================================
// Đăng ký handler granular permission
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, PermissionHandler>();

builder.Services.AddAuthorization(options =>
{
    foreach (var key in PermissionConstants.All)
    {
        options.AddPolicy(PermissionConstants.PolicyPrefix + key,
            policy => policy.Requirements.Add(new PermissionRequirement(key)));
    }

    options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());

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
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token. Bắt buộc có chữ Bearer ở đầu. Ví dụ: Bearer eyJhbG..."
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
        .SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost" || new Uri(origin).Host == "127.0.0.1")
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
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Append("IS-TOKEN-EXPIRED", "true");
                }
                Console.WriteLine($"[AUTH ERROR] Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"[AUTH ERROR] OnChallenge triggered: {context.Error}, {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

// ================================================================
// BUILD
// ================================================================
var app = builder.Build();

// Middleware log lỗi chưa xử lý (unhandled exceptions) để debug 500
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[UNHANDLED ERROR] {context.Request.Path} -> {ex}");
        throw;
    }
});

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
app.MapHub<QuanLyDoanKham.API.Hubs.QueueHub>("/hub/queue");

app.Run();
