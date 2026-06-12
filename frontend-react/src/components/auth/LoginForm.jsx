import { useState } from 'react';
import Button from '../ui/Button';

const allAccounts = [
  { email: 'admin@jobconnect.com', label: 'Admin (Plataforma)', password: 'Admin@123' },
  { email: 'gestor@jobconnect.com', label: 'Gestor (JobConnect)', password: 'JobConnect@123' },
  { email: 'recrutador@jobconnect.com', label: 'Recrutador (JobConnect)', password: 'JobConnect@123' },
  { email: 'candidato@jobconnect.com', label: 'Candidato (JobConnect)', password: 'JobConnect@123' },
  { email: 'rodrigo.oliveira@agilemind.com.br', label: 'Gestor (AgileMind)', password: 'JobConnect@123' },
  { email: 'luciana.ferreira@agilemind.com.br', label: 'Recrutador (AgileMind)', password: 'JobConnect@123' },
  { email: 'amanda.costa@cloudforce.com.br', label: 'Gestor (CloudForce)', password: 'JobConnect@123' },
  { email: 'paulo.henrique@cloudforce.com.br', label: 'Recrutador (CloudForce)', password: 'JobConnect@123' },
  { email: 'fernanda.lima@datamind.com.br', label: 'Gestor (DataMind)', password: 'JobConnect@123' },
  { email: 'ricardo.almeida@datamind.com.br', label: 'Recrutador (DataMind)', password: 'JobConnect@123' },
  { email: 'mariana.santos@inovatech.com.br', label: 'Gestor (InovaTech)', password: 'JobConnect@123' },
  { email: 'carlos.silva@inovatech.com.br', label: 'Recrutador (InovaTech)', password: 'JobConnect@123' },
];

export default function LoginForm({ onSubmit, error }) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const fillAccount = (a) => {
    setEmail(a.email);
    setPassword(a.password);
  };

  const handleSelect = (e) => {
    const selected = allAccounts.find(a => a.email === e.target.value);
    if (selected) fillAccount(selected);
  };

  const handleSubmit = e => {
    e.preventDefault();
    onSubmit(email, password);
  };

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4 w-full max-w-sm">
      <h2 className="m-0 text-2xl font-extrabold text-ink">Entrar</h2>
      <p className="m-0 text-sm text-muted">Acesse sua conta no JobConnect Pro</p>
      {error && <div className="bg-red-50 border border-red-200 text-danger text-xs font-bold px-3.5 py-2.5 rounded-lg">{error}</div>}
      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-bold text-muted">Email</label>
        <input type="email" required value={email} onChange={e => setEmail(e.target.value)} placeholder="Digite seu email..."
          className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all" />
      </div>
      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-bold text-muted">Senha</label>
        <input type="password" required value={password} onChange={e => setPassword(e.target.value)}
          className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all" />
      </div>
      <Button type="submit" size="lg" className="w-full">Entrar</Button>
      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-bold text-muted">Selecionar conta de teste</label>
        <select onChange={handleSelect} defaultValue=""
          className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all">
          <option value="" disabled>Escolha uma conta...</option>
          {allAccounts.map(a => (
            <option key={a.email} value={a.email}>{a.label}</option>
          ))}
        </select>
      </div>
    </form>
  );
}
