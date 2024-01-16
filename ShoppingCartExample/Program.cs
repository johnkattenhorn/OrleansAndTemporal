using System.Net;
using System.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Orleans.Runtime;

var builder = WebApplication.CreateBuilder(args);

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

using var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shopping Cart Example"));
}

int callCount = 0;

app.MapPost("/payment/process", () =>
{
    callCount++;
    if (callCount % 2 == 0)
    {
        // Simulate a failure every other call
        return Results.Problem("Simulated payment processing failure.", statusCode: (int)HttpStatusCode.InternalServerError);

    }
    return Results.Ok("Payment processed successfully.");
});

app.MapPost("/cart/add", async (int cartId, Product product, IClusterClient client) =>
{
    var cartGrain = client.GetGrain<ShoppingCart>(cartId);
    await cartGrain.AddItem(product);
    return Results.Ok("Item added to cart");
});

app.MapPost("/cart/remove", async (int cartId, Product product, IClusterClient client) =>
{
    var cartGrain = client.GetGrain<ShoppingCart>(cartId);
    await cartGrain.RemoveItem(product);
    return Results.Ok("Item removed from cart");
});

app.MapGet("/cart/view", async (int cartId, IClusterClient client) =>
{
    var cartGrain = client.GetGrain<ShoppingCart>(cartId);
    var items = await cartGrain.ViewCart();
    return Results.Ok(items);
});

app.MapPost("/cart/checkout", async (int cartId, IClusterClient client) =>
{
    var cartGrain = client.GetGrain<ShoppingCart>(cartId);
    await cartGrain.Checkout();
    return Results.Ok("Checkout process initiated");
});

app.Run();