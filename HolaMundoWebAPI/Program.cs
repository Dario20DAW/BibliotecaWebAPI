﻿using System.Text;
using System.Text.Json.Serialization;
using BibliotecaAPI;
using BibliotecaAPI.Datos;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Swagger;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection();
builder.Services.AddOutputCache(opciones =>
{
    opciones.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(15);
});
        

var origenesPermitidos = builder.Configuration.GetSection("origenesPermitidos").Get<string[]>()!;  


builder.Services.AddCors(opciones  =>
{
    opciones.AddDefaultPolicy(opcionesCors =>
    {
        opcionesCors.WithOrigins(origenesPermitidos).AllowAnyMethod().AllowAnyHeader()
        .WithExposedHeaders("cantidad-total-registros");
    });
});

// Configura el servicio para usar controladores y agregar soporte para NewtonsoftJson (JSON más flexible)
builder.Services.AddControllers().AddNewtonsoftJson();



// Configura el contexto de la base de datos con una conexión a SQL Server usando la cadena de conexión "DefaultConnection"
builder.Services.AddDbContext<ApplicationDbContext>(opciones => opciones.UseSqlServer("name=DefaultConnection"));



// Agrega AutoMapper para facilitar el mapeo entre objetos de diferentes tipos, usando el tipo Program para buscar los perfiles de mapeo
builder.Services.AddAutoMapper(typeof(Program));



// Configura la autenticación de usuario utilizando Identity para manejo de usuarios, roles y tokens en la base de datos
builder.Services.AddIdentityCore<Usuario>()
    .AddEntityFrameworkStores<ApplicationDbContext>() // Usa la base de datos para el almacenamiento de usuarios
    .AddDefaultTokenProviders(); // Proporciona generadores de tokens predeterminados para la autenticación




// Registra servicios adicionales para el manejo de usuarios y autenticación
builder.Services.AddScoped<UserManager<Usuario>>(); // Maneja la creación y gestión de usuarios
builder.Services.AddScoped<SignInManager<Usuario>>(); // Gestiona el inicio de sesión de usuarios
builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();
builder.Services.AddTransient<IServicioHash, ServicioHash>();
builder.Services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosLocal>();


builder.Services.AddHttpContextAccessor(); // Proporciona acceso al contexto HTTP en cualquier parte de la aplicación

// Configura la autenticación mediante JWT (JSON Web Tokens) para validación de tokens
builder.Services.AddAuthentication().AddJwtBearer(opciones =>
{
    opciones.MapInboundClaims = false; // Desactiva el mapeo de reclamos (claims) de entrada

    opciones.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["llavejwt"]!)),
        ClockSkew = TimeSpan.Zero,
    };

});


builder.Services.AddAuthorization(opciones =>
{
    opciones.AddPolicy("esadmin", politica => politica.RequireClaim("esadmin"));

});


builder.Services.AddSwaggerGen(opciones =>
{
    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        In = ParameterLocation.Header,
        BearerFormat = "JWT"
    });

    opciones.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
     {  new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new List<string>()
    }
    });

    opciones.OperationFilter<FiltroAutorizacion>();


});



// Construye la aplicación
var app = builder.Build();

// Área de middlewares: código que se ejecuta en la pipeline de solicitudes HTTP

// Middleware personalizado para bloquear acceso a la ruta "/bloqueado" con un código de estado 403 (Acceso Denegado)
app.Use(async (contexto, next) =>
{
    if (contexto.Request.Path == "/bloqueado")
    {
        contexto.Response.StatusCode = 403; // Establece el código de estado 403 (Acceso Denegado)
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


app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

app.UseCors();

app.UseOutputCache();

// Mapea las rutas de los controladores para que la aplicación sepa cómo manejarlas
app.MapControllers();

// Inicia la aplicación y comienza a escuchar las solicitudes HTTP
app.Run();
