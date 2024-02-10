using Orleans.Runtime;
using Polly;
using Polly.Retry;

namespace ShoppingCartExample.Polly;

public sealed class ShoppingCartGrain : Grain, IShoppingCartGrain
{
    private readonly IPersistentState<List<Product>> _state;
    private readonly ILogger<ShoppingCartGrain> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ShoppingCartGrain(
        [PersistentState("cart", "carts")] IPersistentState<List<Product>> state,
        ILogger<ShoppingCartGrain> logger,
        IHttpClientFactory httpClientFactory)
        : base()
    {
        _state = state;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task AddItem(Product productToAdd)
    {
        if (productToAdd == null)
        {
            throw new ArgumentNullException(nameof(productToAdd));
        }

        _logger.LogInformation($"[ShoppingCartExample] Adding product {productToAdd.Name} to cart.");

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
            _logger.LogInformation($"[ShoppingCartExample] Removed product {productToRemove.Name} from cart.");
        }
        else
        {
            _logger.LogWarning($"[ShoppingCartExample] Attempted to remove non-existing product {productToRemove.Name} from cart.");
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
        // Define a polly retry policy
        AsyncRetryPolicy<HttpResponseMessage> retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(1),
                (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning($"[ShoppingCartExample] Retry {retryCount} for {context.OperationKey} due to {outcome.Exception?.Message ?? outcome.Result.ReasonPhrase}.");
                });

        var client = _httpClientFactory.CreateClient("ShippingClient");
        var response = await retryPolicy.ExecuteAsync(() => client.PostAsync("/shipping/process", null));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("[ShoppingCartExample] Shipping processing failed after retries.");
            return Result<string>.Failure("Shipping processing failed.");
        }

        _logger.LogInformation("[ShoppingCartExample] Shipping processed successfully.");
        _state.State.Clear();
        await _state.WriteStateAsync();

        return Result<string>.Success("Shipping processed successfully.");
    }

    private async Task<Result<string>> ProcessPayment()
    {
        // Define a polly retry policy
        AsyncRetryPolicy<HttpResponseMessage> retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(1),
                (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        $"[ShoppingCartExample] Retry {retryCount} for {context.OperationKey}");

                    _logger.LogWarning($"to {outcome.Exception?.Message ?? outcome.Result.ReasonPhrase}.");
                });

        var client = _httpClientFactory.CreateClient("PaymentClient");
        var response = await retryPolicy.ExecuteAsync(() => client.PostAsync("/payment/process", null));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("[ShoppingCartExample] Payment processing failed after retries.");
            return Result<string>.Failure("Payment processing failed.");
        }

        _logger.LogInformation("[ShoppingCartExample] Payment processed successfully.");
        _state.State.Clear();
        await _state.WriteStateAsync();

        return Result<string>.Success("Payment processed successfully.");

    }

    private async Task<Result<string>> ReversePayment()
    {
        // Implement the logic to reverse the payment here
        // This is typically an API call to your payment service

        _logger.LogInformation("[ShoppingCartExample] Payment reversed due to shipping failure.");

        return Result<string>.Success("Payment reversed successfully.");
    }
}