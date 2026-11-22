using PedidoPostLab.Api.Contracts;

namespace PedidoPostLab.Api.Endpoints;

public static class PedidoEndpoints
{
    public static IEndpointRouteBuilder MapOrders(this IEndpointRouteBuilder app)
    {
        app.MapPost("/pedidos", (CriarPedidoRequest request) =>
        {
            var itensRecebidos = request.Itens ?? Array.Empty<ItemPedidoRequest>();

            var resposta = new PedidoCriadoResponse(
                Id: 0,
                ClienteId: request.ClienteId,
                DataPedido: DateTime.UtcNow,
                ValorTotal: 0,
                Itens: itensRecebidos
                    .Select(i => new ItemPedidoResponse(
                        ProdutoId: i.ProdutoId,
                        NomeProduto: "(ainda não consultado)",
                        Quantidade: i.Quantidade,
                        PrecoUnitario: 0,
                        Subtotal: 0))
                    .ToList());

            return TypedResults.Created($"/pedidos/{resposta.Id}", resposta);
        });

        return app;
    }
}