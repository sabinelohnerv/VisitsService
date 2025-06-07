using Cassandra.Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using VisitService.API.Repositories;
using VisitService.API.Services;
using VisitService.API.Validators;
using FluentValidation.AspNetCore;

using Microsoft.IdentityModel.Tokens;
using System.Text;

using UsersService.Config;
using UsersService.Services;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// ==== Configuración de Cassandra ====
builder.Services.Configure<CassandraOptions>(
    builder.Configuration.GetSection("Cassandra"));

builder.Services.AddSingleton<CassandraSessionFactory>();

// ==== Repositorios y Servicios ====
builder.Services.AddScoped<VisitRepository>();
builder.Services.AddScoped<VisitService.API.Services.VisitService>();

var jwtConfig = builder.Configuration.GetSection("Jwt");
var secretKey = jwtConfig.GetValue<string>("Key")
    ?? throw new InvalidOperationException("JWT secret key is missing in configuration.");


// ==== JWT Token ====

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

        ValidIssuer = jwtConfig["Issuer"],
        ValidAudience = jwtConfig["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// ==== FluentValidation ====
builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateVisitRequestValidator>();


// ==== Swagger ====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Visit Service API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresa tu token JWT en este formato: **Bearer &lt;tu_token&gt;**"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


var app = builder.Build();

// ==== Middleware ====
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
