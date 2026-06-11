import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/axios';
import Button from '../components/ui/Button';
import { useAuth } from '../contexts/AuthContext';

const levels = [
  { v: 'Internship', l: 'Estágio' },
  { v: 'Junior', l: 'Júnior' },
  { v: 'Mid', l: 'Pleno' },
  { v: 'Senior', l: 'Sênior' },
  { v: 'Specialist', l: 'Especialista' },
  { v: 'Leadership', l: 'Liderança' },
];

const models = [
  { v: 'Remote', l: 'Remoto' },
  { v: 'Hybrid', l: 'Híbrido' },
  { v: 'OnSite', l: 'Presencial' },
];

export default function CreateJob() {
  const { isCompanyUser, user } = useAuth();
  const navigate = useNavigate();
  const [step, setStep] = useState(1);
  const [error, setError] = useState('');
  const [form, setForm] = useState({
    title: '', companyId: (user?.companyId || '').toString(),
    location: '', workModel: 'Remote', level: 'Junior',
    salaryMin: '', salaryMax: '',
    description: '', requirements: '', benefits: ''
  });

  const set = key => e => setForm(p => ({ ...p, [key]: e.target.value }));

  const next = () => { setStep(s => Math.min(s + 1, 3)); setError(''); };
  const prev = () => { setStep(s => Math.max(s - 1, 1)); setError(''); };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const payload = { ...form, salaryMin: form.salaryMin ? parseFloat(form.salaryMin) : null, salaryMax: form.salaryMax ? parseFloat(form.salaryMax) : null };
      await api.post('/vagas', payload);
      navigate('/dashboard');
    } catch (err) {
      setError(err.response?.data?.error || err.response?.data?.message || 'Erro ao criar vaga.');
    }
  };

  if (!isCompanyUser()) {
    return <div className="animate-fade-in"><p className="text-sm text-muted">Apenas recrutadores e gestores podem criar vagas.</p></div>;
  }

  const inputClass = 'w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all';

  return (
    <div className="animate-fade-in max-w-2xl mx-auto flex flex-col gap-6">
      <Button variant="ghost" size="sm" onClick={() => navigate('/dashboard')} style={{ alignSelf: 'flex-start' }}>&larr; Voltar ao Painel</Button>
      <div className="flex items-center gap-2">
        {[1, 2, 3].map(s => (
          <div key={s} className="flex items-center gap-2 flex-1">
            <div className={`grid w-8 h-8 place-items-center rounded-full text-xs font-bold ${step >= s ? 'bg-brand text-white' : 'bg-soft text-muted'}`}>{s}</div>
            <span className={`text-[11px] font-bold ${step >= s ? 'text-brand' : 'text-muted'}`}>
              {s === 1 ? 'Informações' : s === 2 ? 'Descrição' : 'Revisão'}
            </span>
            {s < 3 && <div className={`h-0.5 flex-1 ${step > s ? 'bg-brand' : 'bg-line'}`} />}
          </div>
        ))}
      </div>

      <form onSubmit={handleSubmit} className="bg-white border border-line rounded-2xl p-6 shadow-lg flex flex-col gap-4">
        {error && <div className="bg-red-50 border border-red-200 text-danger text-xs font-bold px-3.5 py-2.5 rounded-lg">{error}</div>}

        {step === 1 && (
          <>
            <h2 className="m-0 text-lg font-bold text-ink">Informações Básicas</h2>
            <div className="flex flex-col gap-1.5">
              <label className="text-xs font-bold text-muted">Título da Vaga</label>
              <input required value={form.title} onChange={set('title')} placeholder="Ex: Desenvolvedor React Pleno" className={inputClass} />
            </div>
            <div className="flex flex-col gap-1.5">
              <label className="text-xs font-bold text-muted">Local</label>
              <input required value={form.location} onChange={set('location')} placeholder="Ex: São Paulo, SP" className={inputClass} />
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-muted">Modelo</label>
                <select value={form.workModel} onChange={set('workModel')} className={inputClass}>
                  {models.map(m => <option key={m.v} value={m.v}>{m.l}</option>)}
                </select>
              </div>
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-muted">Nível</label>
                <select value={form.level} onChange={set('level')} className={inputClass}>
                  {levels.map(l => <option key={l.v} value={l.v}>{l.l}</option>)}
                </select>
              </div>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-muted">Salário Mínimo</label>
                <input type="number" value={form.salaryMin} onChange={set('salaryMin')} placeholder="3000" className={inputClass} />
              </div>
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-muted">Salário Máximo</label>
                <input type="number" value={form.salaryMax} onChange={set('salaryMax')} placeholder="6000" className={inputClass} />
              </div>
            </div>
          </>
        )}

        {step === 2 && (
          <>
            <h2 className="m-0 text-lg font-bold text-ink">Descrição e Requisitos</h2>
            <div className="flex flex-col gap-1.5">
              <label className="text-xs font-bold text-muted">Descrição</label>
              <textarea rows="5" required value={form.description} onChange={set('description')} placeholder="Descreva a vaga em detalhes..." className={`${inputClass} min-h-[120px] resize-y`} />
            </div>
            <div className="flex flex-col gap-1.5">
              <label className="text-xs font-bold text-muted">Requisitos</label>
              <textarea rows="4" value={form.requirements} onChange={set('requirements')} placeholder="Requisitos para a vaga..." className={`${inputClass} min-h-[100px] resize-y`} />
            </div>
            <div className="flex flex-col gap-1.5">
              <label className="text-xs font-bold text-muted">Benefícios</label>
              <textarea rows="3" value={form.benefits} onChange={set('benefits')} placeholder="Benefícios oferecidos..." className={`${inputClass} min-h-[80px] resize-y`} />
            </div>
          </>
        )}

        {step === 3 && (
          <>
            <h2 className="m-0 text-lg font-bold text-ink">Revisar</h2>
            <div className="bg-soft rounded-xl p-4 flex flex-col gap-2 text-sm">
              <p className="m-0"><strong>Título:</strong> {form.title}</p>
              <p className="m-0"><strong>Local:</strong> {form.location}</p>
              <p className="m-0"><strong>Modelo:</strong> {models.find(m => m.v === form.workModel)?.l}</p>
              <p className="m-0"><strong>Nível:</strong> {levels.find(l => l.v === form.level)?.l}</p>
              <p className="m-0"><strong>Salário:</strong> R$ {form.salaryMin || '—'} a R$ {form.salaryMax || '—'}</p>
              <p className="m-0"><strong>Descrição:</strong> {form.description?.slice(0, 200)}{form.description?.length > 200 ? '...' : ''}</p>
              {form.requirements && <p className="m-0"><strong>Requisitos:</strong> {form.requirements?.slice(0, 150)}...</p>}
              {form.benefits && <p className="m-0"><strong>Benefícios:</strong> {form.benefits}</p>}
            </div>
          </>
        )}

        <div className="flex justify-between mt-2">
          <div>
            {step > 1 && <Button type="button" variant="secondary" onClick={prev}>Voltar</Button>}
          </div>
          {step < 3 ? (
            <Button type="button" onClick={next}>Próximo</Button>
          ) : (
            <Button type="submit">Criar Vaga</Button>
          )}
        </div>
      </form>
    </div>
  );
}
