import { type BrowserHistoryItem, type DashboardMetrics } from "../types";

const API_BASE_URL = import.meta.env.VITE_API_URL;

export const apiService = {
    /**
     * Busca as métricas consolidadas do dashboard (Cards e Gráfico)
     */
    async getMetrics(): Promise<DashboardMetrics> {
        try {
            const response = await fetch(`${API_BASE_URL}/analytics/metrics`);
            if (!response.ok) throw new Error('Erro ao buscar métricas');
            return await response.json();
        } catch (error) {
            console.error('API Error:', error);
            // Retorna valores zerados como fallback seguro caso a API esteja offline
            return { totalAcessos: 0, agentesAtivos: 0, sitesBloqueadosAlertas: 0 };
        }
    },

    /**
     * Busca os históricos de navegação com suporte a filtros
     */
    async getHistory(filters?: { searchTerm?: string; browser?: string; user?: string }): Promise<BrowserHistoryItem[]> {
        try {
            const params = new URLSearchParams();
            if (filters?.searchTerm) params.append('q', filters.searchTerm);
            if (filters?.browser) params.append('browser', filters.browser);
            if (filters?.user) params.append('user', filters.user);

            const response = await fetch(`${API_BASE_URL}/history?${params.toString()}`);
            if (!response.ok) throw new Error('Erro ao buscar logs de auditoria');
            return await response.json();
        } catch (error) {
            console.error('API Error:', error);
            return []; // Retorna lista vazia em caso de falha de conexão
        }
    }
};
