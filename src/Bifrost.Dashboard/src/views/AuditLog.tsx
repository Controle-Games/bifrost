import { useState, useMemo } from "react";
import { type BrowserHistoryItem } from "../types";

// Dados estáticos para validação visual e demonstração
const MOCK_AUDIT_LOGS: BrowserHistoryItem[] = [
  { id: '1', user: 'rodrigo.limao', machineName: 'NOTE-DEV-01', url: 'https://github.com/controle-games/bifrost', title: 'Controle-Games/bifrost: Aplicativo de monitoramento', accessedAt: '2026-08-28T10:15:30Z', browser: 'Chrome' },
  { id: '2', user: 'ana.silva', machineName: 'NOTE-FIN-03', url: 'https://bancobrasil.com.br', title: 'Banco do Brasil | Atendimento', accessedAt: '2026-08-28T10:14:22Z', browser: 'Edge' },
  { id: '3', user: 'rodrigo.limao', machineName: 'NOTE-DEV-01', url: 'https://stackoverflow.com', title: 'How to use Tailwind v4 with Vite', accessedAt: '2026-08-28T10:12:05Z', browser: 'Firefox' },
  { id: '4', user: 'paulo.santos', machineName: 'DESKTOP-DIR-05', url: 'https://youtube.com', title: 'Treinamento de Segurança GPO', accessedAt: '2026-08-28T09:58:40Z', browser: 'Chrome' },
  { id: '5', user: 'ana.silva', machineName: 'NOTE-FIN-03', url: 'https://gmail.com', title: 'Caixa de Entrada - Trabalho', accessedAt: '2026-08-28T09:45:10Z', browser: 'Edge' },
  { id: '6', user: 'paulo.santos', machineName: 'DESKTOP-DIR-05', url: 'https://g1.globo.com', title: 'G1 - O Portal de Notícias da Globo', accessedAt: '2026-08-28T09:30:15Z', browser: 'Chrome' },
];

export default function AuditLog() {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedBrowser, setSelectedBrowser] = useState('');
  const [selectedUser, setSelectedUser] = useState('');

  // Filtros em tempo real para a demonstração
  const filteredLogs = useMemo(() => {
    return MOCK_AUDIT_LOGS.filter(log => {
      const matchesSearch = log.title.toLowerCase().includes(searchTerm.toLowerCase()) || 
                            log.url.toLowerCase().includes(searchTerm.toLowerCase());
      const matchesBrowser = selectedBrowser ? log.browser === selectedBrowser : true;
      const matchesUser = selectedUser ? log.user === selectedUser : true;
      return matchesSearch && matchesBrowser && matchesUser;
    });
  }, [searchTerm, selectedBrowser, selectedUser]);

  const getBrowserIcon = (browser: string) => {
    switch (browser.toLowerCase()) {
      case 'chrome': return <i className="fa-brands fa-chrome text-amber-500"></i>;
      case 'edge': return <i className="fa-brands fa-edge text-blue-400"></i>;
      case 'firefox': return <i className="fa-brands fa-firefox-browser text-orange-500"></i>;
      default: return <i className="fa-solid fa-globe text-slate-400"></i>;
    }
  };

  return (
    <div className="space-y-6">
      {/* Barra de Filtros */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 bg-slate-900 border border-slate-800 p-4 rounded-xl">
        <div className="relative">
          <input
            type="text"
            placeholder="Buscar por título ou URL..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full bg-slate-950 border border-slate-800 rounded-lg px-4 py-2 text-sm text-white focus:outline-none focus:border-blue-500"
          />
        </div>
        
        <div>
          <select
            value={selectedBrowser}
            onChange={(e) => setSelectedBrowser(e.target.value)}
            className="w-full bg-slate-950 border border-slate-800 rounded-lg px-4 py-2 text-sm text-slate-300 focus:outline-none focus:border-blue-500"
          >
            <option value="">Todos os Navegadores</option>
            <option value="Chrome">Chrome</option>
            <option value="Edge">Edge</option>
            <option value="Firefox">Firefox</option>
          </select>
        </div>

        <div>
          <select
            value={selectedUser}
            onChange={(e) => setSelectedUser(e.target.value)}
            className="w-full bg-slate-950 border border-slate-800 rounded-lg px-4 py-2 text-sm text-slate-300 focus:outline-none focus:border-blue-500"
          >
            <option value="">Todos os Usuários</option>
            <option value="rodrigo.limao">rodrigo.limao</option>
            <option value="ana.silva">ana.silva</option>
            <option value="paulo.santos">paulo.santos</option>
          </select>
        </div>
      </div>

      {/* Tabela de Resultados */}
      <div className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden">
        <table className="w-full text-left text-sm text-slate-300">
          <thead className="text-xs uppercase bg-slate-800/50 text-slate-400 border-b border-slate-800">
            <tr>
              <th className="py-3 px-4">Usuário / Máquina</th>
              <th className="py-3 px-4">Navegador</th>
              <th className="py-3 px-4">Título / URL</th>
              <th className="py-3 px-4 text-right">Data/Hora</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-800/50">
            {filteredLogs.length > 0 ? (
              filteredLogs.map((log) => (
                <tr key={log.id} className="hover:bg-slate-800/20 transition-colors">
                  <td className="py-3 px-4">
                    <div className="font-medium text-white">{log.user}</div>
                    <div className="text-xs text-slate-500">{log.machineName}</div>
                  </td>
                  <td className="py-3 px-4">
                    {getBrowserIcon(log.browser)} <span className="ml-1">{log.browser}</span>
                  </td>
                  <td className="py-3 px-4 max-w-md truncate">
                    <div className="font-medium text-slate-200 truncate">{log.title}</div>
                    <a href={log.url} target="_blank" rel="noreferrer" className="text-xs text-blue-400 hover:underline truncate block">
                      {log.url}
                    </a>
                  </td>
                  <td className="py-3 px-4 text-right text-xs text-slate-400">
                    {new Date(log.accessedAt).toLocaleString('pt-BR')}
                  </td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan={4} className="py-8 text-center text-slate-500">
                  Nenhum registro encontrado para os filtros selecionados.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
