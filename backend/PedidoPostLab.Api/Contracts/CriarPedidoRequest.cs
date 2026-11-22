namespace PedidoPostLab.Api.Contracts;

public sealed record CriarPedidoRequest(int ClienteId, IReadOnlyList<ItemPedidoRequest>? Itens);
