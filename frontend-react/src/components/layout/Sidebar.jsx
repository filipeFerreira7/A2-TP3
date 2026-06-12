import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';

export default function Sidebar() {
  const { user, logout, isCompanyUser, isCandidate } = useAuth();
  const navigate = useNavigate();

  const navItems = [
    { to: '/', label: 'Início', icon: '\u2302' },
    { to: '/vagas', label: 'Vagas', icon: '\uD83D\uDCBC' },
    { to: '/empresas', label: 'Empresas', icon: '\uD83C\uDFEA' },
    { to: '/dashboard', label: 'Painel', icon: '\uD83D\uDEE0\uFE0F' },
  ];

  const companyItems = [
    { to: '/kanban', label: 'Kanban', icon: '\uD83D\uDCCB' },
    { to: '/analytics', label: 'Analytics', icon: '\uD83D\uDCCA' },
  ];

  return (
    <aside className="fixed top-0 left-0 bottom-0 w-[240px] flex flex-col bg-sidebar-bg text-sidebar-text z-50">
      <div className="flex items-center gap-3 px-4 py-5 border-b border-white/10">
        <img src="/logoo.png" alt="JobConnect Pro" className="w-16 h-16 rounded-lg flex-shrink-0 object-contain" />
        <div>
          <div className="text-white text-sm font-bold block">JobConnect Pro</div>
          <div className="text-[11px] text-sidebar-text block">Recrutamento inteligente</div>
        </div>
      </div>

      <nav className="flex-1 px-2.5 py-3 flex flex-col gap-1">
        {navItems.map(item => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `flex items-center gap-2.5 px-3 py-2.5 rounded-lg text-sm font-medium transition-all duration-150 ${
                isActive ? 'bg-brand text-white' : 'text-sidebar-text hover:text-white hover:bg-white/5'
              }`
            }
          >
            <span className="text-lg w-6 text-center">{item.icon}</span>
            {item.label}
          </NavLink>
        ))}

        {isCandidate() && user && (
          <>
            <div className="text-[11px] text-sidebar-text/60 font-bold uppercase tracking-wider px-3 pt-4 pb-1">
              Perfil
            </div>
            <NavLink
              to="/meu-perfil"
              className={({ isActive }) =>
                `flex items-center gap-2.5 px-3 py-2.5 rounded-lg text-sm font-medium transition-all duration-150 ${
                  isActive ? 'bg-brand text-white' : 'text-sidebar-text hover:text-white hover:bg-white/5'
                }`
              }
            >
              <span className="text-lg w-6 text-center">👤</span>
              Meu Perfil
            </NavLink>
          </>
        )}

        {isCompanyUser() && user && (
          <>
            <div className="text-[11px] text-sidebar-text/60 font-bold uppercase tracking-wider px-3 pt-4 pb-1">
              Gestão
            </div>
            {companyItems.map(item => (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  `flex items-center gap-2.5 px-3 py-2.5 rounded-lg text-sm font-medium transition-all duration-150 ${
                    isActive ? 'bg-brand text-white' : 'text-sidebar-text hover:text-white hover:bg-white/5'
                  }`
                }
              >
                <span className="text-lg w-6 text-center">{item.icon}</span>
                {item.label}
              </NavLink>
            ))}
          </>
        )}
      </nav>

      <div className="px-4 py-3.5 border-t border-white/10">
        {user ? (
          <>
            <div className="text-xs px-2.5 py-1.5 rounded-md text-center mb-2 bg-white/5 text-ok font-bold">
              {user.fullName} &middot; {user.primaryPermission}
            </div>
            <button
              onClick={() => { logout(); navigate('/login'); }}
              className="w-full py-2 px-3 text-xs font-bold text-sidebar-text bg-white/5 rounded-md border-none cursor-pointer hover:bg-white/10 transition-all"
            >
              Sair
            </button>
          </>
        ) : (
          <div className="flex gap-1.5">
            <button
              onClick={() => navigate('/login')}
              className="flex-1 py-2 px-3 text-xs font-bold text-white bg-accent-strong rounded-md border-none cursor-pointer hover:opacity-90 transition-all"
            >
              Entrar
            </button>
          </div>
        )}
      </div>
    </aside>
  );
}
