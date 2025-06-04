using ElectronicsStoreAss3.Data;
using ElectronicsStoreAss3.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("ElectronicsStoreAss3Context")));

// Authentication & Authorization
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Authentication/Login";
        options.LogoutPath = "/Authentication/Logout";
        options.AccessDeniedPath = "/Authentication/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Session Configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".ElectronicsStore.Session";
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Business Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();

// Logging Configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddDebug();
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Database Initialization
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        if (app.Environment.IsDevelopment())
        {
            logger.LogInformation("Initializing database for development environment");
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            Init.SeedAll(db);
            logger.LogInformation("Database initialized successfully");
        }
        else
        {
            // For production, use migrations instead of EnsureCreated
            db.Database.Migrate();
            logger.LogInformation("Database migration completed");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred while initializing database");
        throw;
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Custom Routes
app.MapControllerRoute(
    name: "ProductDetails",
    pattern: "product/{sku}",
    defaults: new { controller = "Product", action = "Details" });

app.MapControllerRoute(
    name: "ProductDetailsById", 
    pattern: "product/id/{id:int}",
    defaults: new { controller = "Product", action = "Details" });

app.MapControllerRoute(
    name: "Tracking",
    pattern: "track/{trackingNumber?}",
    defaults: new { controller = "Shipment", action = "Track" });

app.MapControllerRoute(
    name: "Catalogue",
    pattern: "catalogue/{category?}",
    defaults: new { controller = "Product", action = "Catalogue" });

// Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();