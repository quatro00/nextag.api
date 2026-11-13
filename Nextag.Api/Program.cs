using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nextag.Api.Data;
using Nextag.Api.Models.Configuration;
using Nextag.Api.Repositories.Implementation;
using Nextag.Api.Repositories.Interface;
using Nextag.Api.Services.ActivoAsignaciones.Implementation;
using Nextag.Api.Services.ActivoAsignaciones.Interface;
using Nextag.Api.Services.ActivoEtiquetas.Implementation;
using Nextag.Api.Services.ActivoEtiquetas.Interface;
using Nextag.Api.Services.ActivoMovimientos.Implementation;
using Nextag.Api.Services.ActivoMovimientos.Interface;
using Nextag.Api.Services.Activos.Implementation;
using Nextag.Api.Services.Activos.Interface;
using Nextag.Api.Services.Administrador.Implementation;
using Nextag.Api.Services.Administrador.Interface;
using Nextag.Api.Services.Archivos.Implementation;
using Nextag.Api.Services.Archivos.Interface;
using Nextag.Api.Services.CategoriasActivo.Implementation;
using Nextag.Api.Services.CategoriasActivo.Interface;
using Nextag.Api.Services.CategoriasTicket.Implementation;
using Nextag.Api.Services.CategoriasTicket.Interface;
using Nextag.Api.Services.EquipoSoporteIntegrantes.Implementation;
using Nextag.Api.Services.EquipoSoporteIntegrantes.Interface;
using Nextag.Api.Services.EquiposSoporte.Implementation;
using Nextag.Api.Services.EquiposSoporte.Interface;
using Nextag.Api.Services.EstadosActivo.Implementation;
using Nextag.Api.Services.EstadosActivo.Interface;
using Nextag.Api.Services.EtiquetaConfiguraciones.Implementation;
using Nextag.Api.Services.EtiquetaConfiguraciones.Interface;
using Nextag.Api.Services.Etiquetas.Implementation;
using Nextag.Api.Services.Etiquetas.Interface;
using Nextag.Api.Services.EtiquetasPublicas.Implementation;
using Nextag.Api.Services.EtiquetasPublicas.Interface;
using Nextag.Api.Services.Marcas.Implementation;
using Nextag.Api.Services.Marcas.Interface;
using Nextag.Api.Services.ModelosActivo.Implementation;
using Nextag.Api.Services.ModelosActivo.Interface;
using Nextag.Api.Services.ProveedorContactos.Implementation;
using Nextag.Api.Services.ProveedorContactos.Interface;
using Nextag.Api.Services.Proveedores.Implementation;
using Nextag.Api.Services.Proveedores.Interface;
using Nextag.Api.Services.Responsables.Implementation;
using Nextag.Api.Services.Responsables.Interface;
using Nextag.Api.Services.Sucursales.Implementation;
using Nextag.Api.Services.Sucursales.Interface;
using Nextag.Api.Services.Tickets.Implementation;
using Nextag.Api.Services.Tickets.Interface;
using Nextag.Api.Services.TiposProveedor.Implementation;
using Nextag.Api.Services.TiposProveedor.Interface;
using Nextag.Api.Services.TiposUbicacion.Implementation;
using Nextag.Api.Services.TiposUbicacion.Interface;
using Nextag.Api.Services.Ubicaciones.Implementation;
using Nextag.Api.Services.Ubicaciones.Interface;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NexTag API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT. Ejemplo: Bearer eyJhbGciOiJIUzI1NiIs..."
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

builder.Services.AddDbContext<NextagContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("NextagConnection"));
});

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("NextagConnection"));
});

// Repositories
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

// Servicios administrador
builder.Services.AddScoped<IUsuarioAdministradorService, UsuarioAdministradorService>();
builder.Services.AddScoped<IEmpresaAdministradorService, EmpresaAdministradorService>();
builder.Services.AddScoped<ISucursalService, SucursalService>();
builder.Services.AddScoped<ITipoUbicacionService, TipoUbicacionService>();
builder.Services.AddScoped<IUbicacionService, UbicacionService>();
builder.Services.AddScoped<ICategoriaActivoService, CategoriaActivoService>();
builder.Services.AddScoped<IUsuarioAdministradorService, UsuarioAdministradorService>();
builder.Services.AddScoped<IEmpresaAdministradorService, EmpresaAdministradorService>();
builder.Services.AddScoped<IMarcaService, MarcaService>();
builder.Services.AddScoped<IModeloActivoService, ModeloActivoService>();
builder.Services.AddScoped<IEstadoActivoService, EstadoActivoService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IProveedorContactoService, ProveedorContactoService>();
builder.Services.AddScoped<ITipoProveedorService, TipoProveedorService>();
builder.Services.AddScoped<IActivoService, ActivoService>();
builder.Services.AddScoped<IEtiquetaService, EtiquetaService>();
builder.Services.AddScoped<IActivoEtiquetaService, ActivoEtiquetaService>();
builder.Services.AddScoped<IEtiquetaPublicaService, EtiquetaPublicaService>();
builder.Services.AddScoped<IResponsableService, ResponsableService>();
builder.Services.AddScoped<IActivoAsignacionService, ActivoAsignacionService>();
builder.Services.AddScoped<IActivoMovimientoService, ActivoMovimientoService>();
builder.Services.AddScoped<ICategoriaTicketService, CategoriaTicketService>();
builder.Services.AddScoped<IEquipoSoporteService, EquipoSoporteService>();
builder.Services.AddScoped<IEquipoSoporteIntegranteService, EquipoSoporteIntegranteService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ITicketEstadoService, TicketEstadoService>();
builder.Services.AddScoped<ITicketAsignacionService, TicketAsignacionService>();
builder.Services.AddScoped<ITicketSeguimientoService, TicketSeguimientoService>();
builder.Services.AddScoped<ITicketFolioService, TicketFolioService>();
builder.Services.AddScoped<ITicketPublicoService, TicketPublicoService>();
builder.Services.AddScoped<IEtiquetaConfiguracionService, EtiquetaConfiguracionService>();


// ==========================================
// ALMACENAMIENTO DE ARCHIVOS
// ==========================================

builder.Services.Configure<ArchivoStorageOptions>(
    builder.Configuration.GetSection(
        ArchivoStorageOptions.SectionName));

var proveedorArchivos =
    builder.Configuration[
        "ArchivoStorage:Provider"];

if (string.Equals(
    proveedorArchivos,
    "Local",
    StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<
        IArchivoStorageService,
        LocalFileStorageService>();
}
else
{
    throw new InvalidOperationException(
        $"Proveedor de almacenamiento no soportado: {proveedorArchivos}");
}

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy("TicketsPublicos", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                context.Connection.RemoteIpAddress?
                    .ToString() ?? "unknown",

            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,

                Window = TimeSpan.FromMinutes(1),

                QueueLimit = 0,

                AutoReplenishment = true
            }));
});

builder.Services.AddIdentityCore<IdentityUser>()
    .AddRoles<ApplicationRole>()
    .AddTokenProvider<DataProtectorTokenProvider<IdentityUser>>("envioCorreos")
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            AuthenticationType = "Jwt",
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("No se encontró Jwt:Key.")))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAny", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();

app.UseCors("AllowAny");
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();