import { useState } from "react";
import { type BrowserHistoryItem } from "../types";

// Dados estáticos para validação visual e demonstração
const MOCK_RECENT_LOGS: BrowserHistoryItem[] = [
  { id: '1', user: 'rodrigo.limao', machineName: 'NOTE-DEV-01', url: 'https://github.com/controle-games/bifrost', title: 'Controle-Games/bifrost: Aplicativo de monitoramento', accessedAt: '2026-08-28T10:15:30Z', browser: 'Chrome' },
  { id: '2', user: 'ana.silva', machineName: 'NOTE-FIN-03', url: 'https://bancobrasil.com.br', title: 'Banco do Brasil | Atendimento', accessedAt: '2026-08-28T10:14:22Z', browser: 'Edge' },
  { id: '3', user: 'rodrigo.limao', machineName: 'NOTE-DEV-01', url: 'https://stackoverflow.com', title: 'How to use Tailwind v4 with Vite', accessedAt: '2026-08-28T10:12:05Z', browser: 'Firefox' },
  { id: '4', user: 'paulo.santos', machineName: 'DESKTOP-DIR-05', url: 'https://youtube.com', title: 'Treinamento de Segurança GPO', accessedAt: '2026-08-28T09:58:40Z', browser: 'Chrome' },
];

export default function Overview() {
  const [metrics] = useState({
    totalAcessos: 12450,
    agentesAtivos: 18,
    alertasBloqueios: 3
  });

  // Função auxiliar para renderizar o ícone correto do navegador
  const getBrowserIcon = (browser: string) => {
    switch (browser.toLowerCase()) {
      case 'chrome':
        return <i className="fa-brands fa-chrome text-amber-500"></i>;
      case 'edge':
        return <i className="fa-brands fa-edge text-blue-400"></i>;
      case 'firefox':
        return <i className="fa-brands fa-firefox-browser text-orange-500"></i>;
      default:
        return <i className="fa-solid fa-globe text-slate-400"></i>;
    }
  };

  return (
    <div className="space-y-8 animate-fade-in">
      {/* Grid de Cards de Métricas */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* Card 1 */}
        <div className="bg-slate-900 border border-slate-800 p-6 rounded-xl flex items-center justify-between">
          <div>
            <span className="text-sm text-slate-400 font-medium">Total de Acessos Coletados</span>
            <h3 className="text-3xl font-bold mt-2 text-white">{metrics.totalAcessos.toLocaleString('pt-BR')}</h3>
          </div>
          <div className="w-12 h-12 rounded-lg bg-blue-500/10 flex items-center justify-center border border-blue-500/20">
            <i className="fa-solid fa-chart-line text-blue-500 text-xl"></i>
          </div>
        </div>

        {/* Card 2 */}
        <div className="bg-slate-900 border border-slate-800 p-6 rounded-xl flex items-center justify-between">
          <div>
            <span className="text-sm text-slate-400 font-medium">Agentes Ativos (AD/GPO)</span>
            <h3 className="text-3xl font-bold mt-2 text-emerald-400">{metrics.agentesAtivos}</h3>
          </div>
          <div className="w-12 h-12 rounded-lg bg-emerald-500/10 flex items-center justify-center border border-emerald-500/20">
            <i className="fa-solid fa-computer text-emerald-400 text-xl text-pulse"></i>
          </div>
        </div>

        {/* Card 3 */}
        <div className="bg-slate-900 border border-slate-800 p-6 rounded-xl flex items-center justify-between">
          <div>
            <span className="text-sm text-slate-400 font-medium">Alertas/Bloqueios Ativos</span>
            <h3 className="text-3xl font-bold mt-2 text-rose-500">{metrics.alertasBloqueios}</h3>
          </div>
          <div className="w-12 h-12 rounded-lg bg-rose-500/10 flex items-center justify-center border border-rose-500/20">
            <i className="fa-solid fa-triangle-exclamation text-rose-500 text-xl"></i>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Gráfico/Progresso de Uso dos Navegadores */}
        <div className="lg:col-span-1 bg-slate-900 border border-slate-800 p-6 rounded-xl">
          <h4 className="text-lg font-semibold mb-4 text-white">Navegadores Monitorados</h4>
          <div className="space-y-4">
            <div>
              <div className="flex justify-between text-sm mb-1">
                <span className="flex items-center gap-2">{getBrowserIcon('chrome')} Chrome</span>
                <span className="font-semibold">65%</span>
              </div>
              <div className="w-full bg-slate-800 h-2 rounded-full overflow-hidden">
                <div className="bg-amber-500 h-full rounded-full" style={{ width: '65%' }}></div>
              </div>
            </div>
            <div>
              <div className="flex justify-between text-sm mb-1">
                <span className="flex items-center gap-2">{getBrowserIcon('edge')} Edge</span>
                <span className="font-semibold">25%</span>
              </div>
              <div className="w-full bg-slate-800 h-2 rounded-full overflow-hidden">
                <div className="bg-blue-400 h-full rounded-full" style={{ width: '25%' }}></div>
              </div>
            </div>
            <div>
              <div className="flex justify-between text-sm mb-1">
                <span className="flex items-center gap-2">{getBrowserIcon('firefox')} Firefox</span>
                <span className="font-semibold">10%</span>
              </div>
              <div className="w-full bg-slate-800 h-2 rounded-full overflow-hidden">
                <div className="bg-orange-500 h-full rounded-full" style={{ width: '10%' }}></div>
              </div>
            </div>
          </div>
        </div>

        {/* Últimas Atividades Coletadas */}
        <div className="lg:col-span-2 bg-slate-900 border border-slate-800 p-6 rounded-xl">
          <h4 className="text-lg font-semibold mb-4 text-white">Atividades Recentes dos Agentes</h4>
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
                {MOCK_RECENT_LOGS.map((log) => (
                  <tr key={log.id} className="hover:bg-slate-800/20 transition-colors">
                    <td className="py-3 px-4">
                      <div className="font-medium text-white">{log.user}</div>
                      <div className="text-xs text-slate-500">{log.machineName}</div>
                    </td>
                    <td className="py-3 px-4">{getBrowserIcon(log.browser)} <span className="ml-1">{log.browser}</span></td>
                    <td className="py-3 px-4 max-w-xs truncate">
                      <div className="font-medium text-slate-200 truncate">{log.title}</div>
                      <div className="text-xs text-blue-400 hover:underline truncate">{log.url}</div>
                    </td>
                    <td className="py-3 px-4 text-right text-xs text-slate-400">
                      {new Date(log.accessedAt).toLocaleTimeString('pt-BR')}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}
