# 📋 Changelog

Todas as alterações notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/).

---

## [Unreleased]

## [0.7.0] - 2026-08-28
### 🚀 Adicionado (Added)
*   **Dashboard (Busca e Filtros):** Implementação de busca textual dinâmica com debounce de 500ms na tela de Auditoria (`AuditLog.tsx`).
*   **Dashboard (Filtros Avançados):** Adicionados seletores dinâmicos de usuários e navegadores integrados com as chamadas do `apiService`.
*   **Dashboard (Resiliência e UX):** Tratamento robusto para os estados de carregamento (*loading*), tela vazia (*empty state*) e erros de conexão de rede.
*   **Dashboard (Paginação):** Sistema de paginação local para listagem fluida e organizada de grandes volumes de logs de histórico.

## [0.6.0] - 2026-08-28
### 🚀 Adicionado (Added)
*   **Dashboard (React + Vite):** Recriação do projeto `Bifrost.Dashboard` utilizando a stack **React + Vite + TypeScript** [2], substituindo o template C# anterior.
*   **Dashboard (Interface):** Estilização completa com **Tailwind CSS v4** e **Font Awesome 6**, incluindo um menu de navegação dinâmico com o ícone de arco-íris (Bifrost) 🌈.
*   **Dashboard (Telas):** Implementação da tela de **Visão Geral (`Overview.tsx`)** com cards de métricas e gráficos de navegadores, e da tela de **Logs de Auditoria (`AuditLog.tsx`)** com busca textual e filtros dinâmicos de usuário e máquina.
*   **Dashboard (Configurações):** Criação do serviço cliente de API (`src/services/api.ts`) preparado para ler variáveis de ambiente via arquivos `.env` e `.env.development`.

## [0.5.0] - 2026-08-27
### 🚀 Adicionado (Added)
*   **Agent**: Adicionado suporte à leitura de diretivas de GPO do Active Directory diretamente através do Registro do Windows (`HKLM\SOFTWARE\Policies\Bifrost`).
*   **Tests**: Implementada suíte de testes unitários isolados para o `BifrostWorker` utilizando **NSubstitute**.

### 🔧 Alterado (Changed)
*   **Agent**: Refatorado o ciclo principal do `BifrostWorker` para utilizar as configurações dinâmicas de tempo de varredura (`BrowserOptions`) e as interfaces injetadas de processamento.

## [0.4.0] - 2026-08-26
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

## [0.3.0] - 2026-08-15
### Added
- **Agent**: Criado o repositório de estado `JsonStateRepository` para persistência local em arquivo JSON (`agent_state.json`).
- **Agent**: Implementado o serviço `HistoryProcessor` para orquestração do controle de delta, garantindo o envio apenas de novos registros por navegador/perfil.
- **Tests**: Adicionados testes unitários para o `JsonStateRepository` e para a lógica de filtragem de delta no `HistoryProcessor`.

## [0.2.0] - 2026-08-14
### Added
- **Agent**: Implementado o leitor de histórico `SqliteBrowserHistoryReader` para navegadores Chromium (Chrome, Edge, Brave) e Firefox via SQLite.
- **Agent**: Adicionado suporte à varredura e leitura dinâmica de múltiplos perfis de usuário (`Default`, `Profile 1`, etc.) no Chromium.
- **Tests**: Criado o projeto `Bifrost.Agent.Tests` com suíte de testes unitários utilizando **xUnit** e **Shouldly**.

### Changed
- **Agent**: Otimizado o manuseio de arquivos SQLite temporários com `Mode=ReadOnly;Cache=Shared` e liberação de pools de conexão para prevenir travamentos no sistema operacional.

## [0.1.0] - 2026-08-12
### 🚀 Adicionado (Added)
- Setup inicial do repositório, configuração do `.gitignore` para projetos .NET/Docker e definição do fluxo de branches via GitHub Flow.
