using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using StudyPlatformAPI.Data;
using StudyPlatformAPI.Services;
using System.Text;
using StudyPlatformAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IEmailService, EmailService>();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Bileti kimin daðýttýðýný kontrol et
            ValidateAudience = true, // Biletin hangi platforma ait olduðunu kontrol et
            ValidateLifetime = true, // Biletin süresi (1 ay) dolmuþ mu kontrol et
            ValidateIssuerSigningKey = true, // Biletin üzerindeki kriptolu mühürü kontrol et
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IPomodoroService, PomodoroService>();
builder.Services.AddScoped<IKanbanService, KanbanService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddControllers();
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


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
