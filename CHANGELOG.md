# 📋 Changelog

Todas as alterações notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/).

---

## [Unreleased]

### 🚀 Adicionado (Added)
- **Estrutura de Solução (.NET 10):** Criação do arquivo de solução `Bifrost.sln` agrupando os projetos:
  - `Bifrost.Shared`: Biblioteca de classes para DTOs e contratos comuns.
  - `Bifrost.Agent`: Worker Service para leitura do histórico de navegadores.
  - `Bifrost.IngestionApi`: Web API para recepção e gravação de alto desempenho.
  - `Bifrost.Dashboard`: Aplicação Web para auditoria e relatórios.
- **Infraestrutura Docker:** Arquivo `docker-compose.yml` contendo os serviços `sqlserver` (2025), `db-init` (container utilitário para execução do DDL inicial) e `bifrost_api`.
- **Banco de Dados:** Script `sql/init.sql` para criação automática da base `BrowserHistoryDb` e da tabela `BrowserHistory` com restrição de unicidade (`UQ_History_Entry`) e índices de busca.
- **Health Checks:** Endpoint `/health` para verificação de liveness da API e conectividade com o SQL Server.
- **CI/CD:** Pipeline do GitHub Actions em `.github/workflows/ci.yml` atualizado para validação automática de build em .NET 10 e sintaxe do Docker Compose.
- **Documentação:** Arquivos `README.md` com arquitetura/guia de execução e `CHANGELOG.md`.

---

## [0.1.0] - 2026-08-12

### 🚀 Adicionado (Added)
- Setup inicial do repositório, configuração do `.gitignore` para projetos .NET/Docker e definição do fluxo de branches via GitHub Flow.
