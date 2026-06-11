import { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import api from '../api/axios';
import KanbanPipeline from '../components/process/KanbanPipeline';
import LoadingSpinner from '../components/ui/LoadingSpinner';

export default function KanbanPage() {
  const { user, isCompanyUser } = useAuth();
  const [jobs, setJobs] = useState([]);
  const [selectedJob, setSelectedJob] = useState('');
  const [applications, setApplications] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (isCompanyUser()) {
      api.get('/vagas/minhas').then(r => setJobs(r.data.vagas || r.data || [])).catch(() => {}).finally(() => setLoading(false));
    } else {
      setLoading(false);
    }
  }, [user, isCompanyUser]);

  useEffect(() => {
    if (!selectedJob) { setApplications([]); return; }
    api.get(`/candidaturas/vaga/${selectedJob}`).then(r => setApplications(r.data.candidaturas || r.data || [])).catch(() => {});
  }, [selectedJob]);

  if (!isCompanyUser()) {
    return <div className="animate-fade-in"><p className="text-sm text-muted">Apenas recrutadores e gestores podem acessar o Kanban.</p></div>;
  }

  if (loading) return <LoadingSpinner />;

  return (
    <div className="animate-fade-in flex flex-col gap-5">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
        <div>
          <h1 className="m-0 text-2xl font-extrabold text-ink">Pipeline Kanban</h1>
          <p className="m-0 text-sm text-muted mt-0.5">Visualize os candidatos organizados por etapa</p>
        </div>
        <div className="flex flex-col gap-1.5 max-w-xs">
          <span className="text-xs font-bold text-muted">Selecionar Vaga</span>
          <select
            value={selectedJob}
            onChange={e => setSelectedJob(e.target.value)}
            className="w-full min-h-[44px] px-3 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand"
          >
            <option value="">Escolha uma vaga...</option>
            {jobs.map(j => <option key={j.id} value={j.id}>{j.title}</option>)}
          </select>
        </div>
      </div>
      {selectedJob ? (
        <KanbanPipeline applications={applications} />
      ) : (
        <p className="text-sm text-muted py-8 text-center">Selecione uma vaga para visualizar o pipeline.</p>
      )}
    </div>
  );
}
