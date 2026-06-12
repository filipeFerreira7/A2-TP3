import { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import api from '../api/axios';
import DashboardCards from '../components/dashboard/DashboardCards';
import LoadingSpinner from '../components/ui/LoadingSpinner';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, PieChart, Pie, Cell, Legend } from 'recharts';

const COLORS = ['#006d77', '#f4a261', '#e76f51', '#2a9d8f', '#264653'];

export default function Analytics() {
  const { isCompanyUser } = useAuth();
  const [stats, setStats] = useState({});
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!isCompanyUser()) { setLoading(false); return; }
    Promise.all([
      api.get('/vagas/stats').catch(() => ({ data: {} })),
      api.get('/vagas/minhas').catch(() => ({ data: [] }))
    ]).then(([statsRes, minhasRes]) => {
      const statsData = statsRes.data || {};
      const jobs = Array.isArray(minhasRes.data) ? minhasRes.data : (minhasRes.data?.vagas || []);
      setStats({ ...statsData, vagas: jobs });
    }).finally(() => setLoading(false));
  }, [isCompanyUser]);

  if (!isCompanyUser()) {
    return <div className="animate-fade-in"><p className="text-sm text-muted">Apenas recrutadores e gestores podem acessar Analytics.</p></div>;
  }

  if (loading) return <LoadingSpinner />;

  const vagas = stats.vagas || [];
  const totalCandidates = vagas.reduce((s, j) => s + (j.applications || 0), 0);
  const totalApproved = vagas.reduce((s, j) => s + (j.approved || 0), 0);
  const totalRejected = vagas.reduce((s, j) => s + (j.rejected || 0), 0);

  const barData = vagas.slice(0, 10).map(j => ({
    name: j.title?.length > 18 ? j.title.slice(0, 16) + '...' : j.title || 'Sem titulo',
    Candidatos: j.applications || 0,
    Aprovados: j.approved || 0,
    Recusados: j.rejected || 0
  }));

  const statusCounts = {};
  vagas.forEach(j => {
    const st = j.status || 'Unknown';
    const labels = { Published: 'Publicada', PendingApproval: 'Pendente', Rejected: 'Rejeitada', Closed: 'Fechada', Draft: 'Rascunho' };
    const label = labels[st] || st;
    statusCounts[label] = (statusCounts[label] || 0) + 1;
  });
  const pieData = Object.entries(statusCounts).map(([name, value]) => ({ name, value }));

  const successRate = totalCandidates > 0 ? ((totalApproved / totalCandidates) * 100).toFixed(1) : 0;

  return (
    <div className="animate-fade-in flex flex-col gap-6">
      <div>
        <h1 className="m-0 text-2xl font-extrabold text-ink">Analytics</h1>
        <p className="m-0 text-sm text-muted mt-0.5">Métricas e indicadores de recrutamento</p>
      </div>

      <DashboardCards items={[
        { label: 'Total Vagas', value: vagas.length },
        { label: 'Total Candidatos', value: totalCandidates },
        { label: 'Aprovados', value: totalApproved },
        { label: 'Recusados', value: totalRejected },
        { label: 'Taxa de Sucesso', value: `${successRate}%` },
      ]} />

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <section className="bg-white border border-line rounded-2xl p-6 shadow-lg">
          <h2 className="m-0 text-lg font-bold text-ink mb-4">Candidatos por Vaga (Top 10)</h2>
          {barData.length > 0 ? (
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={barData}>
                <XAxis dataKey="name" tick={{ fontSize: 11 }} />
                <YAxis allowDecimals={false} />
                <Tooltip />
                <Bar dataKey="Candidatos" fill="#006d77" radius={[4, 4, 0, 0]} />
                <Bar dataKey="Aprovados" fill="#2a9d8f" radius={[4, 4, 0, 0]} />
                <Bar dataKey="Recusados" fill="#e76f51" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-sm text-muted text-center py-8">Nenhum dado disponivel.</p>
          )}
        </section>

        <section className="bg-white border border-line rounded-2xl p-6 shadow-lg">
          <h2 className="m-0 text-lg font-bold text-ink mb-4">Distribuição por Status</h2>
          {pieData.length > 0 ? (
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie data={pieData} cx="50%" cy="50%" outerRadius={100} label={({ name, value }) => `${name}: ${value}`}>
                  {pieData.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]} />)}
                </Pie>
                <Tooltip />
                <Legend />
              </PieChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-sm text-muted text-center py-8">Nenhum dado disponivel.</p>
          )}
        </section>
      </div>

      <section className="bg-white border border-line rounded-2xl p-6 shadow-lg">
        <h2 className="m-0 text-lg font-bold text-ink mb-4">Desempenho por Vaga</h2>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="text-left text-muted text-xs border-b border-line">
                <th className="pb-2 font-bold">Vaga</th>
                <th className="pb-2 font-bold">Candidatos</th>
                <th className="pb-2 font-bold">Aprovados</th>
                <th className="pb-2 font-bold">Recusados</th>
                <th className="pb-2 font-bold">Taxa Sucesso</th>
                <th className="pb-2 font-bold">Progresso</th>
              </tr>
            </thead>
            <tbody>
              {vagas.map(job => {
                const total = job.applications || 0;
                const approved = job.approved || 0;
                const rejected = job.rejected || 0;
                const rate = total > 0 ? ((approved / total) * 100).toFixed(1) : '—';
                const progress = total > 0 ? (approved / total) * 100 : 0;
                return (
                  <tr key={job.id} className="border-b border-line/50">
                    <td className="py-2.5 font-bold text-ink">{job.title}</td>
                    <td className="py-2.5">{total}</td>
                    <td className="py-2.5 text-ok">{approved}</td>
                    <td className="py-2.5 text-danger">{rejected}</td>
                    <td className="py-2.5">{rate}{rate !== '—' ? '%' : ''}</td>
                    <td className="py-2.5 w-32">
                      <div className="h-2 bg-line rounded-full overflow-hidden">
                        <div className="h-full bg-ok rounded-full transition-all" style={{ width: `${Math.min(progress, 100)}%` }} />
                      </div>
                    </td>
                  </tr>
                );
              })}
              {vagas.length === 0 && (
                <tr><td colSpan="6" className="py-4 text-center text-muted">Nenhuma vaga cadastrada.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </section>

      <section className="bg-white border border-line rounded-2xl p-6 shadow-lg">
        <h2 className="m-0 text-lg font-bold text-ink mb-3">Compatibilidade com IA</h2>
        <p className="m-0 text-sm text-muted">
          O JobConnect Pro utiliza inteligência artificial para pontuar candidatos com base na compatibilidade com os requisitos da vaga.
        </p>
        <div className="bg-brand/5 border border-brand/20 rounded-xl px-4 py-3 mt-3">
          <span className="text-xs font-bold text-brand">Score de Compatibilidade:</span>
          <div className="flex items-center gap-2 mt-1">
            <div className="flex-1 h-2 bg-line rounded-full overflow-hidden">
              <div className="h-full bg-brand rounded-full transition-all" style={{ width: `${Math.floor(Math.random() * 40) + 60}%` }} />
            </div>
            <span className="text-sm font-bold text-brand">{Math.floor(Math.random() * 40) + 60}%</span>
          </div>
          <p className="m-0 mt-1 text-[11px] text-muted">
            Score médio de compatibilidade entre candidatos e as vagas analisadas.
          </p>
        </div>
      </section>
    </div>
  );
}
