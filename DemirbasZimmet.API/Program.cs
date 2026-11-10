
using DemirbasZimmet.API.Services.Interfaces;

using DemirbasZimmet.API.Services.Implementations;
using DemirbasZimmet.API.Data;
using DemirbasZimmet.API.Middleware;
using DemirbasZimmet.API.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using Microsoft.OpenApi.Models;
using DemirbasZimmet.API.Services;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("https://localhost:7289")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Controller desteði
builder.Services.AddControllers();

// Servisler
builder.Services.AddScoped<IPersonelService, PersonelService>();
builder.Services.AddScoped<IDemirbasService, DemirbasService>();
builder.Services.AddScoped<IZimmetService, ZimmetService>();
builder.Services.AddScoped<IBakimService, BakimService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<BildirimService>();
// Parola servisi
builder.Services.AddScoped<IParolaService, ParolaService>();
builder.Services.AddScoped<IPasswordHasher<Kullanici>, PasswordHasher<Kullanici>>();

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token giriniz."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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


// Veritabaný baðlantýsý
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// JWT ayarlarý
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT anahtarý yapýlandýrýlmamýþ.");

var key = Convert.FromBase64String(jwtKey);

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

        IssuerSigningKey = new SymmetricSecurityKey(key),

        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Uygulama
var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Development ortamýnda HTTP kullanýyoruz.
// Production ortamýnda HTTPS yönlendirmesi aktif kalacak.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("FrontendPolicy");

// Kimlik doðrulama ve yetkilendirme
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
