using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using System.Net.Http;
using ShoppingCartExample;

public interface IShoppingCart : IGrainWithIntegerKey
{
    Task AddItem(Product productToAdd);
    Task RemoveItem(Product productToRemove);
    Task<Result<string>> Checkout();
    Task<List<Product>> ViewCart();
}

public sealed class ShoppingCart : Grain, IShoppingCart
{
    private readonly IPersistentState<List<Product>> _state;
    private readonly ILogger<ShoppingCart> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ShoppingCart(
        [PersistentState("cart", "carts")] IPersistentState<List<Product>> state,
        ILogger<ShoppingCart> logger,
        IHttpClientFactory httpClientFactory)
        : base()
    {
        this._state = state;
        this._logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task AddItem(Product productToAdd)
    {
        if (productToAdd == null)
        {
            throw new ArgumentNullException(nameof(productToAdd));
        }

        _logger.LogInformation($"Adding product {productToAdd.Name} to cart.");
        _state.State.Add(productToAdd);
        await _state.WriteStateAsync();
    }

    public async Task RemoveItem(Product productToRemove)
    {
        var product = _state.State.FirstOrDefault(p => p.Name == productToRemove.Name);

        if (product != null)
        {
            _state.State.Remove(product);
            await _state.WriteStateAsync();
            _logger.LogInformation($"Removed product {productToRemove.Name} from cart.");
        }
        else
        {
            _logger.LogWarning($"Attempted to remove non-existing product {productToRemove.Name} from cart.");
        }
    }

    public async Task<List<Product>> ViewCart()
    {
        await _state.ReadStateAsync();
        return new List<Product>(_state.State);
    }

    public async Task<Result<string>> Checkout()
    {
        if (!_state.State.Any())
        {
            return Result<string>.Failure("Nothing in cart.");
        }

        var paymentResult = await ProcessPayment();

        if (!paymentResult.IsSuccess)
        {
            // Payment failed, no need to proceed further

            return Result<string>.Failure("Payment processing failed.");
        }

        var shippingResult = await ProcessShipping();

        if (!shippingResult.IsSuccess)
        {
            // Shipping failed, initiate compensating transaction for payment

            await ReversePayment();
            return Result<string>.Failure("Shipping processing failed.");
        }

        return Result<string>.Success("Checkout processing success");
    }

    private async Task<Result<string>> ProcessShipping()
    {
        var client = _httpClientFactory.CreateClient("ShippingClient");
        var response = await client.PostAsync("/shipping/process", null);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Shipping processing failed.");

            return Result<string>.Failure("Shipping processing failed.");
        }

        _logger.LogInformation("Shipping processed successfully.");

        _state.State.Clear();
        await _state.WriteStateAsync();

        return Result<string>.Success("Shipping processed successfully.");
    }

    private async Task<Result<string>> ProcessPayment()
    {
        var client = _httpClientFactory.CreateClient("PaymentClient");
        var response = await client.PostAsync("/payment/process", null);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Payment processing failed.");

            return Result<string>.Failure("Payment processing failed.");
        }

        _logger.LogInformation("Payment processed successfully.");

        _state.State.Clear();
        await _state.WriteStateAsync();

        return Result<string>.Success("Payment processed successfully.");
    }

    private async Task<Result<string>> ReversePayment()
    {
        // Implement the logic to reverse the payment here
        // This is typically an API call to your payment service

        _logger.LogInformation("Payment reversed due to shipping failure.");

        return Result<string>.Success("Payment reversed successfully.");
    }
}


[GenerateSerializer]
public record Product
{
    [Id(0)] public string Name { get; set; } = "";
};