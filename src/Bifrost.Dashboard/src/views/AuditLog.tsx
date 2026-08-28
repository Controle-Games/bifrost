import { useState, useEffect, useMemo } from 'react';
import { apiService } from '../services/api';
import type { BrowserHistoryItem } from '../types';

export default function AuditLog() {
  const [logs, setLogs] = useState<BrowserHistoryItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Estados dos filtros
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [selectedBrowser, setSelectedBrowser] = useState('');
  const [selectedUser, setSelectedUser] = useState('');

  // Estados de Paginação
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 10;

  // 1. Efeito de Debounce para a barra de buscas (aguarda 500ms sem digitação)
  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedSearch(searchTerm);
      setCurrentPage(1); // Reseta para a primeira página ao buscar
    }, 500);

    return () => clearTimeout(handler);
  }, [searchTerm]);

  // Reseta para a primeira página quando os outros filtros mudarem
  useEffect(() => {
    setCurrentPage(1);
  }, [selectedBrowser, selectedUser]);

  // 2. Efeito para buscar dados reais da Ingestion API via service
  useEffect(() => {
    async function fetchLogs() {
      setLoading(true);
      setError(null);
      try {
        const data = await apiService.getHistory({
          searchTerm: debouncedSearch,
          browser: selectedBrowser,
          user: selectedUser,
        });
        setLogs(data);
      } catch (err) {
        setError('Não foi possível conectar à Ingestion API. Verifique se o serviço está online.');
        console.error(err);
      } finally {
        setLoading(false);
      }
    }

    fetchLogs();
  }, [debouncedSearch, selectedBrowser, selectedUser]);

  // 3. Paginação dos dados retornados
  const paginatedLogs = useMemo(() => {
    const startIndex = (currentPage - 1) * itemsPerPage;
    return logs.slice(startIndex, startIndex + itemsPerPage);
  }, [logs, currentPage]);

  const totalPages = Math.ceil(logs.length / itemsPerPage) || 1;

  // Auxiliares visuais
  const getBrowserIcon = (browser: string) => {
    switch (browser.toLowerCase()) {
      case 'chrome': return <i className="fa-brands fa-chrome text-amber-500"></i>;
      case 'edge': return <i className="fa-brands fa-edge text-blue-400"></i>;
      case 'firefox': return <i className="fa-brands fa-firefox-browser text-orange-500"></i>;
      default: return <i className="fa-solid fa-globe text-slate-400"></i>;
    }
  };

  // Extrai lista única de usuários para o dropdown com base nos logs atuais
  const uniqueUsers = useMemo(() => {
    const users = logs.map(log => log.user);
    return Array.from(new Set(users)).filter(Boolean);
  }, [logs]);

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
            className="w-full bg-slate-950 border border-slate-800 rounded-lg px-4 py-2 text-sm text-white placeholder-slate-500 focus:outline-none focus:border-blue-500 transition-colors"
          />
          {searchTerm && (
            <button 
              onClick={() => setSearchTerm('')} 
              className="absolute right-3 top-2.5 text-slate-500 hover:text-white"
            >
              <i className="fa-solid fa-xmark"></i>
            </button>
          )}
        </div>
        
        <div>
          <select
            value={selectedBrowser}
            onChange={(e) => setSelectedBrowser(e.target.value)}
            className="w-full bg-slate-950 border border-slate-800 rounded-lg px-4 py-2 text-sm text-slate-300 focus:outline-none focus:border-blue-500 transition-colors"
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
            className="w-full bg-slate-950 border border-slate-800 rounded-lg px-4 py-2 text-sm text-slate-300 focus:outline-none focus:border-blue-500 transition-colors"
          >
            <option value="">Todos os Usuários</option>
            {uniqueUsers.map(user => (
              <option key={user} value={user}>{user}</option>
            ))}
          </select>
        </div>
      </div>

      {/* Tabela de Resultados ou Estados Alternativos */}
      <div className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden shadow-xl">
        {loading ? (
          /* Estado: Carregando (Loading) */
          <div className="py-20 flex flex-col items-center justify-center text-slate-400 gap-4">
            <i className="fa-solid fa-circle-notch fa-spin text-3xl text-blue-500"></i>
            <span className="text-sm font-medium">Buscando logs de auditoria...</span>
          </div>
        ) : error ? (
          /* Estado: Erro */
          <div className="py-16 flex flex-col items-center justify-center text-rose-500 gap-3">
            <i className="fa-solid fa-triangle-exclamation text-4xl"></i>
            <span className="font-semibold">{error}</span>
            <button 
              onClick={() => setSearchTerm(searchTerm)} // Força re-trigger da busca
              className="mt-2 px-4 py-2 bg-slate-800 text-slate-200 text-xs rounded-lg hover:bg-slate-700 hover:text-white transition-colors"
            >
              Tentar Novamente
            </button>
          </div>
        ) : logs.length === 0 ? (
          /* Estado: Vazio (Empty State) */
          <div className="py-20 flex flex-col items-center justify-center text-slate-500 gap-3">
            <i className="fa-solid fa-folder-open text-4xl"></i>
            <span className="text-sm">Nenhum registro encontrado para a sua busca.</span>
          </div>
        ) : (
          /* Estado: Dados Prontos */
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm text-slate-300">
                <thead className="text-xs uppercase bg-slate-800/50 text-slate-400 border-b border-slate-800">
                  <tr>
                    <th className="py-3.5 px-4">Usuário / Máquina</th>
                    <th className="py-3.5 px-4">Navegador</th>
                    <th className="py-3.5 px-4">Título / URL</th>
                    <th className="py-3.5 px-4 text-right">Data/Hora</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-800/40">
                  {paginatedLogs.map((log) => (
                    <tr key={log.id} className="hover:bg-slate-800/15 transition-colors">
                      <td className="py-3.5 px-4">
                        <div className="font-semibold text-white">{log.user}</div>
                        <div className="text-xs text-slate-500">{log.machineName}</div>
                      </td>
                      <td className="py-3.5 px-4">
                        {getBrowserIcon(log.browser)} <span className="ml-1.5">{log.browser}</span>
                      </td>
                      <td className="py-3.5 px-4 max-w-md truncate">
                        <div className="font-medium text-slate-200 truncate">{log.title || 'Sem título'}</div>
                        <a 
                          href={log.url} 
                          target="_blank" 
                          rel="noreferrer" 
                          className="text-xs text-blue-400 hover:text-blue-300 hover:underline truncate block mt-0.5"
                        >
                          {log.url}
                        </a>
                      </td>
                      <td className="py-3.5 px-4 text-right text-xs text-slate-400">
                        {new Date(log.accessedAt).toLocaleString('pt-BR')}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Controles de Paginação */}
            <div className="px-6 py-4 bg-slate-900/50 border-t border-slate-800 flex items-center justify-between">
              <div className="text-xs text-slate-400">
                Mostrando <span className="text-white font-medium">{Math.min(logs.length, (currentPage - 1) * itemsPerPage + 1)}</span> a{' '}
                <span className="text-white font-medium">{Math.min(logs.length, currentPage * itemsPerPage)}</span> de{' '}
                <span className="text-white font-medium">{logs.length}</span> registros.
              </div>

              <div className="flex items-center gap-2">
                <button
                  onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
                  disabled={currentPage === 1}
                  className="px-3 py-1.5 bg-slate-800 hover:bg-slate-700 disabled:opacity-40 disabled:hover:bg-slate-800 text-xs text-slate-300 rounded-md transition-colors"
                >
                  Anterior
                </button>
                <span className="text-xs text-slate-400 px-2">
                  Página <strong className="text-white">{currentPage}</strong> de <strong className="text-white">{totalPages}</strong>
                </span>
                <button
                  onClick={() => setCurrentPage(prev => Math.min(prev + 1, totalPages))}
                  disabled={currentPage === totalPages}
                  className="px-3 py-1.5 bg-slate-800 hover:bg-slate-700 disabled:opacity-40 disabled:hover:bg-slate-800 text-xs text-slate-300 rounded-md transition-colors"
                >
                  Próxima
                </button>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
