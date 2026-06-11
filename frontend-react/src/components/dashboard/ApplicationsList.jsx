import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Pill from '../ui/Pill';
import Button from '../ui/Button';
import ProcessFlow from '../process/ProcessFlow';
import { date, translateAppStatus } from '../../utils/formatters';

export default function ApplicationsList({ applications }) {
  const navigate = useNavigate();
  const [expanded, setExpanded] = useState(null);

  const toggle = (id) => setExpanded(prev => prev === id ? null : id);

  return (
    <div className="flex flex-col gap-3">
      {(!applications || applications.length === 0) && (
        <p className="text-sm text-muted py-4 text-center">Nenhuma candidatura encontrada.</p>
      )}
      {applications?.map(app => {
        const isRejected = app.status === 'Rejected' || app.status === 'Withdrawn';
        const isExpanded = expanded === app.id;
        return (
          <div key={app.id} className="bg-white border border-line rounded-xl shadow-md overflow-hidden">
            <div
              onClick={() => toggle(app.id)}
              className="px-4 py-3 flex items-center justify-between gap-3 cursor-pointer hover:bg-soft/50 transition-colors"
            >
              <div className="flex items-center gap-3 min-w-0">
                <div className="grid w-9 h-9 place-items-center bg-brand/10 text-brand font-bold rounded-lg text-xs flex-shrink-0">
                  {(app.company || '?')[0]}
                </div>
                <div className="min-w-0">
                  <p className="m-0 text-sm font-bold text-ink truncate">{app.jobTitle || 'Vaga'}</p>
                  <p className="m-0 text-xs text-muted">{app.company || ''} &middot; {date(app.appliedAt)}</p>
                </div>
              </div>
              <div className="flex items-center gap-2 flex-shrink-0">
                <Pill variant={app.status === 'Approved' ? 'success' : isRejected ? 'danger' : app.status === 'InProgress' ? 'warning' : 'default'}>
                  {translateAppStatus(app.status)}
                </Pill>
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" className={`text-muted transition-transform ${isExpanded ? 'rotate-180' : ''}`}>
                  <polyline points="6 9 12 15 18 9" />
                </svg>
              </div>
            </div>

            {isExpanded && (
              <div className="border-t border-line px-4 py-3 flex flex-col gap-3">
                {app.stages?.length > 0 && (
                  <ProcessFlow
                    steps={app.stages.map((s, i) => ({
                      key: `stage-${i}`,
                      label: s.name,
                      isCurrent: s.isCurrent,
                      isCompleted: s.isCompleted
                    }))}
                  />
                )}

                <div className="flex items-center gap-2">
                  <Button size="sm" variant="ghost" onClick={() => navigate(`/processo/${app.id}`)}>
                    Ver Detalhes
                  </Button>
                </div>

                {isRejected && app.feedbackMessage && (
                  <div className="bg-red-50 border border-red-200 text-danger text-xs px-3.5 py-2.5 rounded-lg">
                    <strong>Devolutiva:</strong> {app.feedbackMessage}
                  </div>
                )}

                {app.status === 'Approved' && (
                  <div className="bg-teal-50 border border-teal-200 text-teal-700 text-xs font-bold px-3.5 py-2.5 rounded-lg">
                    Parabéns! Você foi aprovado(a) nesta vaga.
                  </div>
                )}
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}
