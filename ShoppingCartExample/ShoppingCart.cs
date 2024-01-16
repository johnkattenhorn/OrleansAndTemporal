using Orleans.Runtime;

public interface IShoppingCart : IGrainWithIntegerKey
{
    Task AddItem(Product productToAdd);
    Task RemoveItem(Product productToRemove);
    Task Checkout();

    Task<List<Product>> ViewCart();
}

public sealed class ShoppingCart : Grain, IShoppingCart
{
    private readonly IPersistentState<List<Product>> _state;
    private readonly ILogger<ShoppingCart> _logger;

    public ShoppingCart(
        [PersistentState("cart", "carts")] IPersistentState<List<Product>> state,
        ILogger<ShoppingCart> logger)
        : base()
    {
        this._state = state;
        this._logger = logger;
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

    public async Task Checkout()
    {
        // Implement checkout logic here

        _state.State.Clear();
        await _state.WriteStateAsync();
        _logger.LogInformation("Checkout complete and cart cleared.");
    }
}

[GenerateSerializer, Alias(nameof(Product))]
public record Product(string Name);