namespace ShoppingCartExample.Polly;

public interface IShoppingCartGrain : IGrainWithIntegerKey
{
    Task AddItem(Product productToAdd);
    Task RemoveItem(Product productToRemove);
    Task<Result<string>> Checkout();
    Task<List<Product>> ViewCart();
}