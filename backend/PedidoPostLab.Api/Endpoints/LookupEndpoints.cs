using Microsoft.EntityFrameworkCore;
using PedidoPostLab.Api.Data;

namespace PedidoPostLab.Api.Endpoints;

public static class LookupEndpoints
{
    public static IEndpointRouteBuilder MapLookups(this IEndpointRouteBuilder app)
    {
        app.MapGet("/clientes", async (PedidosDbContext db, CancellationToken cancellationToken) =>
        {
            var clientes = await db.Clientes
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .Select(c => new { c.Id, c.Nome, c.Email })
                .ToListAsync(cancellationToken);

            return Results.Ok(clientes);
        });

        app.MapGet("/produtos", async (PedidosDbContext db, CancellationToken cancellationToken) =>
        {
            var produtos = await db.Produtos
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .Select(p => new { p.Id, p.Nome, p.PrecoUnitario, p.EstoqueDisponivel })
                .ToListAsync(cancellationToken);

            return Results.Ok(produtos);
        });

        return app;
    }
}