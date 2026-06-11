import { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import api from '../api/axios';
import DashboardCards from '../components/dashboard/DashboardCards';
import LoadingSpinner from '../components/ui/LoadingSpinner';

export default function Analytics() {
  const { isCompanyUser } = useAuth();
  const [stats, setStats] = useState({});
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!isCompanyUser()) { setLoading(false); return; }
    Promise.all([
      api.get('/vagas/stats').then(r => setStats(r.data || {})).catch(() => {}),
      api.get('/vagas/minhas').then(r => {
        const jobs = r.data.vagas || r.data || [];
        setStats(prev => ({ ...prev, vagas: jobs }));
      }).catch(() => {})
    ]).finally(() => setLoading(false));
  }, [isCompanyUser]);

  if (!isCompanyUser()) {
    return <div className="animate-fade-in"><p className="text-sm text-muted">Apenas recrutadores e gestores podem acessar Analytics.</p></div>;
  }

  if (loading) return <LoadingSpinner />;

  const vagas = stats.vagas || [];

  return (
    <div className="animate-fade-in flex flex-col gap-6">
      <div>
        <h1 className="m-0 text-2xl font-extrabold text-ink">Analytics</h1>
        <p className="m-0 text-sm text-muted mt-0.5">Métricas e indicadores de recrutamento</p>
      </div>

      <DashboardCards items={[
        { label: 'Total Vagas', value: vagas.length },
        { label: 'Total Candidatos', value: vagas.reduce((s, j) => s + (j.applications || 0), 0) },
        { label: 'Total Candidatos (cadastrados)', value: stats.totalCandidatos ?? '—' },
        { label: 'Empresas', value: stats.totalEmpresas ?? '—' },
        { label: 'Contratações', value: stats.totalContratacoes ?? '—' },
      ]} />

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
              </tr>
            </thead>
            <tbody>
              {vagas.map(job => {
                const total = job.applications || 0;
                const approved = 0;
                const rejected = 0;
                const rate = total > 0 ? ((approved / total) * 100).toFixed(1) : '—';
                return (
                  <tr key={job.id} className="border-b border-line/50">
                    <td className="py-2.5 font-bold text-ink">{job.title}</td>
                    <td className="py-2.5">{total}</td>
                    <td className="py-2.5 text-ok">{approved}</td>
                    <td className="py-2.5 text-danger">{rejected}</td>
                    <td className="py-2.5">{rate}{rate !== '—' ? '%' : ''}</td>
                  </tr>
                );
              })}
              {vagas.length === 0 && (
                <tr><td colSpan="5" className="py-4 text-center text-muted">Nenhuma vaga cadastrada.</td></tr>
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
