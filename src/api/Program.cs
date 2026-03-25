using CQRS.POC.Api.Middleware;
using CQRS.POC.Application;
using CQRS.POC.Application.Abstractions;
using CQRS.POC.Application.Common.Behaviors;
using CQRS.POC.Application.Products.Commands;
using CQRS.POC.Infrastructure;
using CQRS.POC.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// EF Core — SQLite per il POC
//builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source=cqrs-poc.db"));
//builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddInfrastructure(builder.Configuration);

// MediatR — scansiona l'assembly di Application per trovare tutti gli Handler , usando un marker (classe custom che nasce cn quell'obiettivo)
// la pipeline di mediatR è composta da una serie di comportamenti (behaviors) che vengono eseguiti in un ordine specifico prima e dopo l'esecuzione dell'handler.
// In questo caso, abbiamo due behaviors:
//      LoggingBehavior e ValidationBehavior.
//      Il primo si occupa di loggare le informazioni sulla richiesta, mentre il secondo esegue la validazione del comando usando FluentValidation.
//      L'ordine è importante perché vogliamo loggare la richiesta prima di eseguire la validazione, in modo da avere traccia anche delle richieste che falliscono la validazione.
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssemblyContaining<CQRS.POC.Application.AssemblyMarker>();

    // Ordine importante: Logging → Validation → Handler
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
}

);


// FluentValidation — scansiona l'assembly di Application per trovare tutti i Validator usando il marker (classe custom che nasce cn quell'obiettivo)
builder.Services.AddValidatorsFromAssemblyContaining<CQRS.POC.Application.AssemblyMarker>();

// API + OpenAPI integrato .NET 9/10
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();


var app = builder.Build();

app.UseExceptionHandler();  // ← prima di tutto il resto

// Crea il DB automaticamente all'avvio (solo per il POC)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}


// Configure the HTTP request pipeline.

app.MapOpenApi();
app.MapScalarApiReference(); // UI su /scalar/v1


app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Content-Security-Policy", "frame-ancestors 'none'");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    await next();
});



app.MapControllers();
app.Run();
