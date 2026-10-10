using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;
using StudyPlatformAPI.Data;
using StudyPlatformAPI.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Veritabaný Baðlantýsý
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servis Baðýmlýlýklarý (Dependency Injection)
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IPomodoroService, PomodoroService>();
builder.Services.AddScoped<IKanbanService, KanbanService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// MinIO Konfigürasyonu (Swagger bloðunun dýþýna, ana servislere taþýndý)
builder.Services.AddMinio(options =>
{
    options.WithEndpoint(builder.Configuration["Minio:Endpoint"] ?? "minio:9000");
    options.WithCredentials(builder.Configuration["Minio:AccessKey"], builder.Configuration["Minio:SecretKey"]);
    options.WithSSL(false);
});

// JWT Kimlik Doðrulama Ayarlarý
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger Konfigürasyonu
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "StudyPlatformAPI", Version = "v1" });

    // Swagger'a JWT kullanacaðýný öðretiyoruz
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Token'ý buraya yapýþtýrýn."
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Docker'da sertifika olmadýðý için kapalý
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();