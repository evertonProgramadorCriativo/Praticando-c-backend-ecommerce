using PedidoPostLab.Api.Contracts;

namespace PedidoPostLab.Api.Services;

public sealed class PedidoCreationService : IPedidoCreationService
{
    public Task<PedidoCreationResult> CreateAsync(
        CriarPedidoRequest request,
        CancellationToken cancellationToken)
    {
        var erroDeValidacao = Validar(request);
        if (erroDeValidacao is not null)
        {
            return Task.FromResult(
                PedidoCreationResult.Failure(PedidoCreationError.Validacao, erroDeValidacao));
        }

        SortedDictionary<int, int> quantidadesPorProduto;
        try
        {
            quantidadesPorProduto = AgruparPorProduto(request.Itens!);
        }
        catch (OverflowException)
        {
            return Task.FromResult(
                PedidoCreationResult.Failure(
                    PedidoCreationError.Validacao,
                    "A quantidade total de um produto é grande demais."));
        }

        // PROVISÓRIO: nos Blocos 10 e 11 isto passa a vir do banco.
        var itens = quantidadesPorProduto
            .Select(par => new ItemPedidoResponse(
                ProdutoId: par.Key,
                NomeProduto: "(ainda não consultado)",
                Quantidade: par.Value,
                PrecoUnitario: 0,
                Subtotal: 0))
            .ToList();

        var pedido = new PedidoCriadoResponse(
            Id: 0,
            ClienteId: request.ClienteId,
            DataPedido: DateTime.UtcNow,
            ValorTotal: 0,
            Itens: itens);

        return Task.FromResult(PedidoCreationResult.Success(pedido));
    }

    private static string? Validar(CriarPedidoRequest request)
    {
        if (request.ClienteId <= 0)
            return "O clienteId deve ser um número maior que zero.";

        if (request.Itens is null || request.Itens.Count == 0)
            return "O pedido deve ter pelo menos um item.";

        foreach (var item in request.Itens)
        {
            if (item is null)
                return "A lista de itens contém um item vazio.";

            if (item.ProdutoId <= 0)
                return "Cada item deve ter um produtoId maior que zero.";

            if (item.Quantidade <= 0)
                return "A quantidade de cada item deve ser maior que zero.";
        }

        return null;
    }

    private static SortedDictionary<int, int> AgruparPorProduto(IReadOnlyList<ItemPedidoRequest> itens)
    {
        var quantidades = new SortedDictionary<int, int>();

        foreach (var item in itens)
        {
            quantidades.TryGetValue(item.ProdutoId, out var quantidadeAtual);
            quantidades[item.ProdutoId] = checked(quantidadeAtual + item.Quantidade);
        }

        return quantidades;
    }
}