import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/axios';
import { useAuth } from '../contexts/AuthContext';
import DashboardCards from '../components/dashboard/DashboardCards';
import ApplicationsList from '../components/dashboard/ApplicationsList';
import CompanyJobsList from '../components/dashboard/CompanyJobsList';
import Button from '../components/ui/Button';
import Modal from '../components/ui/Modal';
import ApplicantsList from '../components/dashboard/ApplicantsList';
import LoadingSpinner from '../components/ui/LoadingSpinner';

export default function Dashboard() {
  const { user, isCandidate, isRecruiter, isManager, isAdmin } = useAuth();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [data, setData] = useState({});
  const [selectedJob, setSelectedJob] = useState(null);
  const [applicants, setApplicants] = useState([]);

  useEffect(() => {
    if (!user) return;
    const role = user.primaryPermission;
    const fetches = {};

    if (role === 'Candidate') {
      fetches.applications = api.get('/candidaturas/minhas').then(r => setData(prev => ({ ...prev, applications: r.data || [] })));
    } else if (role === 'Recruiter' || role === 'Manager') {
      fetches.jobs = api.get('/vagas/minhas').then(r => setData(prev => ({ ...prev, jobs: r.data || [] })));
      fetches.stats = api.get('/vagas/stats').then(r => setData(prev => ({ ...prev, stats: r.data || {} })));
    } else if (role === 'Administrator') {
      fetches.stats = api.get('/vagas/stats').then(r => setData(prev => ({ ...prev, stats: r.data || {} })));
    }

    Promise.all(Object.values(fetches)).catch(() => {}).finally(() => setLoading(false));
  }, [user]);

  const viewApplicants = async (jobId) => {
    try {
      const { data: r } = await api.get(`/candidaturas/vaga/${jobId}`);
      setApplicants(r || []);
      setSelectedJob(jobId);
    } catch {}
  };

  const handleDelete = async (jobId) => {
    try {
      await api.delete(`/vagas/${jobId}`);
      setData(prev => ({ ...prev, jobs: (prev.jobs || []).filter(j => j.id !== jobId) }));
    } catch {}
  };

  if (!user) return <LoadingSpinner text="Redirecionando..." />;
  if (loading) return <LoadingSpinner />;

  const role = user.primaryPermission;

  return (
    <div className="animate-fade-in flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="m-0 text-2xl font-extrabold text-ink">Olá, {user.fullName}!</h1>
          <p className="m-0 text-sm text-muted mt-0.5 capitalize">
            {role === 'Candidate' ? 'Candidato' : role === 'Recruiter' ? 'Recrutador' : role === 'Manager' ? 'Gestor' : 'Administrador'}
            {user.company && ` · ${user.company.name || ''}`}
          </p>
        </div>
      </div>

      {/* Candidate Dashboard */}
      {isCandidate() && (
        <>
          <DashboardCards items={[
            { label: 'Candidaturas', value: data.applications?.length || 0 },
            { label: 'Em Andamento', value: data.applications?.filter(a => a.status === 'InProgress').length || 0 },
            { label: 'Aprovadas', value: data.applications?.filter(a => a.status === 'Approved').length || 0 },
            { label: 'Recusadas', value: data.applications?.filter(a => a.status === 'Rejected').length || 0 },
          ]} />
          <section className="flex flex-col gap-3">
            <h2 className="m-0 text-lg font-bold text-ink">Minhas Candidaturas</h2>
            <ApplicationsList applications={data.applications} />
          </section>
        </>
      )}

      {/* Recruiter Dashboard */}
      {isRecruiter() && (
        <>
          <DashboardCards items={[
            { label: 'Vagas Ativas', value: data.jobs?.length || 0 },
            { label: 'Total Candidatos', value: data.jobs?.reduce((s, j) => s + (j.applications || 0), 0) || 0 },
          ]} />
          <div className="flex gap-3">
            <Button onClick={() => navigate('/criar-vaga')}>+ Nova Vaga</Button>
          </div>
          <section className="flex flex-col gap-3">
            <h2 className="m-0 text-lg font-bold text-ink">Minhas Vagas</h2>
            <CompanyJobsList jobs={data.jobs} onViewApplicants={viewApplicants} onDelete={handleDelete} />
          </section>
        </>
      )}

      {/* Admin Dashboard */}
      {isAdmin() && (
        <>
          <DashboardCards items={[
            { label: 'Empresas', value: data.stats?.totalEmpresas || '—' },
            { label: 'Vagas', value: data.stats?.totalVagas || '—' },
            { label: 'Candidatos', value: data.stats?.totalCandidatos || '—' },
            { label: 'Contratações', value: data.stats?.totalContratacoes || '—' },
          ]} />
          <section className="bg-white border border-line rounded-2xl p-6 shadow-lg flex flex-col items-center gap-3 text-center">
            <h2 className="m-0 text-lg font-bold text-ink">Painel Administrativo</h2>
            <p className="m-0 text-sm text-muted max-w-md">Gerencie usuários, empresas e acompanhe relatórios gerais do sistema.</p>
            <Button onClick={() => navigate('/admin')}>Acessar Administração</Button>
          </section>
        </>
      )}

      {/* Manager Dashboard */}
      {isManager() && (
        <>
          <DashboardCards items={[
            { label: 'Vagas', value: data.jobs?.length || 0 },
            { label: 'Total Candidatos', value: data.jobs?.reduce((s, j) => s + (j.applications || 0), 0) || 0 },
            { label: 'Empresas', value: data.stats?.totalEmpresas || '—' },
            { label: 'Contratações', value: data.stats?.totalContratacoes || '—' },
          ]} />
          <div className="flex gap-3">
            <Button onClick={() => navigate('/criar-vaga')}>+ Nova Vaga</Button>
          </div>

          {/* Pending approval jobs */}
          {(data.jobs || []).filter(j => j.status === 'PendingApproval').length > 0 && (
            <section className="flex flex-col gap-3">
              <h2 className="m-0 text-lg font-bold text-ink">Vagas Pendentes de Aprovação</h2>
              <div className="flex flex-col gap-2">
                {(data.jobs || []).filter(j => j.status === 'PendingApproval').map(job => (
                    <div key={job.id} className="bg-amber-50 border border-amber-200 rounded-xl px-4 py-3 flex items-center justify-between gap-3">
                      <div className="min-w-0">
                        <p className="m-0 text-sm font-bold text-ink">{job.title}</p>
                        <p className="m-0 text-xs text-muted">{job.company}</p>
                      </div>
                      <div className="flex gap-2">
                        <Button size="sm" onClick={async () => {
                          try {
                            await api.post(`/vagas/${job.id}/aprovar`);
                            const { data: r } = await api.get('/vagas/minhas');
                            setData(prev => ({ ...prev, jobs: r.data || r || [] }));
                          } catch {}
                        }}>Aprovar</Button>
                        <Button size="sm" variant="danger" onClick={async () => {
                          const reason = window.prompt('Motivo da rejeição (devolutiva para o RH):');
                          if (reason === null) return;
                          try {
                            await api.post(`/vagas/${job.id}/rejeitar`, { reason });
                            const { data: r } = await api.get('/vagas/minhas');
                            setData(prev => ({ ...prev, jobs: r.data || r || [] }));
                          } catch {}
                        }}>Recusar</Button>
                      </div>
                    </div>
                ))}
              </div>
            </section>
          )}

          <section className="flex flex-col gap-3">
            <h2 className="m-0 text-lg font-bold text-ink">Vagas da Empresa</h2>
            <CompanyJobsList jobs={data.jobs} onViewApplicants={viewApplicants} onDelete={handleDelete} />
          </section>
        </>
      )}

      {/* Applicants Modal */}
      <Modal open={!!selectedJob} onClose={() => { setSelectedJob(null); setApplicants([]); }} title="Candidatos">
        <ApplicantsList
          applicants={applicants}
          onStatusChange={(isManager() || isRecruiter() || isAdmin()) ? async (appId, newStatus) => {
            let feedback = null;
            if (newStatus === 'Rejected') {
              feedback = window.prompt('Motivo da recusa (devolutiva para o candidato):');
              if (feedback === null) return;
            }
            if (newStatus === 'InProgress') {
              const plataforma = window.prompt('Informe a plataforma da entrevista (ex: Zoom, Teams, Google Meet):');
              if (plataforma === null) return;
              feedback = `via ${plataforma}`;
            }
            try {
              await api.patch(`/candidaturas/${appId}/status`, { status: newStatus, feedback });
              const { data: r } = await api.get(`/candidaturas/vaga/${selectedJob}`);
              setApplicants(r || []);
            } catch {}
          } : null}
        />
      </Modal>
    </div>
  );
}
