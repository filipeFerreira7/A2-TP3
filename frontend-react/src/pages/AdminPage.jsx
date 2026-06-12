import { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import api from '../api/axios';
import DashboardCards from '../components/dashboard/DashboardCards';
import LoadingSpinner from '../components/ui/LoadingSpinner';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, PieChart, Pie, Cell, Legend } from 'recharts';

const COLORS = ['#006d77', '#f4a261', '#e76f51', '#2a9d8f', '#264653'];

function TabButton({ active, onClick, children }) {
  return (
    <button
      onClick={onClick}
      className={`px-4 py-2 text-sm font-bold rounded-lg border-none cursor-pointer transition-all ${
        active ? 'bg-brand text-white' : 'bg-soft text-muted hover:text-ink'
      }`}
    >
      {children}
    </button>
  );
}

export default function AdminPage() {
  const { isAdmin } = useAuth();
  const [tab, setTab] = useState('usuarios');
  const [loading, setLoading] = useState(true);
  const [users, setUsers] = useState([]);
  const [companies, setCompanies] = useState([]);
  const [reports, setReports] = useState(null);
  const [message, setMessage] = useState('');
  const [permUser, setPermUser] = useState(null);

  const roleLabels = { Candidate: 'Candidato', Recruiter: 'Recrutador', Manager: 'Gestor', Administrator: 'Administrador' };

  useEffect(() => {
    if (!isAdmin()) return;
    setLoading(true);
    Promise.all([
      api.get('/admin/usuarios').catch(() => ({ data: [] })),
      api.get('/admin/empresas').catch(() => ({ data: [] })),
      api.get('/admin/relatorios').catch(() => ({ data: null }))
    ]).then(([u, c, r]) => {
      setUsers(u.data || []);
      setCompanies(c.data || []);
      setReports(r.data || null);
    }).finally(() => setLoading(false));
  }, [isAdmin]);

  const handleSavePerms = async (userId, role, companyId) => {
    try {
      await api.put(`/admin/usuarios/${userId}/role`, { role, companyId: companyId || null });
      const { data } = await api.get('/admin/usuarios');
      setUsers(data || []);
      setPermUser(null);
      setMessage('Permissões atualizadas com sucesso!');
    } catch (err) {
      setMessage(err.response?.data?.error || 'Erro ao atualizar permissões.');
    }
  };

  const handleUserStatus = async (userId, isActive) => {
    try {
      await api.put(`/admin/usuarios/${userId}/status`, { isActive });
      setUsers(prev => prev.map(u => u.id === userId ? { ...u, isActive } : u));
      setMessage(isActive ? 'Usuario ativado.' : 'Usuario desativado.');
    } catch { setMessage('Erro ao alterar status.'); }
  };

  const handleCompanyStatus = async (companyId, isActive) => {
    try {
      await api.put(`/admin/empresas/${companyId}/status`, { isActive });
      setCompanies(prev => prev.map(c => c.id === companyId ? { ...c, isActive } : c));
      setMessage(isActive ? 'Empresa ativada.' : 'Empresa desativada.');
    } catch { setMessage('Erro ao alterar status.'); }
  };

  if (!isAdmin()) {
    return <div className="animate-fade-in"><p className="text-sm text-muted">Apenas administradores podem acessar esta pagina.</p></div>;
  }

  if (loading) return <LoadingSpinner />;

  return (
    <div className="animate-fade-in flex flex-col gap-6">
      <div>
        <h1 className="m-0 text-2xl font-extrabold text-ink">Administração</h1>
        <p className="m-0 text-sm text-muted mt-0.5">Gerenciar usuários, empresas e relatórios do sistema</p>
      </div>

      {message && (
        <div className="bg-ok/10 border border-ok/30 text-ok text-xs font-bold px-3.5 py-2.5 rounded-lg flex items-center justify-between">
          <span>{message}</span>
          <button onClick={() => setMessage('')} className="bg-transparent border-none cursor-pointer text-ok text-sm">&times;</button>
        </div>
      )}

      <div className="flex gap-2 flex-wrap">
        <TabButton active={tab === 'usuarios'} onClick={() => setTab('usuarios')}>Usuários</TabButton>
        <TabButton active={tab === 'empresas'} onClick={() => setTab('empresas')}>Empresas</TabButton>
        <TabButton active={tab === 'relatorios'} onClick={() => setTab('relatorios')}>Relatórios</TabButton>
      </div>

      {/* Users Tab */}
      {tab === 'usuarios' && (
        <section className="bg-white border border-line rounded-2xl p-6 shadow-lg">
          <h2 className="m-0 text-lg font-bold text-ink mb-4">Gerenciar Usuários ({users.length})</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="text-left text-muted text-xs border-b border-line">
                  <th className="pb-2 font-bold">Nome</th>
                  <th className="pb-2 font-bold">Email</th>
                  <th className="pb-2 font-bold">Permissão</th>
                  <th className="pb-2 font-bold">Empresa</th>
                  <th className="pb-2 font-bold">Status</th>
                  <th className="pb-2 font-bold">Ações</th>
                </tr>
              </thead>
              <tbody>
                {users.map(u => (
                  <tr key={u.id} className="border-b border-line/50">
                    <td className="py-2.5 font-bold text-ink">{u.fullName}</td>
                    <td className="py-2.5 text-muted">{u.email}</td>
                    <td className="py-2.5">
                      <span className={`text-xs font-bold px-2 py-0.5 rounded-full ${
                        u.primaryPermission === 'Administrator' ? 'bg-purple-100 text-purple-700' :
                        u.primaryPermission === 'Manager' ? 'bg-brand/10 text-brand' :
                        u.primaryPermission === 'Recruiter' ? 'bg-accent-strong/10 text-accent-strong' :
                        'bg-soft text-muted'
                      }`}>
                        {roleLabels[u.primaryPermission] || u.primaryPermission}
                      </span>
                    </td>
                    <td className="py-2.5 text-muted text-xs">
                      {u.companyId
                        ? companies.find(c => c.id === u.companyId)?.tradeName || 'Vinculado'
                        : '—'}
                    </td>
                    <td className="py-2.5">
                      <span className={`text-xs font-bold px-2 py-0.5 rounded-full ${u.isActive ? 'bg-ok/10 text-ok' : 'bg-danger/10 text-danger'}`}>
                        {u.isActive ? 'Ativo' : 'Inativo'}
                      </span>
                    </td>
                    <td className="py-2.5">
                      <div className="flex gap-1.5">
                        <button
                          onClick={() => setPermUser({ ...u, selectedRole: u.primaryPermission, selectedCompany: u.companyId || '' })}
                          className="px-3 py-1 text-xs font-bold rounded-lg border border-line bg-white cursor-pointer hover:bg-soft transition-all"
                        >
                          Gerenciar Permissões
                        </button>
                        <button
                          onClick={() => handleUserStatus(u.id, !u.isActive)}
                          className={`px-3 py-1 text-xs font-bold rounded-lg border-none cursor-pointer transition-all ${
                            u.isActive ? 'bg-danger/10 text-danger hover:bg-danger/20' : 'bg-ok/10 text-ok hover:bg-ok/20'
                          }`}
                        >
                          {u.isActive ? 'Desativar' : 'Ativar'}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
                {users.length === 0 && (
                  <tr><td colSpan="6" className="py-4 text-center text-muted">Nenhum usuario encontrado.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </section>
      )}

      {/* Permission Modal */}
      {permUser && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={() => setPermUser(null)}>
          <div className="bg-white rounded-2xl shadow-2xl p-6 w-full max-w-md flex flex-col gap-4" onClick={e => e.stopPropagation()}>
            <h3 className="m-0 text-lg font-bold text-ink">Gerenciar Permissões</h3>
            <p className="m-0 text-sm text-muted">
              Usuário: <strong>{permUser.fullName}</strong>
            </p>

            <div className="flex flex-col gap-1.5">
              <label className="text-xs font-bold text-muted">Perfil</label>
              <select
                value={permUser.selectedRole}
                onChange={e => setPermUser(p => ({ ...p, selectedRole: e.target.value, selectedCompany: '' }))}
                className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand transition-all"
              >
                <option value="Candidate">Candidato</option>
                <option value="Recruiter">Recrutador</option>
                <option value="Manager">Gestor</option>
                <option value="Administrator">Administrador</option>
              </select>
            </div>

            {(permUser.selectedRole === 'Recruiter' || permUser.selectedRole === 'Manager') && (
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-muted">Vincular à Empresa</label>
                <select
                  value={permUser.selectedCompany}
                  onChange={e => setPermUser(p => ({ ...p, selectedCompany: e.target.value }))}
                  className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand transition-all"
                >
                  <option value="">Selecione uma empresa...</option>
                  {companies.filter(c => c.isActive).map(c => (
                    <option key={c.id} value={c.id}>{c.tradeName}</option>
                  ))}
                </select>
              </div>
            )}

            <div className="flex justify-end gap-2 mt-2">
              <button
                onClick={() => setPermUser(null)}
                className="px-4 py-2 text-sm font-bold rounded-lg border border-line bg-white cursor-pointer hover:bg-soft transition-all"
              >
                Cancelar
              </button>
              <button
                onClick={() => handleSavePerms(permUser.id, permUser.selectedRole, permUser.selectedRole === 'Candidate' || permUser.selectedRole === 'Administrator' ? null : permUser.selectedCompany)}
                className="px-4 py-2 text-sm font-bold rounded-lg border-none bg-brand text-white cursor-pointer hover:opacity-90 transition-all"
              >
                Salvar
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Companies Tab */}
      {tab === 'empresas' && (
        <section className="bg-white border border-line rounded-2xl p-6 shadow-lg">
          <h2 className="m-0 text-lg font-bold text-ink mb-4">Gerenciar Empresas ({companies.length})</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="text-left text-muted text-xs border-b border-line">
                  <th className="pb-2 font-bold">Razão Social</th>
                  <th className="pb-2 font-bold">Nome Fantasia</th>
                  <th className="pb-2 font-bold">CNPJ</th>
                  <th className="pb-2 font-bold">Email</th>
                  <th className="pb-2 font-bold">Vagas</th>
                  <th className="pb-2 font-bold">Status</th>
                  <th className="pb-2 font-bold">Ações</th>
                </tr>
              </thead>
              <tbody>
                {companies.map(c => (
                  <tr key={c.id} className="border-b border-line/50">
                    <td className="py-2.5 font-bold text-ink">{c.legalName}</td>
                    <td className="py-2.5">{c.tradeName}</td>
                    <td className="py-2.5 text-muted font-mono text-xs">{c.cnpj}</td>
                    <td className="py-2.5 text-muted">{c.email}</td>
                    <td className="py-2.5">{c.jobCount}</td>
                    <td className="py-2.5">
                      <span className={`text-xs font-bold px-2 py-0.5 rounded-full ${c.isActive ? 'bg-ok/10 text-ok' : 'bg-danger/10 text-danger'}`}>
                        {c.isActive ? 'Ativa' : 'Inativa'}
                      </span>
                    </td>
                    <td className="py-2.5">
                      <button
                        onClick={() => handleCompanyStatus(c.id, !c.isActive)}
                        className={`px-3 py-1 text-xs font-bold rounded-lg border-none cursor-pointer transition-all ${
                          c.isActive ? 'bg-danger/10 text-danger hover:bg-danger/20' : 'bg-ok/10 text-ok hover:bg-ok/20'
                        }`}
                      >
                        {c.isActive ? 'Desativar' : 'Ativar'}
                      </button>
                    </td>
                  </tr>
                ))}
                {companies.length === 0 && (
                  <tr><td colSpan="7" className="py-4 text-center text-muted">Nenhuma empresa encontrada.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </section>
      )}

      {/* Reports Tab */}
      {tab === 'relatorios' && reports && (
        <>
          <DashboardCards items={[
            { label: 'Usuários', value: reports.totalUsers },
            { label: 'Empresas', value: reports.totalCompanies },
            { label: 'Vagas', value: reports.totalJobs },
            { label: 'Candidaturas', value: reports.totalApplications },
            { label: 'Contratações', value: reports.totalHires },
          ]} />

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <section className="bg-white border border-line rounded-2xl p-6 shadow-lg">
              <h2 className="m-0 text-lg font-bold text-ink mb-4">Vagas por Status</h2>
              {reports.jobsByStatus?.length > 0 ? (
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={reports.jobsByStatus.map(s => ({ name: s.status, Quantidade: s.count }))}>
                    <XAxis dataKey="name" tick={{ fontSize: 11 }} />
                    <YAxis allowDecimals={false} />
                    <Tooltip />
                    <Bar dataKey="Quantidade" fill="#006d77" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              ) : (
                <p className="text-sm text-muted text-center py-8">Sem dados.</p>
              )}
            </section>

            <section className="bg-white border border-line rounded-2xl p-6 shadow-lg">
              <h2 className="m-0 text-lg font-bold text-ink mb-4">Distribuição Geral</h2>
              {reports.jobsByStatus?.length > 0 ? (
                <ResponsiveContainer width="100%" height={300}>
                  <PieChart>
                    <Pie data={reports.jobsByStatus.map(s => ({ name: s.status, value: s.count }))} cx="50%" cy="50%" outerRadius={100} label={({ name, value }) => `${name}: ${value}`}>
                      {reports.jobsByStatus.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]} />)}
                    </Pie>
                    <Tooltip />
                    <Legend />
                  </PieChart>
                </ResponsiveContainer>
              ) : (
                <p className="text-sm text-muted text-center py-8">Sem dados.</p>
              )}
            </section>
          </div>

          <section className="bg-white border border-line rounded-2xl p-6 shadow-lg">
            <h2 className="m-0 text-lg font-bold text-ink mb-4">Resumo do Sistema</h2>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
              <div className="bg-soft rounded-xl p-4 text-center">
                <p className="m-0 text-2xl font-extrabold text-ink">{reports.pendingApprovalJobs}</p>
                <p className="m-0 text-xs text-muted mt-1">Vagas Pendentes</p>
              </div>
              <div className="bg-soft rounded-xl p-4 text-center">
                <p className="m-0 text-2xl font-extrabold text-danger">{reports.rejectedJobs}</p>
                <p className="m-0 text-xs text-muted mt-1">Vagas Rejeitadas</p>
              </div>
              <div className="bg-soft rounded-xl p-4 text-center">
                <p className="m-0 text-2xl font-extrabold text-ok">{reports.totalHires}</p>
                <p className="m-0 text-xs text-muted mt-1">Contratações</p>
              </div>
              <div className="bg-soft rounded-xl p-4 text-center">
                <p className="m-0 text-2xl font-extrabold text-brand">{reports.totalApplications}</p>
                <p className="m-0 text-xs text-muted mt-1">Candidaturas</p>
              </div>
            </div>
          </section>
        </>
      )}
    </div>
  );
}
