import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../api/axios';
import Button from '../components/ui/Button';
import ProcessFlow from '../components/process/ProcessFlow';
import LoadingSpinner from '../components/ui/LoadingSpinner';
import { translateAppStatus } from '../utils/formatters';

export default function ProcessTracking() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [process, setProcess] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    api.get(`/candidaturas/${id}/processo`)
      .then(r => setProcess(r.data))
      .catch(() => setError('Nao foi possivel carregar o processo.'))
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) return <LoadingSpinner />;
  if (error) return (
    <div className="animate-fade-in flex flex-col items-center gap-3 py-12">
      <p className="text-sm text-muted">{error}</p>
      <Button onClick={() => navigate('/dashboard')}>Voltar</Button>
    </div>
  );
  if (!process) return null;

  const stages = (process.stages || []).map((s, i) => ({
    key: `stage-${i}`,
    label: s.name,
    icon: '\u2714\uFE0F',
    isCurrent: s.isCurrent,
    isCompleted: s.isCompleted
  }));

  return (
    <div className="animate-fade-in max-w-3xl mx-auto flex flex-col gap-6">
      <Button variant="ghost" size="sm" onClick={() => navigate('/dashboard')} style={{ alignSelf: 'flex-start' }}>&larr; Voltar</Button>

      <div className="bg-white border border-line rounded-2xl p-6 shadow-lg flex flex-col gap-5">
        <div>
          <h1 className="m-0 text-xl font-extrabold text-ink">Acompanhamento</h1>
          <p className="m-0 mt-1 text-sm text-muted">
            {process.jobTitle} &middot; {process.company}
          </p>
        </div>

        <ProcessFlow steps={stages} />

        {process.isFinished && (
          <div className="bg-teal-50 border border-teal-200 text-teal-700 text-sm font-bold px-4 py-3 rounded-xl text-center">
            Processo concluido!
          </div>
        )}

        <div className="bg-soft rounded-xl p-4 flex flex-col gap-2 text-sm">
          <p className="m-0"><strong>Status:</strong> {translateAppStatus(process.status)}</p>
          <p className="m-0"><strong>Etapa atual:</strong> {process.currentStageName || '—'}</p>
        </div>
      </div>
    </div>
  );
}
