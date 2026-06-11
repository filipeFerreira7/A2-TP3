const defaultSteps = [
  { key: 'Applied', label: 'Aplicou', icon: '\u2714\uFE0F' },
  { key: 'Screening', label: 'Triagem', icon: '\uD83D\uDD0D' },
  { key: 'Interview', label: 'Entrevista', icon: '\uD83D\uDCAC' },
  { key: 'Test', label: 'Teste T\u00e9cnico', icon: '\uD83D\uDCDD' },
  { key: 'Offer', label: 'Proposta', icon: '\uD83C\uDF93' },
  { key: 'Hired', label: 'Contratado', icon: '\uD83C\uDF89' },
];

export default function ProcessFlow({ currentStep = 0, steps = defaultSteps, style = {} }) {
  return (
    <div className="flex items-center gap-1 overflow-x-auto py-3" style={style}>
      {steps.map((step, i) => {
        const done = step.isCompleted ?? (i < currentStep);
        const active = step.isCurrent ?? (i === currentStep);
        return (
          <div key={step.key || i} className="flex items-center gap-1 flex-1 min-w-0">
            <div className={`flex items-center gap-1.5 px-3 py-1.5 rounded-full text-[11px] font-bold whitespace-nowrap transition-all ${
              active ? 'bg-brand text-white' : done ? 'bg-teal-50 text-brand-strong' : 'bg-soft text-muted'
            }`}>
              {done || active ? step.icon || '\u2714\uFE0F' : <span className="w-3.5 h-3.5 rounded-full border-2 border-line inline-block" />}
              {step.label}
            </div>
            {i < steps.length - 1 && <div className={`h-0.5 flex-1 ${done ? 'bg-brand' : 'bg-line'}`} />}
          </div>
        );
      })}
    </div>
  );
}
