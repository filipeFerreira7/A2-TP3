import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../api/axios';
import { useAuth } from '../contexts/AuthContext';
import Button from '../components/ui/Button';
import Pill from '../components/ui/Pill';
import LoadingSpinner from '../components/ui/LoadingSpinner';
import ApplicationForm from '../components/jobs/ApplicationForm';
import { money, date, translateWorkModel, translateLevel } from '../utils/formatters';

export default function JobDetailPage() {
  const { id } = useParams();
  const { user, isCandidate, isCompanyUser } = useAuth();
  const navigate = useNavigate();
  const [job, setJob] = useState(null);
  const [loading, setLoading] = useState(true);
  const [applying, setApplying] = useState(false);
  const [applied, setApplied] = useState(false);
  const [showForm, setShowForm] = useState(false);
  const [msg, setMsg] = useState('');

  useEffect(() => {
    api.get(`/vagas/${id}`).then(r => {
      setJob(r.data);
    }).catch(() => navigate('/vagas'))
      .finally(() => setLoading(false));
  }, [id, navigate]);

  useEffect(() => {
    if (user && isCandidate()) {
      api.get('/candidaturas/minhas').then(r => {
        const apps = r.data.candidaturas || r.data || [];
        setApplied(!!apps.find(a => a.jobId === id));
      }).catch(() => {});
    }
  }, [id, user, isCandidate]);

  const handleApplyClick = () => {
    if (!user) { navigate('/login', { state: { from: { pathname: `/vagas/${id}` } } }); return; }
    if (!isCandidate()) { setMsg('Apenas candidatos podem se candidatar.'); return; }
    setShowForm(true);
  };

  const handleFormSuccess = () => {
    setApplied(true);
    setShowForm(false);
    setMsg('Candidatura realizada com sucesso!');
  };

  const handleFormCancel = () => {
    setShowForm(false);
  };

  if (loading) return <LoadingSpinner />;
  if (!job) return <p className="text-sm text-muted">Vaga não encontrada.</p>;

  const benefitsList = (job.benefits || '').split('\n').filter(Boolean);
  const tagsList = (job.tags || '').split(',').map(s => s.trim()).filter(Boolean);

  return (
    <div className="animate-fade-in max-w-3xl mx-auto flex flex-col gap-6">
      <Button variant="ghost" size="sm" onClick={() => navigate('/vagas')} style={{ alignSelf: 'flex-start' }}>&larr; Voltar</Button>

      <div className="bg-white border border-line rounded-2xl p-6 shadow-lg">
        <div className="flex items-start gap-4 mb-4">
          <div className="grid w-12 h-12 place-items-center bg-brand/10 text-brand font-bold rounded-xl text-lg flex-shrink-0">
            {(job.company || 'JC')[0]}
          </div>
          <div className="flex-1">
            <h1 className="m-0 text-2xl font-extrabold text-ink">{job.title}</h1>
            <p className="m-0 mt-1 text-sm text-muted">{job.company}{job.location ? ` \u00B7 ${job.location}` : ''}</p>
          </div>
        </div>

        <div className="flex flex-wrap gap-2 mb-4">
          <Pill variant="default">{translateWorkModel(job.workModel)}</Pill>
          <Pill variant="default">{translateLevel(job.level)}</Pill>
          {job.openPositions > 0 && <Pill variant="success">{job.openPositions} vaga{job.openPositions > 1 ? 's' : ''}</Pill>}
        </div>

        <p className="text-sm font-bold text-brand mb-5">{money(job.minimumSalary)} — {money(job.maximumSalary)}</p>

        {job.companyDescription && (
          <div className="mb-6">
            <h2 className="m-0 text-lg font-bold text-ink mb-2">Sobre a empresa</h2>
            <p className="m-0 text-sm text-muted leading-relaxed whitespace-pre-line">{job.companyDescription}</p>
          </div>
        )}

        <hr className="border-line my-6" />

        <div className="mb-6">
          <h2 className="m-0 text-lg font-bold text-ink mb-2">Descrição da vaga</h2>
          <p className="m-0 text-sm text-muted leading-relaxed whitespace-pre-line">{job.description}</p>
        </div>

        {job.responsibilities && (
          <div className="mb-6">
            <h2 className="m-0 text-lg font-bold text-ink mb-2">Responsabilidades</h2>
            <p className="m-0 text-sm text-muted leading-relaxed whitespace-pre-line">{job.responsibilities}</p>
          </div>
        )}

        {job.requirements && (
          <div className="mb-6">
            <h2 className="m-0 text-lg font-bold text-ink mb-2">Requisitos</h2>
            <p className="m-0 text-sm text-muted leading-relaxed whitespace-pre-line">{job.requirements}</p>
          </div>
        )}

        {job.requiredSkills?.length > 0 && (
          <div className="mb-6">
            <h2 className="m-0 text-lg font-bold text-ink mb-2">Habilidades obrigatórias</h2>
            <div className="flex flex-wrap gap-2">
              {job.requiredSkills.map((s, i) => <Pill key={i} variant="default">{s}</Pill>)}
            </div>
          </div>
        )}

        {job.differentialSkills?.length > 0 && (
          <div className="mb-6">
            <h2 className="m-0 text-lg font-bold text-ink mb-2">Habilidades diferenciais</h2>
            <div className="flex flex-wrap gap-2">
              {job.differentialSkills.map((s, i) => <Pill key={i} variant="warning">{s}</Pill>)}
            </div>
          </div>
        )}

        {benefitsList.length > 0 && (
          <div className="mb-6">
            <h2 className="m-0 text-lg font-bold text-ink mb-2">Benefícios</h2>
            <ul className="m-0 text-sm text-muted leading-relaxed pl-4 space-y-1">
              {benefitsList.map((b, i) => <li key={i}>{b}</li>)}
            </ul>
          </div>
        )}

        {tagsList.length > 0 && (
          <div className="mb-6">
            <h2 className="m-0 text-lg font-bold text-ink mb-2">Tags</h2>
            <div className="flex flex-wrap gap-2">
              {tagsList.map((t, i) => <Pill key={i} variant="default">{t}</Pill>)}
            </div>
          </div>
        )}

        {job.schedule && (
          <div className="mb-6">
            <h2 className="m-0 text-lg font-bold text-ink mb-2">Horário</h2>
            <p className="m-0 text-sm text-muted leading-relaxed">{job.schedule}</p>
          </div>
        )}

        <hr className="border-line my-6" />

        <div className="flex flex-wrap gap-x-6 gap-y-1 text-xs text-muted">
          <span>Publicada em {date(job.publishedAt)}</span>
          <span>Inscrições até {date(job.closingDate)}</span>
        </div>

        {isCompanyUser() && user ? (
          <Button onClick={() => navigate(`/vaga-edit/${id}`)} size="lg" variant="secondary" className="w-full mt-4">
            Editar Vaga
          </Button>
        ) : (
          <>
            {!showForm && !applied && (
              <Button onClick={handleApplyClick} disabled={applying} size="lg" className="w-full mt-4">
                {!user ? 'Entrar para Candidatar' : applying ? 'Candidatando...' : 'Candidatar-se'}
              </Button>
            )}
            {applied && <Button disabled size="lg" className="w-full mt-4" variant="secondary">Candidatura Enviada</Button>}
            {msg && <p className={`text-sm mt-2 font-bold ${msg.includes('sucesso') ? 'text-ok' : 'text-danger'}`}>{msg}</p>}
            {showForm && !applied && (
              <ApplicationForm user={user} jobId={id} onSuccess={handleFormSuccess} onCancel={handleFormCancel} />
            )}
          </>
        )}
      </div>
    </div>
  );
}
