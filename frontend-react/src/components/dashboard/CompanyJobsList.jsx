import { useNavigate } from 'react-router-dom';
import Button from '../ui/Button';
import { date } from '../../utils/formatters';

export default function CompanyJobsList({ jobs, onViewApplicants, onDelete }) {
  const navigate = useNavigate();

  const statusLabel = (s) => {
    const map = { Draft: 'Rascunho', PendingApproval: 'Pendente', Published: 'Publicada', Closed: 'Fechada', Rejected: 'Rejeitada' };
    return map[s] || s;
  };

  return (
    <div className="flex flex-col gap-3">
      {(!jobs || jobs.length === 0) && (
        <p className="text-sm text-muted py-4 text-center">Nenhuma vaga cadastrada.</p>
      )}
      {jobs?.map(job => (
        <div key={job.id} className="bg-white border border-line rounded-xl px-4 py-3 shadow-md flex items-center justify-between gap-3 cursor-pointer hover:bg-soft/50 transition-colors" onClick={() => navigate(`/vagas/${job.id}`)}>
          <div className="min-w-0 flex-1">
            <p className="m-0 text-sm font-bold text-ink">{job.title}</p>
            <p className="m-0 text-xs text-muted">{job.company} &middot; {statusLabel(job.status)} &middot; {job.applications || 0} candidatos</p>
            {job.status === 'Rejected' && job.rejectionReason && (
              <p className="m-0 mt-1 text-xs text-danger"><strong>Devolutiva:</strong> {job.rejectionReason}</p>
            )}
          </div>
          <div className="flex items-center gap-1 flex-shrink-0">
            {onViewApplicants && (
              <Button size="sm" variant="secondary" onClick={(e) => { e.stopPropagation(); onViewApplicants(job.id); }}>Candidatos</Button>
            )}
            <button
              onClick={(e) => { e.stopPropagation(); navigate(`/vaga-edit/${job.id}`); }}
              className="grid w-8 h-8 place-items-center rounded-lg text-muted hover:text-ink hover:bg-soft transition-all cursor-pointer border-0 bg-transparent"
              title="Editar vaga"
            >
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M17 3a2.85 2.83 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5Z"/><path d="m15 5 4 4"/></svg>
            </button>
            <button
              onClick={(e) => { e.stopPropagation(); if (window.confirm('Tem certeza que deseja excluir esta vaga?')) onDelete?.(job.id); }}
              className="grid w-8 h-8 place-items-center rounded-lg text-muted hover:text-danger hover:bg-red-50 transition-all cursor-pointer border-0 bg-transparent"
              title="Excluir vaga"
            >
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M3 6h18"/><path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"/><path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2"/><line x1="10" x2="10" y1="11" y2="17"/><line x1="14" x2="14" y1="11" y2="17"/></svg>
            </button>
          </div>
        </div>
      ))}
    </div>
  );
}
