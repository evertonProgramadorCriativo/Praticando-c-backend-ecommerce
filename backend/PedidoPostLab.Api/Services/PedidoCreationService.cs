using Microsoft.EntityFrameworkCore;
using PedidoPostLab.Api.Contracts;
using PedidoPostLab.Api.Data;

namespace PedidoPostLab.Api.Services;

public sealed class PedidoCreationService : IPedidoCreationService
{
    private readonly PedidosDbContext _db;

    public PedidoCreationService(PedidosDbContext db)
    {
        _db = db;
    }

    public async Task<PedidoCreationResult> CreateAsync(
        CriarPedidoRequest request,
        CancellationToken cancellationToken)
    {
        var erroDeValidacao = Validar(request);
        if (erroDeValidacao is not null)
        {
            return PedidoCreationResult.Failure(PedidoCreationError.Validacao, erroDeValidacao);
        }

        SortedDictionary<int, int> quantidadesPorProduto;
        try
        {
            quantidadesPorProduto = AgruparPorProduto(request.Itens!);
        }
        catch (OverflowException)
        {
            return PedidoCreationResult.Failure(
                PedidoCreationError.Validacao,
                "A quantidade total de um produto é grande demais.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var clienteExiste = await _db.Clientes
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.ClienteId, cancellationToken);

        if (!clienteExiste)
        {
            return PedidoCreationResult.Failure(
                PedidoCreationError.ClienteNaoEncontrado,
                $"Cliente {request.ClienteId} não encontrado.");
        }

        var idsDosProdutos = quantidadesPorProduto.Keys.ToList();

        var produtos = await _db.Produtos
            .AsNoTracking()
            .Where(p => idsDosProdutos.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var idsAusentes = idsDosProdutos.Where(id => !produtos.ContainsKey(id)).ToList();
        if (idsAusentes.Count > 0)
        {
            return PedidoCreationResult.Failure(
                PedidoCreationError.ProdutoNaoEncontrado,
                $"Produto(s) não encontrado(s): {string.Join(", ", idsAusentes)}.");
        }

        var itens = new List<ItemPedidoResponse>();
        decimal valorTotal = 0;

        foreach (var (produtoId, quantidade) in quantidadesPorProduto)
        {
            var produto = produtos[produtoId];
            var subtotal = produto.PrecoUnitario * quantidade;
            valorTotal += subtotal;

            itens.Add(new ItemPedidoResponse(
                ProdutoId: produtoId,
                NomeProduto: produto.Nome,
                Quantidade: quantidade,
                PrecoUnitario: produto.PrecoUnitario,
                Subtotal: subtotal));
        }

        // PROVISÓRIO: no Bloco 11 aqui entram a baixa de estoque, a gravação e o Commit.
        var pedido = new PedidoCriadoResponse(
            Id: 0,
            ClienteId: request.ClienteId,
            DataPedido: DateTime.UtcNow,
            ValorTotal: valorTotal,
            Itens: itens);

        return PedidoCreationResult.Success(pedido);
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