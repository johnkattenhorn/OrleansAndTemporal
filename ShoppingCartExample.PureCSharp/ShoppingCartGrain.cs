using Orleans.Runtime;

namespace ShoppingCartExample.PureCSharp;

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
        const int maxAttempts = 3;
        const int delayMilliseconds = 1000; // Delay of 1 second between retries

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                _logger.LogInformation($"[ShoppingCartExample] Attempting to process shipping (Attempt {attempt}/{maxAttempts}).");

                var client = _httpClientFactory.CreateClient("ShippingClient");
                var response = await client.PostAsync("/shipping/process", null);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[ShoppingCartExample] Shipping processed successfully.");
                    _state.State.Clear();
                    await _state.WriteStateAsync();
                    return Result<string>.Success("Shipping processed successfully.");
                }

                _logger.LogWarning("[ShoppingCartExample] Shipping processing failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ShoppingCartExample] Error occurred during shipping processing (Attempt {attempt}/{maxAttempts}).");
            }

            if (attempt < maxAttempts)
            {
                _logger.LogInformation($"[ShoppingCartExample] Retrying in {delayMilliseconds}ms...");
                await Task.Delay(delayMilliseconds);
            }
        }

        return Result<string>.Failure("Shipping processing failed after retries.");
    }

    private async Task<Result<string>> ProcessPayment()
    {
        const int maxAttempts = 3;
        const int delayMilliseconds = 1000; // Delay of 1 second between retries

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                _logger.LogInformation($"[ShoppingCartExample] Attempting to process payment (Attempt {attempt}/{maxAttempts}).");

                var client = _httpClientFactory.CreateClient("PaymentClient");
                var response = await client.PostAsync("/payment/process", null);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[ShoppingCartExample] Payment processed successfully.");
                    _state.State.Clear();
                    await _state.WriteStateAsync();
                    return Result<string>.Success("Payment processed successfully.");
                }

                _logger.LogWarning("[ShoppingCartExample] Payment processing failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ShoppingCartExample] Error occurred during payment processing (Attempt {attempt}/{maxAttempts}).");
            }

            if (attempt < maxAttempts)
            {
                _logger.LogInformation($"[ShoppingCartExample] Retrying in {delayMilliseconds}ms...");
                await Task.Delay(delayMilliseconds);
            }
        }

        return Result<string>.Failure("Payment processing failed after retries.");
    }


    private async Task<Result<string>> ReversePayment()
    {
        // Implement the logic to reverse the payment here
        // This is typically an API call to your payment service

        _logger.LogInformation("[ShoppingCartExample] Payment reversed due to shipping failure.");

        return Result<string>.Success("Payment reversed successfully.");
    }
}