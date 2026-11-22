namespace PedidoPostLab.Api.Contracts;

public sealed record ItemPedidoResponse(
    int ProdutoId,
    string NomeProduto,
    int Quantidade,
    decimal PrecoUnitario,
    decimal Subtotal);