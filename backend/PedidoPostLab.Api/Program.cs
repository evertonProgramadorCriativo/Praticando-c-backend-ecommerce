/*
 * Importa a biblioteca oficial da Microsoft para comunicação e execução
 * de comandos em bancos de dados SQL Server.
 */
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PedidoPostLab.Api.Data;
/**
 * Importa o namespace que contém os endpoints de lookup para clientes e produtos.
 */ 
using PedidoPostLab.Api.Endpoints;

/*
 * Inicializa o construtor da aplicação web (Minimal API), carregando as
 * configurações padrão do ambiente (como appsettings.json e variáveis de ambiente).
 */
var builder = WebApplication.CreateBuilder(args);

/*
 * Busca a string de conexão chamada "DefaultConnection" nas configurações.
 * Caso ela não exista ou esteja vazia, interrompe a inicialização lançando um erro.
 */
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("A connection string 'DefaultConnection' não foi configurada.");

/*
 * Registra o serviço padrão para formatar respostas de erro seguindo a
 * especificação RFC 7807 (Problem Details), deixando os erros da API padronizados.
 */
builder.Services.AddProblemDetails();
/* * Registra o contexto de banco de dados (PedidosDbContext) como um serviço da aplicação,
 * configurando-o para usar o SQL Server com a string de conexão obtida acima.
 */
builder.Services.AddDbContext<PedidosDbContext>(options =>
    options.UseSqlServer(connectionString));

/*
 * Constrói a instância final da aplicação com todos os serviços e configurações definidos acima.
 */
var app = builder.Build();

/*
 * Adiciona middlewares ao pipeline de requisições:
 * - UseExceptionHandler: captura exceções não tratadas e retorna respostas de erro padronizadas.
 * - UseStatusCodePages: garante que respostas de erro sem corpo (ex: 404) retornem detalhes legíveis.
 */
app.UseExceptionHandler();
app.UseStatusCodePages();

/*
 * Define o endpoint GET "/health" para verificar a saúde da API e do banco de dados.
 * Recebe um token de cancelamento (caso o cliente aborte a requisição) e o serviço de logs.
 * conecta-se ao banco de dados, executa uma consulta para contar clientes e produtos,
 * e retorna um JSON com o status da API e do banco.
 */
app.MapGet("/health", async (
    PedidosDbContext db,
    CancellationToken cancellationToken, 
    ILogger<Program> logger) =>
{
    try
    {
        /*
         * Cria e abre uma conexão assíncrona com o SQL Server.
         * O "await using" garante que a conexão será fechada e liberada da memória ao final.
         */
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        /*
         * Prepara uma consulta SQL para contar o total de registros nas tabelas
         * dbo.Clientes e dbo.Produtos em uma única ida ao banco de dados.
         */
        await using var command = new SqlCommand(
            "SELECT (SELECT COUNT(*) FROM dbo.Clientes), (SELECT COUNT(*) FROM dbo.Produtos);",
            connection);

        /*
         * Executa a consulta no banco de dados e lê a primeira (e única) linha retornada.
         */
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

/*
         * Conta a quantidade de produtos usando o Entity Framework, para demonstrar
         * que a API consegue acessar o banco de dados tanto via SQL direto quanto via EF.
         */
           var produtosViaEf = await db.Produtos.CountAsync(cancellationToken);


        /*
         * Retorna status HTTP 200 (OK) com um objeto JSON confirmando que o banco está
         * conectado e exibindo a quantidade de clientes e produtos encontrados.
         */
        return Results.Ok(new
        {
            status = "ok",
            banco = "conectado",
            clientes = reader.GetInt32(0),
            produtos = reader.GetInt32(1),
            produtosViaEf
        });
    }
    catch (SqlException exception)
    {
        /*
         * Caso ocorra qualquer falha de conexão ou erro de SQL, registra um aviso nos logs
         * da aplicação e retorna o status HTTP 503 (Serviço Indisponível) de forma amigável.
         */
        logger.LogWarning(exception, "Não foi possível conectar ao SQL Server.");
        return Results.Problem(
            title: "Banco indisponível",
            detail: "A API não conseguiu conectar ao SQL Server.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

/*
 * Inicia o servidor web e coloca a API para escutar e responder às requisições HTTP.
 */

 /** rotas de lookup para clientes e produtos */
app.MapLookups();

app.Run();

 
