import Pill from '../ui/Pill';
import Button from '../ui/Button';
import { date, translateAppStatus } from '../../utils/formatters';

export default function ApplicantsList({ applicants, onStatusChange }) {
  const nextStatus = (current) => {
    if (current === 'Received') return { status: 'InProgress', label: 'Iniciar Triagem' };
    if (current === 'InProgress') return { status: 'Approved', label: 'Aprovar' };
    return null;
  };

  return (
    <div className="flex flex-col gap-2.5">
      {(!applicants || applicants.length === 0) && (
        <p className="text-sm text-muted py-4 text-center">Nenhum candidato para esta vaga.</p>
      )}
      {applicants?.map(app => {
        const next = onStatusChange ? nextStatus(app.status) : null;
        return (
          <div key={app.applicationId || app.id} className="bg-white border border-line rounded-lg px-4 py-3 flex items-center justify-between gap-3">
            <div className="flex items-center gap-3 min-w-0">
              <div className="grid w-9 h-9 place-items-center bg-brand/10 text-brand font-bold rounded-lg text-xs flex-shrink-0">
                {(app.candidateName || '?')[0]}
              </div>
              <div className="min-w-0">
                <p className="m-0 text-sm font-bold text-ink truncate">{app.candidateName}</p>
                <p className="m-0 text-xs text-muted">{app.cpf} &middot; {date(app.appliedAt)}</p>
              </div>
            </div>
            <div className="flex items-center gap-2 flex-shrink-0">
              <Pill variant={app.status === 'Approved' ? 'success' : app.status === 'Rejected' ? 'danger' : app.status === 'InProgress' ? 'warning' : 'default'}>
                {translateAppStatus(app.status)}
              </Pill>
              {next && (
                <>
                  <Button size="sm" variant="primary" onClick={() => onStatusChange(app.applicationId || app.id, next.status)}>
                    {next.label}
                  </Button>
                  {app.status !== 'Rejected' && (
                    <Button size="sm" variant="danger" onClick={() => onStatusChange(app.applicationId || app.id, 'Rejected')}>
                      Recusar
                    </Button>
                  )}
                </>
              )}
            </div>
          </div>
        );
      })}
    </div>
  );
}
