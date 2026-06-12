import { useNavigate } from 'react-router-dom';
import Card from '../ui/Card';

export default function CompanyCard({ company }) {
  const { id, tradeName, city, state } = company;
  const navigate = useNavigate();

  return (
    <Card className="flex flex-col gap-2.5 h-full" onClick={() => navigate(`/empresas/${id}`)}>
      <div className="flex items-start gap-3">
        <div className="grid w-10 h-10 place-items-center bg-brand/10 text-brand font-bold rounded-lg flex-shrink-0 text-sm">
          {tradeName?.[0] || '?'}
        </div>
        <div className="flex-1 min-w-0">
          <h4 className="m-0 text-sm font-bold text-ink truncate">{tradeName}</h4>
          {(city || state) && <p className="m-0 mt-0.5 text-xs text-muted">{[city, state].filter(Boolean).join(', ')}</p>}
        </div>
      </div>
    </Card>
  );
}
