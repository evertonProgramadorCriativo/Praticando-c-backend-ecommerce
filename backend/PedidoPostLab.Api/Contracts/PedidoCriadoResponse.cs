namespace PedidoPostLab.Api.Contracts;

public sealed record PedidoCriadoResponse(
    int Id,
    int ClienteId,
    DateTime DataPedido,
    decimal ValorTotal,
    IReadOnlyList<ItemPedidoResponse> Itens);