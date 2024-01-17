namespace ShoppingCartExample;

[GenerateSerializer]
public record Product
{
    [Id(0)] public string Name { get; set; } = "";
};