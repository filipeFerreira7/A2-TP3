import api from '../../api/axios';
import { translateAppStatus } from '../../utils/formatters';

const columns = [
  { key: 'Received', label: 'Recebidas', color: 'bg-gray-100' },
  { key: 'InProgress', label: 'Em Andamento', color: 'bg-blue-50' },
  { key: 'Approved', label: 'Aprovadas', color: 'bg-green-50' },
  { key: 'Rejected', label: 'Recusadas', color: 'bg-red-50' },
];

export default function KanbanPipeline({ applications }) {
  const getColumnApps = key => applications?.filter(a => a.status === key) || [];

  const downloadResume = async (appId, fileName) => {
    try {
      const response = await api.get(`/candidaturas/${appId}/curriculo`, { responseType: 'blob' });
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', fileName || 'curriculo.pdf');
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch {}
  };

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-3">
      {columns.map(col => (
        <div key={col.key} className={`${col.color} rounded-xl p-3 min-h-[300px] flex flex-col gap-2`}>
          <div className="flex items-center justify-between mb-2">
            <span className="text-xs font-bold text-muted">{col.label}</span>
            <span className="bg-white text-[11px] font-bold text-muted px-2 py-0.5 rounded-full">{getColumnApps(col.key).length}</span>
          </div>
          {getColumnApps(col.key).map(app => (
            <div
              key={app.applicationId}
              className="bg-white border border-line rounded-lg px-3 py-2.5 shadow-sm flex flex-col gap-1.5"
            >
              <p className="m-0 text-sm font-bold text-ink truncate">Candidato: {app.candidateName || 'N/A'}</p>
              {app.resumeFileName && (
                <button
                  onClick={() => downloadResume(app.applicationId, app.resumeFileName)}
                  className="text-xs text-brand hover:text-brand-strong font-bold border-0 bg-transparent cursor-pointer p-0 text-left flex items-center gap-1"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" x2="12" y1="15" y2="3"/></svg>
                  {app.resumeFileName}
                </button>
              )}
            </div>
          ))}
        </div>
      ))}
    </div>
  );
}
