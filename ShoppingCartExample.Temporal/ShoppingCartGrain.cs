using Orleans.Runtime;
using Temporalio.Client;
using Temporalio.Workflows;

namespace ShoppingCartExample.Temporal;

public sealed class ShoppingCartGrain : Grain, IShoppingCartGrain
{
    private readonly IPersistentState<List<Product>> _state;
    private readonly ILogger<ShoppingCartGrain> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITemporalClient _temporalClient;

    public ShoppingCartGrain(
        [PersistentState("cart", "carts")] IPersistentState<List<Product>> state,
        ILogger<ShoppingCartGrain> logger,
        IHttpClientFactory httpClientFactory,
        ITemporalClient temporalClient)
        : base()
    {
        _state = state;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _temporalClient = temporalClient;
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

    public async Task ClearCart()
    {
        await _state.ClearStateAsync();
    }
    public async Task<Result<string>> Checkout()
    {
        if (!_state.State.Any())
        {
            return Result<string>.Failure("Nothing in cart.");
        }

        // Start a workflow

        var handle = await _temporalClient.StartWorkflowAsync(
            (CheckoutWorkflow wf) => wf.RunAsync(this.GetPrimaryKeyLong()),
            new() { Id = nameof(CheckoutWorkflow), TaskQueue = CheckoutWorkflow.TaskQueue });

        // Wait for a result

        var result = await handle.GetResultAsync();

        if (result.IsSuccess)
        {
            await ClearCart();
        }

        return result;
    }
}