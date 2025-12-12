using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PortfolioBlog.Api.Data;
using System.Text;
using Microsoft.AspNetCore.Routing;

var builder = WebApplication.CreateBuilder(args);

// =====================
// Services
// =====================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PortfolioBlog.Api",
        Version = "v1"
    });

    // 🔐 JWT Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez : Bearer {votre token JWT}"
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

// =====================
// Database
// =====================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// =====================
// Identity
// =====================

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// =====================
// JWT Auth
// =====================

var jwtKey = builder.Configuration["Jwt:Key"] ?? "DEV_ONLY_CHANGE_ME_please_32_chars_min";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "PortfolioBlog";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "PortfolioBlog";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// =====================
// App
// =====================

var app = builder.Build();

// =====================
// Middleware
// =====================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PortfolioBlog.Api v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// =====================
// Seed
// =====================

await DbSeeder.SeedAsync(app.Services);

// =====================
// Debug (temporaire)
// =====================

app.MapGet("/debug/endpoints", (IEnumerable<EndpointDataSource> sources) =>
{
    var list = sources
        .SelectMany(s => s.Endpoints)
        .OfType<RouteEndpoint>()
        .Select(e => new
        {
            route = e.RoutePattern.RawText,
            methods = e.Metadata.OfType<HttpMethodMetadata>()
                .FirstOrDefault()?.HttpMethods
        });

    return Results.Ok(list);
});

// =====================
// Root → Swagger
// =====================

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
