using Microsoft.EntityFrameworkCore;
using ShopVRG.Data;
using ShopVRG.Data.Repositories;
using ShopVRG.Domain.Repositories;
using ShopVRG.Events;
using ShopVRG.Events.ServiceBus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure SQLite database
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "shopvrg.db");
builder.Services.AddDbContext<ShopDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Register repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Register event sender - Azure Service Bus or InMemory fallback
var useAzureServiceBus = builder.Configuration.GetValue<bool>("ServiceBus:UseAzure");
var serviceBusConnectionString = builder.Configuration["ServiceBus:ConnectionString"];

if (useAzureServiceBus && !string.IsNullOrEmpty(serviceBusConnectionString))
{
    Console.WriteLine("✓ Using Azure Service Bus for event messaging");
    builder.Services.AddSingleton<IEventSender>(sp =>
        new ServiceBusEventSender(serviceBusConnectionString));
}
else
{
    Console.WriteLine("⚠ Using InMemory event sender (development mode)");
    builder.Services.AddSingleton<InMemoryEventSender>();
    builder.Services.AddSingleton<IEventSender>(sp => sp.GetRequiredService<InMemoryEventSender>());
}

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ShopVRG - PC Components Store API",
        Version = "v1",
        Description = "API for managing PC components store - Orders, Payments, and Shipping",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "ShopVRG Team",
            Email = "contact@shopvrg.com"
        }
    });
});

// Configure CORS
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

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();

    // Delete and recreate for clean state (development only)
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();

    Console.WriteLine($"Database location: {dbPath}");
    Console.WriteLine($"Products in database: {db.Products.Count()}");
}

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShopVRG API v1");
    c.RoutePrefix = string.Empty; // Swagger UI at root
});

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine();
Console.WriteLine("╔═══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║     ShopVRG - PC Components Store API                              ║");
Console.WriteLine("║     Domain-Driven Design + .NET 9                                  ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine($"Swagger UI: http://localhost:5000");
Console.WriteLine($"API Base:   http://localhost:5000/api");
Console.WriteLine();
Console.WriteLine("Available endpoints:");
Console.WriteLine("  GET  /api/products         - List all products");
Console.WriteLine("  GET  /api/products/active  - List active products");
Console.WriteLine("  GET  /api/products/{code}  - Get product by code");
Console.WriteLine("  POST /api/orders           - Place a new order");
Console.WriteLine("  POST /api/payments         - Process payment");
Console.WriteLine("  POST /api/shipping         - Ship order");
Console.WriteLine("  GET  /api/shipping/carriers - List carriers");
Console.WriteLine();

app.Run();
