export interface BrowserHistoryItem {
    id: string;
    user: string;
    machineName: string;
    url: string;
    title: string;
    accessedAt: string;
    browser: 'Chrome' | 'Edge' | 'Brave' | 'Firefox' | 'Opera' | string;
}

export interface DashboardMetrics {
    totalAcessos: number;
    agentesAtivos: number;
    sitesBloqueadosAlertas: number;
}
