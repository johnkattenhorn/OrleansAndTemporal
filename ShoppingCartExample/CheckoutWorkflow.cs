using ShoppingCartExample;
using System.Net.Http;
using Temporalio.Activities;
using Temporalio.Common;
using Temporalio.Workflows;

[Workflow]
public class CheckoutWorkflow
{
    public const string TaskQueue = "shopping-cart-example";

    private readonly ILogger<ShoppingCartGrain> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IClusterClient _orleansClient;

    public CheckoutWorkflow() { }

    public CheckoutWorkflow(
        ILogger<ShoppingCartGrain> logger,
        IHttpClientFactory httpClientFactory,
        IClusterClient client)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _orleansClient = client;
    }


    [WorkflowRun]
    public async Task<Result<string>> RunAsync(long cartId)
    {

        // Workflow 
        // Process Payment (first time will always fail, second time will pass)
        // if Payment failed after 24 hours then call reverse payment method (optional)
        // Process Shipping (first time passes at the moment)
        
        // if both are successful then:
        // Update Cart (via orleans grain clear cart message)


            var payment = await Workflow.ExecuteActivityAsync(
                (CheckoutWorkflow wf) => wf.ProcessPayment(cartId),new ActivityOptions
                {
                    StartToCloseTimeout = TimeSpan.FromMinutes(5)
                });

            var shipping = await Workflow.ExecuteActivityAsync(
                (CheckoutWorkflow wf) => wf.ProcessShipping(cartId), new ActivityOptions
                {
                    StartToCloseTimeout = TimeSpan.FromMinutes(5)
                });

            if (payment.IsSuccess & shipping.IsSuccess)
            {
                var shoppingCart = _orleansClient.GetGrain<IShoppingCartGrain>(cartId);

                Workflow.Logger.LogInformation("[ShoppingCartExample] Checkout was successful. Clearing Cart.");
                await shoppingCart.ClearCart();

                return Result<string>.Success("Checkout was successful. Clearing Cart.");
            }

            Workflow.Logger.LogError("[ShoppingCartExample] Checkout failed.");
        return Result<string>.Failure("Checkout failed.");
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

            return Result<string>.Failure("Shipping processing failed.");
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

            return Result<string>.Failure("Payment processing failed.");
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