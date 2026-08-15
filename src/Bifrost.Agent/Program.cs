using Bifrost.Agent;
using Bifrost.Agent.Configuration;
using Bifrost.Agent.Services;

var builder = Host.CreateApplicationBuilder(args);

// Mapeia as configurações (BrowserOptions) do appsettings.json
builder.Services.Configure<BrowserOptions>(
    builder.Configuration.GetSection(BrowserOptions.SectionName)
);

// Registros das Injeções de Dependências
builder.Services.AddTransient<IBrowserHistoryReader, SqliteBrowserHistoryReader>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
