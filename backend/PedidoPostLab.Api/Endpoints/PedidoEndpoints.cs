using PedidoPostLab.Api.Contracts;
using PedidoPostLab.Api.Services;

namespace PedidoPostLab.Api.Endpoints;

public static class PedidoEndpoints
{
    public static IEndpointRouteBuilder MapOrders(this IEndpointRouteBuilder app)
    {
        app.MapPost("/pedidos", async (
            CriarPedidoRequest request,
            IPedidoCreationService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.CreateAsync(request, cancellationToken);

            if (result.IsSuccess)
            {
                var pedido = result.Pedido!;
                return Results.Created($"/pedidos/{pedido.Id}", pedido);
            }

            return ToProblem(result);
        });

        return app;
    }

    private static IResult ToProblem(PedidoCreationResult result) => result.Error switch
    {
        PedidoCreationError.Validacao => Results.Problem(
            title: "Pedido inválido",
            detail: result.Message,
            statusCode: StatusCodes.Status400BadRequest),

        PedidoCreationError.ClienteNaoEncontrado => Results.Problem(
            title: "Cliente não encontrado",
            detail: result.Message,
            statusCode: StatusCodes.Status404NotFound),

        PedidoCreationError.ProdutoNaoEncontrado => Results.Problem(
            title: "Produto não encontrado",
            detail: result.Message,
            statusCode: StatusCodes.Status404NotFound),

        PedidoCreationError.EstoqueInsuficiente => Results.Problem(
            title: "Estoque insuficiente",
            detail: result.Message,
            statusCode: StatusCodes.Status409Conflict),

        _ => Results.Problem(
            title: "Erro ao criar o pedido",
            detail: result.Message,
            statusCode: StatusCodes.Status500InternalServerError)
    };
}