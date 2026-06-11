import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../api/axios';
import Button from '../components/ui/Button';
import { useAuth } from '../contexts/AuthContext';
import LoadingSpinner from '../components/ui/LoadingSpinner';

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

export default function EditJob() {
  const { id } = useParams();
  const { isCompanyUser } = useAuth();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [form, setForm] = useState(null);

  useEffect(() => {
    if (!isCompanyUser()) { navigate('/dashboard'); return; }
    api.get(`/vagas/${id}/editar`).then(r => {
      const j = r.data;
      setForm({
        title: j.title || '',
        level: j.level || 'Junior',
        workModel: j.workModel || 'Remote',
        location: j.location || '',
        minimumSalary: j.minimumSalary?.toString() || '',
        maximumSalary: j.maximumSalary?.toString() || '',
        openPositions: j.openPositions?.toString() || '1',
        closingDate: j.closingDate?.split('T')[0] || '',
        description: j.description || '',
        requirements: j.requirements || '',
        responsibilities: j.responsibilities || '',
        benefits: j.benefits || '',
        tags: j.tags || '',
        schedule: j.schedule || '',
        companyDescription: j.companyDescription || '',
        requiredSkills: j.requiredSkills?.join(', ') || '',
        differentialSkills: j.differentialSkills?.join(', ') || '',
      });
    }).catch(() => navigate('/dashboard'))
      .finally(() => setLoading(false));
  }, [id, navigate, isCompanyUser]);

  const set = key => e => setForm(p => ({ ...p, [key]: e.target.value }));

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    setError('');
    try {
      const payload = {
        title: form.title,
        description: form.description,
        minimumSalary: form.minimumSalary ? parseFloat(form.minimumSalary) : null,
        maximumSalary: form.maximumSalary ? parseFloat(form.maximumSalary) : null,
        workModel: form.workModel,
        level: form.level,
        openPositions: parseInt(form.openPositions, 10),
        closingDate: new Date(form.closingDate).toISOString(),
        tags: form.tags || null,
        benefits: form.benefits || null,
        location: form.location || null,
        companyDescription: form.companyDescription || null,
        responsibilities: form.responsibilities || null,
        requirements: form.requirements || null,
        schedule: form.schedule || null,
        requiredSkills: (form.requiredSkills || '').split(',').map(s => s.trim()).filter(Boolean),
        differentialSkills: (form.differentialSkills || '').split(',').map(s => s.trim()).filter(Boolean),
      };
      await api.put(`/vagas/${id}`, payload);
      navigate('/dashboard');
    } catch (err) {
      setError(err.response?.data?.error || err.response?.data?.message || 'Erro ao salvar vaga.');
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <LoadingSpinner />;
  if (!form) return <p className="text-sm text-muted">Vaga não encontrada.</p>;

  const inputClass = 'w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all';

  return (
    <div className="animate-fade-in max-w-2xl mx-auto flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <h1 className="m-0 text-xl font-extrabold text-ink">Editar Vaga</h1>
        <Button variant="ghost" size="sm" onClick={() => navigate('/dashboard')}>&larr; Voltar</Button>
      </div>

      <form onSubmit={handleSubmit} className="bg-white border border-line rounded-2xl p-6 shadow-lg flex flex-col gap-4">
        {error && <div className="bg-red-50 border border-red-200 text-danger text-xs font-bold px-3.5 py-2.5 rounded-lg">{error}</div>}

        <h2 className="m-0 text-lg font-bold text-ink">Informações Básicas</h2>

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-muted">Título da Vaga</label>
          <input required value={form.title} onChange={set('title')} placeholder="Ex: Desenvolvedor React Pleno" className={inputClass} />
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

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-muted">Local</label>
          <input value={form.location} onChange={set('location')} placeholder="Ex: São Paulo, SP" className={inputClass} />
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div className="flex flex-col gap-1.5">
            <label className="text-xs font-bold text-muted">Salário Mínimo</label>
            <input type="number" value={form.minimumSalary} onChange={set('minimumSalary')} placeholder="3000" className={inputClass} />
          </div>
          <div className="flex flex-col gap-1.5">
            <label className="text-xs font-bold text-muted">Salário Máximo</label>
            <input type="number" value={form.maximumSalary} onChange={set('maximumSalary')} placeholder="6000" className={inputClass} />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div className="flex flex-col gap-1.5">
            <label className="text-xs font-bold text-muted">Vagas Disponíveis</label>
            <input type="number" min="1" required value={form.openPositions} onChange={set('openPositions')} className={inputClass} />
          </div>
          <div className="flex flex-col gap-1.5">
            <label className="text-xs font-bold text-muted">Data de Encerramento</label>
            <input type="date" required value={form.closingDate} onChange={set('closingDate')} className={inputClass} />
          </div>
        </div>

        <hr className="border-line my-2" />
        <h2 className="m-0 text-lg font-bold text-ink">Descrição</h2>

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-muted">Sobre a empresa</label>
          <textarea rows="3" value={form.companyDescription} onChange={set('companyDescription')} placeholder="Descrição da empresa..." className={`${inputClass} min-h-[80px] resize-y`} />
        </div>

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-muted">Descrição da Vaga</label>
          <textarea rows="5" required value={form.description} onChange={set('description')} placeholder="Descreva a vaga em detalhes..." className={`${inputClass} min-h-[120px] resize-y`} />
        </div>

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-muted">Responsabilidades</label>
          <textarea rows="4" value={form.responsibilities} onChange={set('responsibilities')} placeholder="Responsabilidades do cargo..." className={`${inputClass} min-h-[100px] resize-y`} />
        </div>

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-muted">Requisitos</label>
          <textarea rows="4" value={form.requirements} onChange={set('requirements')} placeholder="Requisitos para a vaga..." className={`${inputClass} min-h-[100px] resize-y`} />
        </div>

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-muted">Benefícios</label>
          <textarea rows="3" value={form.benefits} onChange={set('benefits')} placeholder="Benefícios oferecidos..." className={`${inputClass} min-h-[80px] resize-y`} />
        </div>

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-muted">Horário</label>
          <input value={form.schedule} onChange={set('schedule')} placeholder="Ex: Segunda a Sexta, 9h às 18h" className={inputClass} />
        </div>

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-muted">Tags (separadas por vírgula)</label>
          <input value={form.tags} onChange={set('tags')} placeholder="Ex: React, C#, Azure" className={inputClass} />
        </div>

        <hr className="border-line my-2" />
        <h2 className="m-0 text-lg font-bold text-ink">Habilidades</h2>

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-muted">Habilidades Obrigatórias (separadas por vírgula)</label>
          <input value={form.requiredSkills} onChange={set('requiredSkills')} placeholder="Ex: React, TypeScript, SQL" className={inputClass} />
        </div>

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-muted">Habilidades Diferenciais (separadas por vírgula)</label>
          <input value={form.differentialSkills} onChange={set('differentialSkills')} placeholder="Ex: Docker, Kubernetes, AWS" className={inputClass} />
        </div>

        <div className="flex justify-between mt-2">
          <Button type="button" variant="secondary" onClick={() => navigate('/dashboard')}>Cancelar</Button>
          <Button type="submit" disabled={saving}>{saving ? 'Salvando...' : 'Salvar Alterações'}</Button>
        </div>
      </form>
    </div>
  );
}
