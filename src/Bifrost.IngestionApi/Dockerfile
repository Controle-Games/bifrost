# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Copia os arquivos de projeto para restore de dependências
COPY ["shared/Bifrost.Shared/Bifrost.Shared.csproj", "shared/Bifrost.Shared/"]
COPY ["src/Bifrost.IngestionApi/Bifrost.IngestionApi.csproj", "src/Bifrost.IngestionApi/"]

# 2. Executa o restore das dependências
RUN dotnet restore "src/Bifrost.IngestionApi/Bifrost.IngestionApi.csproj"

# 3. Copia todo o código fonte e faz a compilação
COPY . .

# 4. Entra na pasta do projeto da API e compila
WORKDIR "src/Bifrost.IngestionApi"
RUN dotnet build "Bifrost.IngestionApi.csproj" -c Release -o /app/build
RUN dotnet publish "Bifrost.IngestionApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio Final / Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Bifrost.IngestionApi.dll"]
