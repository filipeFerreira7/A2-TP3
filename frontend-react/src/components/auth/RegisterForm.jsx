import { useState } from 'react';
import Button from '../ui/Button';

const permissions = [
  { value: 'Candidate', label: 'Candidato', desc: 'Buscar vagas e candidatar-se' },
  { value: 'Recruiter', label: 'Recrutador', desc: 'Gerenciar vagas e candidatos' },
  { value: 'Manager', label: 'Gestor', desc: 'Coordenar equipe de recrutamento' },
];

export default function RegisterForm({ onSubmit, error }) {
  const [form, setForm] = useState({ fullName: '', email: '', password: '', primaryPermission: 'Candidate' });

  const handleSubmit = e => {
    e.preventDefault();
    onSubmit(form.fullName, form.email, form.password, form.primaryPermission);
  };

  const set = key => e => setForm(prev => ({ ...prev, [key]: e.target.value }));

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4 w-full max-w-sm">
      <h2 className="m-0 text-2xl font-extrabold text-ink">Criar Conta</h2>
      <p className="m-0 text-sm text-muted">Cadastre-se no JobConnect Pro</p>
      {error && <div className="bg-red-50 border border-red-200 text-danger text-xs font-bold px-3.5 py-2.5 rounded-lg">{error}</div>}

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-bold text-muted">Nome completo</label>
        <input type="text" required value={form.fullName} onChange={set('fullName')}
          className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all" />
      </div>
      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-bold text-muted">Email</label>
        <input type="email" required value={form.email} onChange={set('email')}
          className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all" />
      </div>
      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-bold text-muted">Senha</label>
        <input type="password" required value={form.password} onChange={set('password')}
          className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all" />
      </div>
      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-bold text-muted">Tipo de conta</label>
        <div className="flex flex-col gap-2">
          {permissions.map(p => (
            <label key={p.value} className={`flex items-start gap-3 p-2.5 rounded-lg border cursor-pointer transition-all ${form.primaryPermission === p.value ? 'border-brand bg-brand/5' : 'border-line'}`}>
              <input type="radio" name="permission" value={p.value} checked={form.primaryPermission === p.value} onChange={set('primaryPermission')} className="mt-0.5 accent-brand" />
              <div className="flex flex-col gap-0.5">
                <span className="text-sm font-bold text-ink">{p.label}</span>
                <span className="text-xs text-muted">{p.desc}</span>
              </div>
            </label>
          ))}
        </div>
      </div>
      <Button type="submit" size="lg" className="w-full">Criar Conta</Button>
    </form>
  );
}
