# 📋 Changelog

Todas as alterações notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/).

---

## [Unreleased]

### 🚀 Adicionado (Added)
- **Infraestrutura:** Arquivo `docker-compose.yml` contendo o serviço do SQL Server 2025 e a API de Ingestão.
- **Banco de Dados:** Script `sql/init.sql` para criação automática da base `BrowserHistoryDb` e da tabela `BrowserHistory` com restrição de unicidade (`UQ_History_Entry`) e índices de busca.
- **API de Ingestão:** Esqueleto da aplicação em .NET 8 (`IngestionApi`).
- **Health Checks:** Endpoint `/health` para verificação de liveness da API e `/health/ready` para verificação de conectividade com o SQL Server.
- **CI/CD:** Pipeline do GitHub Actions em `.github/workflows/ci.yml` para validação automática de build do .NET e sintaxe do Docker Compose.
- **Documentação:** Arquivos `README.md` com arquitetura/guia de execução e `CHANGELOG.md`.

---

## [0.1.0] - 2026-08-12

### 🚀 Adicionado (Added)
- Setup inicial do repositório, configuração do `.gitignore` para projetos .NET/Docker e definição do fluxo de branches via GitHub Flow.
