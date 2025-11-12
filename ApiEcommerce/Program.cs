using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ApiEcommerce.Repository.IRepository;
using ApiEcommerce.Repository;
using ApiEcommerce.Mapping;
using Mapster;
using MapsterMapper;
using ApiEcommerce.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.CodeAnalysis.Options;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ApiEcommerce.Models;
using ApiEcommerce.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("ConexionSql");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
  options.UseSqlServer(connectionString)
  .UseSeeding((context, _) =>
  {
      var appContext = (ApplicationDbContext)context;

      DataSeeder.SeedData(appContext);

  })
);

builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024; // 1 MB
    options.UseCaseSensitivePaths = true;
});

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Configuración de políticas de contraseña: ajustar según las reglas de seguridad de tu proyecto.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false; // Permitir contraseñas sin minúsculas
    options.Password.RequireUppercase = false; // Permitir contraseñas sin mayúsculas
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(jwtOptions =>
{
    jwtOptions.RequireHttpsMetadata = false;
    jwtOptions.SaveToken = true;
    jwtOptions.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("AppSettings:SecretKey")!)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddControllers(Options =>
{
    Options.CacheProfiles.Add(CacheProfiles.Default30, CacheProfiles.Default30Profile);
    Options.CacheProfiles.Add(CacheProfiles.Default20, CacheProfiles.Default20Profile);
});

// Configurar Mapster
var mapsterConfig = TypeAdapterConfig.GlobalSettings;
// Registrar mappings definidos en la carpeta Mapping
ApiEcommerce.Mapping.CategoryMapping.RegisterMappings(mapsterConfig);
ApiEcommerce.Mapping.ProductMapping.RegisterMappings(mapsterConfig);
ApiEcommerce.Mapping.UserMapping.RegisterMappings(mapsterConfig);

builder.Services.AddSingleton(mapsterConfig);
// Registrar MapsterMapper ServiceMapper como IMapper
builder.Services.AddScoped<MapsterMapper.IMapper, MapsterMapper.ServiceMapper>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// Registrar Api Explorer y Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nuestra API utiliza la Autenticación JWT usando el esquema Bearer. \n\r\n\r" +
                      "Ingresa la palabra a continuación el token generado en login.\n\r\n\r" +
                      "Ejemplo: \"12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "API Ecommerce V1",
        Description = "API Ecommerce para la gestión de productos y categorías. Versión 1",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Soporte API Ecommerce",
            Email = "soporte@example.com",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Licencia API Ecommerce",
            Url = new Uri("https://example.com/license")
        }
    });
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2",
        Title = "API Ecommerce V2",
        Description = "API Ecommerce para la gestión de productos y categorías. Versión 2",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Soporte API Ecommerce",
            Email = "soporte@example.com",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Licencia API Ecommerce",
            Url = new Uri("https://example.com/license")
        }
    });
});
var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    //options.ApiVersionReader = ApiVersionReader.Combine( new QueryStringApiVersionReader("api-version"));
});

apiVersioningBuilder.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(PolicyNames.AllowSpecificOrigin, policy =>
    {
        // AllowAnyOrigin en lugar de WithOrigins("*") para permitir todos los orígenes
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    // Registrar los endpoints de Swagger por cada versión detectada por ApiVersioning
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
    });
}

app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseCors(PolicyNames.AllowSpecificOrigin);
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();
