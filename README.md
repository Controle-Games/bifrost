# 🌈 Bifrost — Browser History Collector & Dashboard

Sistema distribuído para coleta, ingestão e visualização consolidada de históricos de navegação (Google Chrome, Microsoft Edge e Mozilla Firefox) a partir de estações de trabalho Windows via GPO.

---

## 🏗️ Arquitetura e Componentes

A solução é dividida em três camadas isoladas para garantir segurança, escalabilidade e facilidade de manutenção:

[Bifrost Agent (C#)] --> HTTPS/Rest --> [Bifrost Igestion API (.NET 8)] --> [SQL Server 2025]

[Bifrost Dashboard] -- Read-Only API / Direct --|

1. **Bifrost Agent (Service):** Coletor em C# que lê arquivos SQLite locais dos navegadores via *Shadow Copy* e envia apenas deltas de dados.
2. **Bifrost Ingestion API (.NET 8):** API centralizada para validação e persistência de dados. Isolada em container Docker.
3. **Banco de Dados (SQL Server 2025):** Persistência de dados indexados por usuário, máquina e data em container Docker.
4. **Bifrost Dashboard:** Painel de visualização com suporte a filtros e busca textual de acessos.

---

## 🚀 Como Executar o Ambiente de Desenvolvimento

### Pré-requisitos
* [Docker Desktop](https://www.docker.com/) instalado e rodando.
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado.
* [Git](https://git-scm.com/) configurado.

### 1. Clonar o Repositório
```bash
git clone [https://github.com/SEU-USUARIO/bifrost.git](https://github.com/SEU-USUARIO/bifrost.git)
cd bifrost
