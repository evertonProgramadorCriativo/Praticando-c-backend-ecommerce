using PedidoPostLab.Api.Contracts;

namespace PedidoPostLab.Api.Services;

public sealed class PedidoCreationResult
{
    private PedidoCreationResult()
    {
    }

    public PedidoCriadoResponse? Pedido { get; private init; }
    public PedidoCreationError? Error { get; private init; }
    public string? Message { get; private init; }

    public bool IsSuccess => Pedido is not null;

    public static PedidoCreationResult Success(PedidoCriadoResponse pedido) =>
        new() { Pedido = pedido };

    public static PedidoCreationResult Failure(PedidoCreationError error, string message) =>
        new() { Error = error, Message = message };
}