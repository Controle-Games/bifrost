interface SidebarProps {
    activeTab: string;
    setActiveTab: (tab: string) => void;
}

export default function Sidebar({ activeTab, setActiveTab }: SidebarProps) {
    const menuItems = [
        { id: 'overview', label: 'Visão Geral', icon: 'fa-chart-line' },
        { id: 'audit', label: 'Auditoria de Logs', icon: 'fa-shield-halved' },
    ];

    return (
        <aside className="w-64 bg-slate-900 border-r border-slate-800 flex flex-col h-screen text-slate-300">
            <div className="p-6 border-b border-slate-800">
                <h1 className="text-xl font-bold text-white flex items-center gap-2">
                    <i className="fa-solid fa-rainbow text-gradient bg-gradient-to-r from-pink-500 via-red-500 to-yellow-500 bg-clip-text text-transparent"></i> Bifrost
                </h1>
                <span className="text-xs text-slate-500">Painel de Auditoria</span>
            </div>

            <nav className="flex-1 p-4 space-y-2">
            {menuItems.map((item) => (
                <button
                    key={item.id}
                    onClick={() => setActiveTab(item.id)}
                    className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium transition-colors ${
                    activeTab === item.id
                        ? 'bg-blue-600 text-white'
                        : 'hover:bg-slate-800 text-slate-400 hover:text-white'
                    }`}>
                    <i className={`fa-solid ${item.icon} w-5`}></i>
                    {item.label}
                </button>
            ))}
            </nav>
        </aside>
    );
}
