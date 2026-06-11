import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/axios';
import Button from '../components/ui/Button';
import JobCard from '../components/jobs/JobCard';

export default function Home() {
  const [recentJobs, setRecentJobs] = useState([]);
  const [stats, setStats] = useState({});
  const navigate = useNavigate();

  useEffect(() => {
    api.get('/vagas?limit=6').then(r => setRecentJobs(r.data.vagas || r.data || [])).catch(() => {});
    api.get('/vagas/stats').then(r => setStats(r.data)).catch(() => {});
  }, []);

  return (
    <div className="flex flex-col gap-8 animate-fade-in">
      <section className="bg-gradient-to-br from-brand to-brand-strong rounded-2xl px-8 py-12 md:px-12 md:py-16 text-white flex flex-col gap-4">
        <h1 className="m-0 text-3xl md:text-4xl font-extrabold leading-tight">
          Conectando talentos <br />às melhores oportunidades
        </h1>
        <p className="m-0 text-base md:text-lg opacity-80 max-w-xl">
          O JobConnect Pro une candidatos e empresas de forma inteligente, com IA e gestão integrada.
        </p>
        <div className="flex gap-3 mt-2">
          <Button variant="primary" size="lg" onClick={() => navigate('/vagas')}>Ver Vagas</Button>
          <Button variant="outline" size="lg" className="border-white text-white hover:bg-white/10" onClick={() => navigate('/empresas')}>Empresas</Button>
        </div>
      </section>

      <section className="grid grid-cols-2 md:grid-cols-4 gap-3">
        {[
          { label: 'Vagas Abertas', value: stats.totalVagas ?? '—' },
          { label: 'Empresas', value: stats.totalEmpresas ?? '—' },
          { label: 'Candidatos', value: stats.totalCandidatos ?? '—' },
          { label: 'Contratações', value: stats.totalContratacoes ?? '—' },
        ].map((s, i) => (
          <div key={i} className="bg-white border border-line rounded-xl px-4 py-5 text-center shadow-sm">
            <p className="m-0 text-2xl font-extrabold text-brand">{s.value}</p>
            <p className="m-0 text-xs text-muted mt-1">{s.label}</p>
          </div>
        ))}
      </section>

      <section>
        <div className="flex items-center justify-between mb-4">
          <h2 className="m-0 text-xl font-bold text-ink">Vagas Recentes</h2>
          <Button variant="ghost" size="sm" onClick={() => navigate('/vagas')}>Ver todas</Button>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {recentJobs.map(job => <JobCard key={job.id} job={job} />)}
          {recentJobs.length === 0 && <p className="col-span-full text-sm text-muted py-4 text-center">Nenhuma vaga encontrada.</p>}
        </div>
      </section>
    </div>
  );
}
