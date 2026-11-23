using PedidoPostLab.Api.Contracts;

namespace PedidoPostLab.Api.Services;

public interface IPedidoCreationService
{
    Task<PedidoCreationResult> CreateAsync(
        CriarPedidoRequest request,
        CancellationToken cancellationToken);
}