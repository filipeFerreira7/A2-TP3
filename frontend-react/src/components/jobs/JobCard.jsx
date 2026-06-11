import { useNavigate } from 'react-router-dom';
import Card from '../ui/Card';
import Pill from '../ui/Pill';
import Button from '../ui/Button';
import { money, translateWorkModel, translateLevel } from '../../utils/formatters';

export default function JobCard({ job }) {
  const navigate = useNavigate();
  const { id, title, company, location, workModel, level, minimumSalary, maximumSalary, description } = job;
  const compName = typeof company === 'object' ? company.name : company || '—';

  return (
    <Card className="h-full flex flex-col gap-2.5">
      <div className="flex items-start gap-3">
        <div className="grid w-10 h-10 place-items-center bg-brand/10 text-brand font-bold rounded-lg flex-shrink-0 text-sm">
          {compName[0]}
        </div>
        <div className="flex-1 min-w-0">
          <h4 className="m-0 text-sm font-bold text-ink truncate">{title}</h4>
          <p className="m-0 mt-0.5 text-xs text-muted truncate">{compName} &middot; {location}</p>
        </div>
      </div>
      <div className="flex flex-wrap gap-1.5 mt-1">
        <Pill variant="default">{translateWorkModel(workModel)}</Pill>
        <Pill variant="default">{translateLevel(level)}</Pill>
      </div>
          <p className="m-0 text-xs text-muted leading-relaxed line-clamp-2 flex-1">{description}</p>
          <p className="m-0 text-sm font-bold text-brand">{money(minimumSalary)} — {money(maximumSalary)}</p>
      <Button size="sm" className="w-full mt-1" onClick={() => navigate(`/vagas/${id}`)}>Saiba mais</Button>
    </Card>
  );
}
