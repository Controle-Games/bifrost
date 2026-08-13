-- 1. Criar o banco se não existir
IF NOT EXISTS (SELECT * FROM sys.database WHERE name = 'BrowserHistoryDb')
    BEGIN
        CREATE DATABASE BrowserHistoryDb;
    END
GO

USE BrowserHistoryDb;
GO

-- 2. Criar a tabela principal
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BrowserHistory')
BEGIN
    CREATE TABLE BrowserHistory (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        MachineName VARCHAR(100) NOT NULL,
        UserName VARCHAR(100) NOT NULL,
        Browser VARCHAR(50) NOT NULL, -- Chrome, Edge, Firefox
        Url VARCHAR(2048) NOT NULL,
        Title NVARCHAR(1000) NULL,
        VisitedAt DATETIME2 NOT NULL, -- Data/Hora UTC em que o site foi visitado
        ProcessedAt DATETIME2 NOT NULL DEFAULT GETCURRENT() -- Data/Hora UTC em que foi gravado na API

        -- Restrição de Unicidade para evitar duplicações na ingestão
        CONSTRAINT UQ_History_Entry UNIQUE (MachineName, UserName, Browser, Url, VisitedAt)
    );
END
GO

-- 3. Índices de Performance para o Dashboard e Consultas da API
-- Busca rápida por máquina e usuário
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BrowserHistory_Machine_User')
BEGIN
    CREATE INDEX IX_BrowserHistory_Machine_User
    ON BrowserHistory (MachineName, UserName);
END
GO

-- Filtros por intervalo de datas/período de navegação
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BrowserHistory_VisitedAt')
BEGIN
    CREATE INDEX IX_BrowserHistory_VisitedAt
    ON BrowserHistory (VisitedAt DESC);
END
GO

-- Filtro por navegador
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BrowserHistory_Browser')
BEGIN
    CREATE INDEX IX_BrowserHistory_Browser
    ON BrowserHistory (Browser);
END
GO
