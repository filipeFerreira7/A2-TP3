import { useState } from 'react';
import Pill from '../ui/Pill';
import Button from '../ui/Button';
import { date, translateAppStatus } from '../../utils/formatters';

const levelLabel = v => ({ 1: 'Básico', 2: 'Intermediário', 3: 'Avançado' }[v] || v);

export default function ApplicantsList({ applicants, onStatusChange }) {
  const [expanded, setExpanded] = useState(null);

  const nextStatus = (current) => {
    if (current === 'Received') return { status: 'InProgress', label: 'Iniciar Triagem' };
    if (current === 'InProgress') return { status: 'Approved', label: 'Aprovar' };
    return null;
  };

  const toggleExpand = id => setExpanded(prev => prev === id ? null : id);

  return (
    <div className="flex flex-col gap-2.5">
      {(!applicants || applicants.length === 0) && (
        <p className="text-sm text-muted py-4 text-center">Nenhum candidato para esta vaga.</p>
      )}
      {applicants?.map(app => {
        const next = onStatusChange ? nextStatus(app.status) : null;
        const isOpen = expanded === (app.applicationId || app.id);
        return (
          <div key={app.applicationId || app.id} className="bg-white border border-line rounded-lg overflow-hidden">
            {/* Header row — always visible */}
            <div className="flex items-center justify-between gap-3 cursor-pointer select-none"
              onClick={() => toggleExpand(app.applicationId || app.id)}>
              <div className="flex items-center gap-3 min-w-0 px-4 py-3 flex-1">
                <div className="grid w-9 h-9 place-items-center bg-brand/10 text-brand font-bold rounded-lg text-xs flex-shrink-0 overflow-hidden">
                  {app.fotoPerfil ? (
                    <img src={`/uploads/fotos/${app.fotoPerfil}`} alt="" className="w-full h-full object-cover" />
                  ) : (
                    (app.candidateName || '?')[0]
                  )}
                </div>
                <div className="min-w-0 flex-1">
                  <p className="m-0 text-sm font-bold text-ink truncate">{app.candidateName}</p>
                  <p className="m-0 text-xs text-muted">{app.cpf} &middot; {date(app.appliedAt)}{app.areaAtuacao ? ` · ${app.areaAtuacao}` : ''}</p>
                </div>
              </div>
              <div className="flex items-center gap-2 flex-shrink-0 pr-4">
                <Pill variant={app.status === 'Approved' ? 'success' : app.status === 'Rejected' ? 'danger' : app.status === 'InProgress' ? 'warning' : 'default'}>
                  {translateAppStatus(app.status)}
                </Pill>
                <span className="text-xs text-muted transition-transform" style={{ transform: isOpen ? 'rotate(180deg)' : 'none' }}>▾</span>
              </div>
            </div>

            {/* Expanded details */}
            {isOpen && (
              <div className="border-t border-line px-4 py-4 flex flex-col gap-4 text-sm">
                {/* Contact & Links */}
                <div className="grid grid-cols-2 gap-3">
                  {app.phoneNumber && (
                    <div>
                      <span className="text-xs font-bold text-muted block">Telefone</span>
                      <span className="text-ink">{app.phoneNumber}</span>
                    </div>
                  )}
                  {app.linkedInUrl && (
                    <div>
                      <span className="text-xs font-bold text-muted block">LinkedIn</span>
                      <a href={app.linkedInUrl} target="_blank" rel="noopener noreferrer" className="text-brand hover:underline break-all">{app.linkedInUrl}</a>
                    </div>
                  )}
                  {app.portfolioUrl && (
                    <div>
                      <span className="text-xs font-bold text-muted block">Portfólio</span>
                      <a href={app.portfolioUrl} target="_blank" rel="noopener noreferrer" className="text-brand hover:underline break-all">{app.portfolioUrl}</a>
                    </div>
                  )}
                  {app.areaAtuacao && (
                    <div>
                      <span className="text-xs font-bold text-muted block">Área</span>
                      <span className="text-ink">{app.areaAtuacao}</span>
                    </div>
                  )}
                </div>

                {/* Summary */}
                {app.summary && (
                  <div>
                    <span className="text-xs font-bold text-muted block">Resumo</span>
                    <p className="m-0 text-ink whitespace-pre-wrap">{app.summary}</p>
                  </div>
                )}

                {/* Application details */}
                <div className="grid grid-cols-2 gap-3">
                  {app.availabilityPreference && (
                    <div>
                      <span className="text-xs font-bold text-muted block">Disponibilidade</span>
                      <span className="text-ink">{app.availabilityPreference}</span>
                    </div>
                  )}
                  {app.salaryExpectation && (
                    <div>
                      <span className="text-xs font-bold text-muted block">Pretensão salarial</span>
                      <span className="text-ink">{app.salaryExpectation}</span>
                    </div>
                  )}
                  {app.experienceNotes && (
                    <div className="col-span-2">
                      <span className="text-xs font-bold text-muted block">Observações</span>
                      <p className="m-0 text-ink whitespace-pre-wrap">{app.experienceNotes}</p>
                    </div>
                  )}
                </div>

                {/* Resume download */}
                {app.resumeFileName && (
                  <div>
                    <a href={`/api/candidaturas/${app.applicationId}/curriculo`} target="_blank" rel="noopener noreferrer"
                      className="inline-flex items-center gap-1.5 text-brand text-sm font-bold hover:underline">
                      📄 {app.resumeFileName}
                    </a>
                  </div>
                )}

                {/* Actions */}
                {next && (
                  <div className="flex items-center gap-2 pt-2 border-t border-line">
                    <Button size="sm" variant="primary" onClick={() => onStatusChange(app.applicationId || app.id, next.status)}>
                      {next.label}
                    </Button>
                    {app.status !== 'Rejected' && (
                      <Button size="sm" variant="danger" onClick={() => onStatusChange(app.applicationId || app.id, 'Rejected')}>
                        Recusar
                      </Button>
                    )}
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
