-- Verifica se o banco de dados 'PedidoPostLab' ainda não existe no servidor
IF DB_ID(N'PedidoPostLab') IS NULL
    -- Cria o banco de dados 'PedidoPostLab'
    CREATE DATABASE PedidoPostLab;
-- Encerra o lote atual de comandos para garantir que o banco seja criado antes de ser usado
GO

-- Muda o contexto da conexão para utilizar o banco de dados recém-criado
USE PedidoPostLab;
-- Encerra o lote atual de comandos
GO

-- Verifica se a tabela de usuários ('U') 'dbo.Clientes' ainda não existe
IF OBJECT_ID(N'dbo.Clientes', N'U') IS NULL
-- Inicia o bloco de execução condicional para criação da tabela e seus índices
BEGIN
    -- Cria a tabela 'dbo.Clientes'
    CREATE TABLE dbo.Clientes (
        -- Coluna de identificação única, autoincrementável (começa em 1, incrementa de 1 em 1) e chave primária
        Id    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clientes PRIMARY KEY,
        -- Nome do cliente (texto Unicode de até 150 caracteres), preenchimento obrigatório
        Nome  NVARCHAR(150)     NOT NULL,
        -- E-mail do cliente (texto Unicode de até 200 caracteres), preenchimento obrigatório
        Email NVARCHAR(200)     NOT NULL
    );
    -- Cria um índice único na coluna Email para impedir cadastros duplicados e acelerar buscas
    CREATE UNIQUE INDEX IX_Clientes_Email ON dbo.Clientes (Email);
-- Finaliza o bloco de execução condicional
END
-- Encerra o lote atual de comandos
GO

-- Verifica se a tabela 'dbo.Produtos' ainda não existe
IF OBJECT_ID(N'dbo.Produtos', N'U') IS NULL
-- Inicia o bloco de execução condicional
BEGIN
    -- Cria a tabela 'dbo.Produtos'
    CREATE TABLE dbo.Produtos (
        -- Chave primária autoincrementável do produto
        Id                INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Produtos PRIMARY KEY,
        -- Nome do produto (texto Unicode de até 150 caracteres), obrigatório
        Nome              NVARCHAR(150)     NOT NULL,
        -- Preço unitário com precisão de até 18 dígitos e 2 casas decimais, obrigatório
        PrecoUnitario     DECIMAL(18,2)     NOT NULL,
        -- Quantidade disponível em estoque (número inteiro), obrigatório
        EstoqueDisponivel INT               NOT NULL
    );
-- Finaliza o bloco de execução condicional
END
-- Encerra o lote atual de comandos
GO

-- Verifica se a tabela 'dbo.Pedidos' ainda não existe
IF OBJECT_ID(N'dbo.Pedidos', N'U') IS NULL
-- Inicia o bloco de execução condicional
BEGIN
    -- Cria a tabela 'dbo.Pedidos'
    CREATE TABLE dbo.Pedidos (
        -- Chave primária autoincrementável do pedido
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pedidos PRIMARY KEY,
        -- Chave estrangeira que referencia o cliente dono do pedido, obrigatória
        ClienteId   INT               NOT NULL,
        -- Data e hora exata em que o pedido foi realizado, obrigatória
        DataPedido  DATETIME2         NOT NULL,
        -- Valor total calculado do pedido (2 casas decimais), obrigatório
        ValorTotal  DECIMAL(18,2)     NOT NULL,
        -- Define a restrição de chave estrangeira ligando 'ClienteId' ao 'Id' da tabela 'dbo.Clientes'
        CONSTRAINT FK_Pedidos_Clientes FOREIGN KEY (ClienteId) REFERENCES dbo.Clientes (Id)
    );
    -- Cria um índice na data do pedido incluindo ClienteId e ValorTotal para otimizar relatórios por período (Covering Index)
    CREATE INDEX IX_Pedidos_DataPedido
        ON dbo.Pedidos (DataPedido) INCLUDE (ClienteId, ValorTotal);
    -- Cria um índice composto para acelerar consultas de pedidos filtrados por cliente e ordenados/filtrados por data
    CREATE INDEX IX_Pedidos_ClienteId_DataPedido
        ON dbo.Pedidos (ClienteId, DataPedido);
-- Finaliza o bloco de execução condicional
END
-- Encerra o lote atual de comandos
GO

-- Verifica se a tabela associativa 'dbo.ItensPedido' ainda não existe
IF OBJECT_ID(N'dbo.ItensPedido', N'U') IS NULL
-- Inicia o bloco de execução condicional
BEGIN
    -- Cria a tabela 'dbo.ItensPedido' (relacionamento N:N entre Pedidos e Produtos)
    CREATE TABLE dbo.ItensPedido (
        -- Chave primária autoincrementável do item do pedido
        Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ItensPedido PRIMARY KEY,
        -- Referência ao pedido ao qual este item pertence, obrigatória
        PedidoId      INT               NOT NULL,
        -- Referência ao produto comprado neste item, obrigatória
        ProdutoId     INT               NOT NULL,
        -- Quantidade comprada deste produto, obrigatória
        Quantidade    INT               NOT NULL,
        -- Preço unitário praticado no momento da compra (preserva histórico caso o preço do produto mude depois)
        PrecoUnitario DECIMAL(18,2)     NOT NULL,
        -- Chave estrangeira que garante que 'PedidoId' exista na tabela 'dbo.Pedidos'
        CONSTRAINT FK_ItensPedido_Pedidos  FOREIGN KEY (PedidoId)  REFERENCES dbo.Pedidos (Id),
        -- Chave estrangeira que garante que 'ProdutoId' exista na tabela 'dbo.Produtos'
        CONSTRAINT FK_ItensPedido_Produtos FOREIGN KEY (ProdutoId) REFERENCES dbo.Produtos (Id)
    );
    -- Cria índice na chave estrangeira PedidoId para acelerar junções (JOINs) entre Pedidos e seus Itens
    CREATE INDEX IX_ItensPedido_PedidoId  ON dbo.ItensPedido (PedidoId);
    -- Cria índice na chave estrangeira ProdutoId para acelerar consultas de histórico de vendas por produto
    CREATE INDEX IX_ItensPedido_ProdutoId ON dbo.ItensPedido (ProdutoId);
-- Finaliza o bloco de execução condicional
END
-- Encerra o lote atual de comandos
GO

-- Verifica se a tabela 'dbo.Clientes' está vazia antes de inserir dados iniciais (evita duplicidade ao reexecutar o script)
IF NOT EXISTS (SELECT 1 FROM dbo.Clientes)
    -- Insere três registros iniciais de clientes para testes/carga inicial
    INSERT INTO dbo.Clientes (Nome, Email) VALUES
        -- Primeiro cliente (suporte a caracteres Unicode com prefixo N)
        (N'Ana Souza',    N'ana@exemplo.com'),
        -- Segundo cliente
        (N'Bruno Lima',   N'bruno@exemplo.com'),
        -- Terceiro cliente
        (N'Carla Mendes', N'carla@exemplo.com');
-- Encerra o lote atual de comandos
GO

-- Verifica se a tabela 'dbo.Produtos' está vazia antes de popular o catálogo inicial
IF NOT EXISTS (SELECT 1 FROM dbo.Produtos)
    -- Insere quatro produtos iniciais com seus respectivos preços e estoques
    INSERT INTO dbo.Produtos (Nome, PrecoUnitario, EstoqueDisponivel) VALUES
        -- Produto 1: Teclado Mecânico
        (N'Teclado Mecânico', 150.00,  10),
        -- Produto 2: Mouse Sem Fio
        (N'Mouse Sem Fio',     80.50,  25),
        -- Produto 3: Monitor 24"
        (N'Monitor 24"',      899.90,   3),
        -- Produto 4: Cabo HDMI
        (N'Cabo HDMI',         25.00, 100);
-- Encerra o lote final de comandos
GO
