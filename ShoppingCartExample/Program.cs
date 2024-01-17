using System.Net;
using Microsoft.OpenApi.Models;
using Serilog;
using ShoppingCartExample;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // Set the minimum log level
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code) // Use color theme
    .CreateLogger();

builder.Host.UseSerilog(); // Use Serilog for logging

builder.Host.UseOrleans(static siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMemoryGrainStorage("carts");
});

builder.Services.AddEndpointsApiExplorer(); // Required for Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Shopping Cart Example", Version = "v1" });
});

builder.Services.AddHttpClient("PaymentClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7282/");
});

builder.Services.AddHttpClient("ShippingClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7282/");
});

using var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shopping Cart Example"));
}

int paymentCallCount = 1;

app.MapPost("/payment/process", async () =>
{
    await Task.Delay(2000); // Delay for 2 seconds

    paymentCallCount++;
    if (paymentCallCount % 2 == 0)
    {
        // Simulate a failure every other call
        return Results.Problem("Simulated payment processing failure.", statusCode: (int)HttpStatusCode.InternalServerError);

    }
    return Results.Ok("Payment processed successfully.");
});

int shippingCallCount = 0;

app.MapPost("/shipping/process", async () =>
{
    await Task.Delay(2000); // Delay for 2 seconds

    shippingCallCount++;
    if (shippingCallCount % 2 == 0)
    {
        // Simulate a failure every other call
        return Results.Problem("Simulated carrier dispatch processing failure.", statusCode: (int)HttpStatusCode.InternalServerError);

    }
    return Results.Ok("Carrier dispatch processed successfully.");
});

app.MapPost("/cart/add", async (int cartId, Product product, IClusterClient client) =>
{
    var cartGrain = client.GetGrain<IShoppingCartGrain>(cartId);
    await cartGrain.AddItem(product);
    return Results.Ok("Item added to cart");
});

app.MapPost("/cart/remove", async (int cartId, Product product, IClusterClient client) =>
{
    var cartGrain = client.GetGrain<IShoppingCartGrain>(cartId);
    await cartGrain.RemoveItem(product);
    return Results.Ok("Item removed from cart");
});

app.MapGet("/cart/view", async (int cartId, IClusterClient client) =>
{
    var cartGrain = client.GetGrain<IShoppingCartGrain>(cartId);
    var items = await cartGrain.ViewCart();
    return Results.Ok(items);
});

app.MapPost("/cart/checkout", async (int cartId, IClusterClient client) =>
{
    var cartGrain = client.GetGrain<IShoppingCartGrain>(cartId);
    var result = await cartGrain.Checkout();

    return !result.IsSuccess ? 
        Results.Problem(result.Error, statusCode: (int)HttpStatusCode.BadRequest) : 
        Results.Ok("Checkout process completed");
});

app.Run();