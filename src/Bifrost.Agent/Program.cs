using Bifrost.Agent.Configuration;
using Bifrost.Agent.Services;
using Bifrost.Agent.Worker;
using Microsoft.Win32;
using System.Runtime.InteropServices;

var builder = Host.CreateApplicationBuilder(args);

// 1. Mapeia a seção BrowserOptions do appSettings
builder.Services.Configure<BrowserOptions>(
    builder.Configuration.GetSection(BrowserOptions.SectionName)
);

// 2. Lê nativamente do Registro do Windows
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    try
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Bifrost");
        if (key != null)
        {
            var intervalValue = key.GetValue("IntervalInMinutes");
            if (intervalValue != null)
            {
                // Sobrescreve o valor da configuração
                builder.Configuration["BrowserOptions:IntervalInMinutes"] = intervalValue.ToString();
            }
        }
    }
    catch (Exception ex)
    {
        // Evita que falhas de permissão de leitura de registro quebrem a inicialização
        Console.WriteLine($"Aviso: Não foi possível ler as diretivas do Registro: {ex.Message}");
    }
}

// Registros das Injeções de Dependências
builder.Services.AddTransient<IBrowserHistoryReader, SqliteBrowserHistoryReader>();
builder.Services.AddTransient<IStateRepository, JsonStateRepository>();
builder.Services.AddTransient<IApiClient, ApiClient>();
builder.Services.AddTransient<IHistoryProcessor, HistoryProcessor>();

// Registra o Worker
builder.Services.AddHostedService<BifrostWorker>();

var host = builder.Build();
host.Run();
