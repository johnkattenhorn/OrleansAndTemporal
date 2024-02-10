using Temporalio.Client.Interceptors;
using Temporalio.Workflows;

namespace ShoppingCartExample.Temporal;

[Workflow]
public class CheckoutWorkflow
{
    public const string TaskQueue = "shopping-cart-example";

    private readonly ILogger<IShoppingCartGrain> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IClusterClient _orleansClient;

    public CheckoutWorkflow()
    {
    }

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
        // Step 1 - Process Payment (first time will always fail, second time will pass)
        // If Payment failed after 24 hours then call reverse payment method (optional)
        // If payment was successful then:
        // Step - 2 Process Shipping (first time passes at the moment, fails every other call after that)

        // If both are successful then:
        // Update Cart (via orleans grain clear cart method)

        var payment = await Workflow.ExecuteActivityAsync(
                (CheckoutActivities act) => act.ProcessPayment(cartId), new ActivityOptions
                {
                    StartToCloseTimeout = TimeSpan.FromMinutes(5)
                });

        var shipping = await Workflow.ExecuteActivityAsync(
                (CheckoutActivities act) => act.ProcessShipping(cartId), new ActivityOptions
                {
                    StartToCloseTimeout = TimeSpan.FromMinutes(5)
                });

        //var clearCart = await Workflow.ExecuteActivityAsync(
        //    (CheckoutActivities act) => act.ClearCart(cartId), new ActivityOptions
        //    {
        //        StartToCloseTimeout = TimeSpan.FromMinutes(5)
        //    });

        return Result<string>.Success("Checkout was successful.");
    }
}