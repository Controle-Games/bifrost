import { useState, useEffect, useMemo } from 'react';
import { apiService } from '../services/api';
import type { BrowserHistoryItem, DashboardMetrics } from '../types';

export default function Overview() {
  const [metrics, setMetrics] = useState<DashboardMetrics | null>(null);
  const [recentLogs, setRecentLogs] = useState<BrowserHistoryItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadDashboardData() {
      setLoading(true);
      setError(null);
      try {
        // Executa as chamadas da API em paralelo para melhor performance
        const [metricsData, historyData] = await Promise.all([
          apiService.getMetrics(),
          apiService.getHistory()
        ]);
        
        setMetrics(metricsData);
        // Exibe apenas as 5 atividades mais recentes
        setRecentLogs(historyData.slice(0, 5));
      } catch (err) {
        setError('Não foi possível carregar os dados em tempo real do Bifrost.');
        console.error(err);
      } finally {
        setLoading(false);
      }
    }

    loadDashboardData();
  }, []);

  // Calcula a distribuição dos navegadores dinamicamente com base nas coletas
  const browserDistribution = useMemo(() => {
    if (recentLogs.length === 0) return { chrome: 0, edge: 0, firefox: 0, other: 0 };
    
    const total = recentLogs.length;
    const chrome = recentLogs.filter(l => l.browser.toLowerCase() === 'chrome').length;
    const edge = recentLogs.filter(l => l.browser.toLowerCase() === 'edge').length;
    const firefox = recentLogs.filter(l => l.browser.toLowerCase() === 'firefox').length;
    
    const chromePct = Math.round((chrome / total) * 100);
    const edgePct = Math.round((edge / total) * 100);
    const firefoxPct = Math.round((firefox / total) * 100);
    const otherPct = Math.max(0, 100 - (chromePct + edgePct + firefoxPct));

    return { chrome: chromePct, edge: edgePct, firefox: firefoxPct, other: otherPct };
  }, [recentLogs]);

  const getBrowserIcon = (browser: string) => {
    switch (browser.toLowerCase()) {
      case 'chrome': return <i className="fa-brands fa-chrome text-amber-500"></i>;
      case 'edge': return <i className="fa-brands fa-edge text-blue-400"></i>;
      case 'firefox': return <i className="fa-brands fa-firefox-browser text-orange-500"></i>;
      default: return <i className="fa-solid fa-globe text-slate-400"></i>;
    }
  };

  if (loading) {
    return (
      <div className="py-20 flex flex-col items-center justify-center text-slate-400 gap-4">
        <i className="fa-solid fa-circle-notch fa-spin text-3xl text-blue-500"></i>
        <span className="text-sm font-medium">Carregando métricas de auditoria...</span>
      </div>
    );
  }

  if (error || !metrics) {
    return (
      <div className="py-16 flex flex-col items-center justify-center text-rose-500 gap-3 bg-slate-900 border border-slate-800 rounded-xl">
        <i className="fa-solid fa-triangle-exclamation text-4xl"></i>
        <span className="font-semibold">{error}</span>
        <button 
          onClick={() => window.location.reload()}
          className="mt-2 px-4 py-2 bg-slate-800 text-slate-200 text-xs rounded-lg hover:bg-slate-700 hover:text-white transition-colors"
        >
          Recarregar Painel
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-8 animate-fade-in">
      {/* Cards de Métricas */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-slate-900 border border-slate-800 p-6 rounded-xl flex items-center justify-between shadow-lg">
          <div>
            <span className="text-sm text-slate-400 font-medium">Total de Acessos Coletados</span>
            <h3 className="text-3xl font-bold mt-2 text-white">{metrics.totalAcessos.toLocaleString('pt-BR')}</h3>
          </div>
          <div className="w-12 h-12 rounded-lg bg-blue-500/10 flex items-center justify-center border border-blue-500/20">
            <i className="fa-solid fa-chart-line text-blue-500 text-xl"></i>
          </div>
        </div>

        <div className="bg-slate-900 border border-slate-800 p-6 rounded-xl flex items-center justify-between shadow-lg">
          <div>
            <span className="text-sm text-slate-400 font-medium">Agentes Ativos (AD/GPO)</span>
            <h3 className="text-3xl font-bold mt-2 text-emerald-400">{metrics.agentesAtivos}</h3>
          </div>
          <div className="w-12 h-12 rounded-lg bg-emerald-500/10 flex items-center justify-center border border-emerald-500/20">
            <i className="fa-solid fa-computer text-emerald-400 text-xl"></i>
          </div>
        </div>

        <div className="bg-slate-900 border border-slate-800 p-6 rounded-xl flex items-center justify-between shadow-lg">
          <div>
            <span className="text-sm text-slate-400 font-medium">Alertas/Bloqueios Ativos</span>
            <h3 className="text-3xl font-bold mt-2 text-rose-500">{metrics.sitesBloqueadosAlertas}</h3>
          </div>
          <div className="w-12 h-12 rounded-lg bg-rose-500/10 flex items-center justify-center border border-rose-500/20">
            <i className="fa-solid fa-triangle-exclamation text-rose-500 text-xl"></i>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Distribuição de Navegadores */}
        <div className="lg:col-span-1 bg-slate-900 border border-slate-800 p-6 rounded-xl shadow-lg flex flex-col justify-between">
          <div>
            <h4 className="text-lg font-semibold mb-4 text-white">Navegadores Monitorados</h4>
            <div className="space-y-4">
              <div>
                <div className="flex justify-between text-sm mb-1">
                  <span className="flex items-center gap-2">{getBrowserIcon('chrome')} Chrome</span>
                  <span className="font-semibold">{browserDistribution.chrome}%</span>
                </div>
                <div className="w-full bg-slate-800 h-2 rounded-full overflow-hidden">
                  <div className="bg-amber-500 h-full rounded-full" style={{ width: `${browserDistribution.chrome}%` }}></div>
                </div>
              </div>
              
              <div>
                <div className="flex justify-between text-sm mb-1">
                  <span className="flex items-center gap-2">{getBrowserIcon('edge')} Edge</span>
                  <span className="font-semibold">{browserDistribution.edge}%</span>
                </div>
                <div className="w-full bg-slate-800 h-2 rounded-full overflow-hidden">
                  <div className="bg-blue-400 h-full rounded-full" style={{ width: `${browserDistribution.edge}%` }}></div>
                </div>
              </div>
              
              <div>
                <div className="flex justify-between text-sm mb-1">
                  <span className="flex items-center gap-2">{getBrowserIcon('firefox')} Firefox</span>
                  <span className="font-semibold">{browserDistribution.firefox}%</span>
                </div>
                <div className="w-full bg-slate-800 h-2 rounded-full overflow-hidden">
                  <div className="bg-orange-500 h-full rounded-full" style={{ width: `${browserDistribution.firefox}%` }}></div>
                </div>
              </div>

              {browserDistribution.other > 0 && (
                <div>
                  <div className="flex justify-between text-sm mb-1">
                    <span className="flex items-center gap-2">{getBrowserIcon('other')} Outros</span>
                    <span className="font-semibold">{browserDistribution.other}%</span>
                  </div>
                  <div className="w-full bg-slate-800 h-2 rounded-full overflow-hidden">
                    <div className="bg-slate-500 h-full rounded-full" style={{ width: `${browserDistribution.other}%` }}></div>
                  </div>
                </div>
              )}
            </div>
          </div>
          
          <p className="text-xs text-slate-500 mt-6 italic">
            * Proporções de uso baseadas nas coletas recebidas no banco de dados [1, 2].
          </p>
        </div>

        {/* Últimas Atividades */}
        <div className="lg:col-span-2 bg-slate-900 border border-slate-800 p-6 rounded-xl shadow-lg">
          <h4 className="text-lg font-semibold mb-4 text-white">Últimas Atividades Coletadas</h4>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-300">
              <thead className="text-xs uppercase bg-slate-800/50 text-slate-400 border-b border-slate-800">
                <tr>
                  <th className="py-3 px-4">Usuário / Máquina</th>
                  <th className="py-3 px-4">Navegador</th>
                  <th className="py-3 px-4">Título / URL</th>
                  <th className="py-3 px-4 text-right">Horário</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800/50">
                {recentLogs.length > 0 ? (
                  recentLogs.map((log) => (
                    <tr key={log.id} className="hover:bg-slate-800/20 transition-colors">
                      <td className="py-3 px-4">
                        <div className="font-medium text-white">{log.user}</div>
                        <div className="text-xs text-slate-500">{log.machineName}</div>
                      </td>
                      <td className="py-3 px-4">{getBrowserIcon(log.browser)} <span className="ml-1.5">{log.browser}</span></td>
                      <td className="py-3 px-4 max-w-xs truncate">
                        <div className="font-medium text-slate-200 truncate">{log.title || 'Sem título'}</div>
                        <div className="text-xs text-blue-400 hover:underline truncate">{log.url}</div>
                      </td>
                      <td className="py-3 px-4 text-right text-xs text-slate-400">
                        {new Date(log.accessedAt).toLocaleTimeString('pt-BR')}
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={4} className="py-8 text-center text-slate-500">
                      Nenhum histórico de navegação coletado recentemente.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}
