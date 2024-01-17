using Temporalio.Activities;

namespace ShoppingCartExample;

public class CheckoutActivities
{
    private readonly ILogger<CheckoutActivities> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IClusterClient _orleansClient;
    
    public CheckoutActivities(
        ILogger<CheckoutActivities> logger,
        IHttpClientFactory httpClientFactory,
        IClusterClient orleansClient)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _orleansClient = orleansClient;
    }

    [Activity]
    public async Task<Result<string>> ProcessShipping(long cartId)
    {
        var cartGrain = _orleansClient.GetGrain<IShoppingCartGrain>(cartId);

        var client = _httpClientFactory.CreateClient("ShippingClient");
        var response = await client.PostAsync("/shipping/process", null);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("[ShoppingCartExample] Shipping processing failed.");

            throw new ApplicationException("Payment processing failed."); // Throw an exception to trigger a retry
        }

        _logger.LogInformation("[ShoppingCartExample] Shipping processed successfully.");
      
        return Result<string>.Success("Shipping processed successfully.");
    }

    [Activity]
    public async Task<Result<string>> ProcessPayment(long cartId)
    {
        var cartGrain = _orleansClient.GetGrain<IShoppingCartGrain>(cartId);

        var client = _httpClientFactory.CreateClient("PaymentClient");
        var response = await client.PostAsync("/payment/process", null);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("[ShoppingCartExample] Payment processing failed.");

            throw new ApplicationException("Payment processing failed."); // Throw an exception to trigger a retry
        }

        _logger.LogInformation("[ShoppingCartExample] Payment processed successfully.");

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