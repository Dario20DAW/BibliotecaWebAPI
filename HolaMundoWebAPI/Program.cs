using System.Text;
using System.Text.Json.Serialization;
using BibliotecaAPI;
using BibliotecaAPI.Datos;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection();
        

var origenesPermitidos = builder.Configuration.GetSection("origenesPermitidos").Get<string[]>()!;  


builder.Services.AddCors(opciones  =>
{
    opciones.AddDefaultPolicy(opcionesCors =>
    {
        opcionesCors.WithOrigins(origenesPermitidos).AllowAnyMethod().AllowAnyHeader()
        .WithExposedHeaders("mi-cabecera");
    });
});

// Configura el servicio para usar controladores y agregar soporte para NewtonsoftJson (JSON m�s flexible)
builder.Services.AddControllers().AddNewtonsoftJson();



// Configura el contexto de la base de datos con una conexi�n a SQL Server usando la cadena de conexi�n "DefaultConnection"
builder.Services.AddDbContext<ApplicationDbContext>(opciones => opciones.UseSqlServer("name=DefaultConnection"));



// Agrega AutoMapper para facilitar el mapeo entre objetos de diferentes tipos, usando el tipo Program para buscar los perfiles de mapeo
builder.Services.AddAutoMapper(typeof(Program));



// Configura la autenticaci�n de usuario utilizando Identity para manejo de usuarios, roles y tokens en la base de datos
builder.Services.AddIdentityCore<Usuario>()
    .AddEntityFrameworkStores<ApplicationDbContext>() // Usa la base de datos para el almacenamiento de usuarios
    .AddDefaultTokenProviders(); // Proporciona generadores de tokens predeterminados para la autenticaci�n




// Registra servicios adicionales para el manejo de usuarios y autenticaci�n
builder.Services.AddScoped<UserManager<Usuario>>(); // Maneja la creaci�n y gesti�n de usuarios
builder.Services.AddScoped<SignInManager<Usuario>>(); // Gestiona el inicio de sesi�n de usuarios
builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();

builder.Services.AddHttpContextAccessor(); // Proporciona acceso al contexto HTTP en cualquier parte de la aplicaci�n

// Configura la autenticaci�n mediante JWT (JSON Web Tokens) para validaci�n de tokens
builder.Services.AddAuthentication().AddJwtBearer(opciones =>
{
    opciones.MapInboundClaims = false; // Desactiva el mapeo de reclamos (claims) de entrada

    opciones.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false, // No valida el emisor del token
        ValidateAudience = false, // No valida la audiencia del token
        ValidateLifetime = false, // No valida la expiraci�n del token
        ValidateIssuerSigningKey = false, // No valida la clave de firma del emisor
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["llavejwt"]!)), // Crea una clave de seguridad sim�trica para validar la firma del token
        ClockSkew = TimeSpan.Zero, // Define el tiempo de desfase permitido para la validaci�n de tiempo (sin desfase)
    };
});


builder.Services.AddAuthorization(opciones =>
{
    opciones.AddPolicy("esadmin", politica => politica.RequireClaim("esadmin"));

});




// Construye la aplicaci�n
var app = builder.Build();

// �rea de middlewares: c�digo que se ejecuta en la pipeline de solicitudes HTTP

// Middleware personalizado para bloquear acceso a la ruta "/bloqueado" con un c�digo de estado 403 (Acceso Denegado)
app.Use(async (contexto, next) =>
{
    if (contexto.Request.Path == "/bloqueado")
    {
        contexto.Response.StatusCode = 403; // Establece el c�digo de estado 403 (Acceso Denegado)
        await contexto.Response.WriteAsync("Acceso denegado"); // Envia un mensaje de acceso denegado al usuario
    }
    else
    {
        await next.Invoke(); // Pasa al siguiente middleware si la ruta no es "/bloqueado"
    }
});


app.Use(async (contexto, next) =>
{
    contexto.Response.Headers.Append("mi-cabecera", "valor");

    await next();
});



app.UseCors();

// Mapea las rutas de los controladores para que la aplicaci�n sepa c�mo manejarlas
app.MapControllers();

// Inicia la aplicaci�n y comienza a escuchar las solicitudes HTTP
app.Run();
