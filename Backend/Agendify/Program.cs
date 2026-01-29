using Agendify.Data;
using Agendify.Repositories;
using Agendify.Middleware;
using Agendify.Common.Converters;
using Agendify.Services.Auth.Authentication;
using Agendify.Services.Auth.JWT;
using Agendify.Services.Auth.Password;
using Agendify.Services.Business;
using Agendify.Services.Appointments;
using Agendify.Services.Calendar;
using Agendify.Services.Providers;
using Agendify.Services.Customers;
using Agendify.Services.ProviderSchedules;
using Agendify.Services.ServicesServices;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);


// Configurar DbContext con SQL Server
builder.Services.AddDbContext<AgendifyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configurar FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Registrar Repositorios
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

// Registrar Servicios
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<ICalendarService, CalendarService>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IProviderService, ProviderService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IProviderScheduleService, ProviderScheduleService>();

// Configurar Controllers con snake_case JSON naming policy
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
        // Converter para manejar DateTime como hora local (evita conversiones UTC)
        options.JsonSerializerOptions.Converters.Add(new LocalDateTimeConverter());
        // Converter para manejar TimeSpan desde strings (ej: "09:00:00")
        options.JsonSerializerOptions.Converters.Add(new TimeSpanConverter());
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Configurar ProblemDetails (RFC 7807)
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        // Agregar información adicional en desarrollo
        if (builder.Environment.IsDevelopment())
        {
            context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        }
        
        // Agregar timestamp
        context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
    };
});

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Agendify API",
        Version = "v1",
        Description = "API para sistema de gestión de turnos Agendify",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Agendify Team"
        }
    });

    // Configurar JWT en Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Solo ingresa el token (sin 'Bearer')",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
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
                },
                Scheme = "bearer",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// Configurar CORS (opcional, pero recomendado para desarrollo)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Aplicar migraciones automáticamente al iniciar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AgendifyDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Aplicando migraciones pendientes...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Migraciones aplicadas exitosamente.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al aplicar migraciones de base de datos");
        throw;
    }
}

// Middleware de manejo de excepciones global
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Agendify API v1");
        options.RoutePrefix = "swagger"; // Accesible en /swagger
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

