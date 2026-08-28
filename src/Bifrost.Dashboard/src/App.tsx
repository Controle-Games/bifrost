import { useState } from "react";
import Sidebar from './components/SideBar';
import Overview from "./views/Overview";
import AuditLog from "./views/AuditLog";

export default function App() {
    const [activeTab, setActiveTab] = useState('overview');

    return (
        <div className="flex h-screen bg-slate-950 text-white font-sans">
            {/* Barra Lateral */}
            <Sidebar activeTab={activeTab} setActiveTab={setActiveTab} />
    
            {/* Conteúdo Principal */}
            <main className="flex-1 overflow-y-auto p-8">
                <header className="mb-8">
                    <h2 className="text-2xl font-bold">
                        {activeTab === 'overview' ? 'Visão Geral' : 'Auditoria'}
                    </h2>
                    <p className="text-slate-400 text-sm">
                        {activeTab === 'overview' 
                            ? 'Métricas globais de coleta do agente' 
                            : 'Pesquisa avançada em todos os logs coletados'}
                    </p>
                </header>
    
                {/* Renderização das Views */}
                {activeTab === 'overview' ? <Overview /> : <AuditLog />}
            </main>
        </div>
    );
}
