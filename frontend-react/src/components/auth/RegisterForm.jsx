import { useState } from 'react';
import Button from '../ui/Button';

const areas = [
  'Fullstack', 'Backend', 'Frontend', 'DevOps',
  'Mobile', 'Data Science', 'QA', 'UX/UI', 'Outro'
];

export default function RegisterForm({ onSubmit, error }) {
  const [form, setForm] = useState({ fullName: '', email: '', password: '', cpf: '', phone: '', areaAtuacao: '' });

  const handleSubmit = e => {
    e.preventDefault();
    onSubmit(form.fullName, form.email, form.password, form.cpf, form.phone, form.areaAtuacao);
  };

  const set = key => e => setForm(prev => ({ ...prev, [key]: e.target.value }));

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4 w-full max-w-sm">
      <h2 className="m-0 text-2xl font-extrabold text-ink">Criar Conta</h2>
      <p className="m-0 text-sm text-muted">Cadastre-se no JobConnect Pro</p>
      {error && (
        <div className="bg-red-50 border border-red-200 text-danger text-xs font-bold px-3.5 py-2.5 rounded-lg">
          {error.includes('\n') ? (
            <ul className="m-0 pl-4 list-disc space-y-0.5">
              {error.split('\n').map((msg, i) => <li key={i}>{msg}</li>)}
            </ul>
          ) : error}
        </div>
      )}

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
        <label className="text-xs font-bold text-muted">CPF</label>
        <input type="text" required value={form.cpf} onChange={set('cpf')} placeholder="000.000.000-00"
          className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all" />
      </div>
      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-bold text-muted">Telefone</label>
        <input type="tel" value={form.phone} onChange={set('phone')} placeholder="(11) 99999-8888"
          className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all" />
      </div>
      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-bold text-muted">Senha</label>
        <input type="password" required value={form.password} onChange={set('password')}
          className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all" />
      </div>
      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-bold text-muted">Área de atuação</label>
        <select required value={form.areaAtuacao} onChange={set('areaAtuacao')}
          className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all">
          <option value="">Selecione...</option>
          {areas.map(a => <option key={a} value={a}>{a}</option>)}
        </select>
      </div>
      <Button type="submit" size="lg" className="w-full">Criar Conta</Button>
    </form>
  );
}
