import { useState } from 'react';
import Button from '../ui/Button';

export default function LoginForm({ onSubmit, error }) {
  const [email, setEmail] = useState('candidato@jobconnect.com');
  const [password, setPassword] = useState('JobConnect@123');

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
        <select required value={email} onChange={e => { setEmail(e.target.value); setPassword('JobConnect@123'); }}
          className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all">
          <option value="candidato@jobconnect.com">candidato@jobconnect.com (Candidato)</option>
          <option disabled>──────────</option>
          <option value="gestor@jobconnect.com">gestor@jobconnect.com (Gestor - JobConnect)</option>
          <option value="recrutador@jobconnect.com">recrutador@jobconnect.com (Recrutador - JobConnect)</option>
          <option disabled>──────────</option>
          <option value="rodrigo.oliveira@agilemind.com.br">rodrigo.oliveira@agilemind.com.br (Gestor - AgileMind)</option>
          <option value="luciana.ferreira@agilemind.com.br">luciana.ferreira@agilemind.com.br (Recrutador - AgileMind)</option>
          <option disabled>──────────</option>
          <option value="amanda.costa@cloudforce.com.br">amanda.costa@cloudforce.com.br (Gestor - CloudForce)</option>
          <option value="paulo.henrique@cloudforce.com.br">paulo.henrique@cloudforce.com.br (Recrutador - CloudForce)</option>
          <option disabled>──────────</option>
          <option value="fernanda.lima@datamind.com.br">fernanda.lima@datamind.com.br (Gestor - DataMind)</option>
          <option value="ricardo.almeida@datamind.com.br">ricardo.almeida@datamind.com.br (Recrutador - DataMind)</option>
          <option disabled>──────────</option>
          <option value="mariana.santos@inovatech.com.br">mariana.santos@inovatech.com.br (Gestor - InovaTech)</option>
          <option value="carlos.silva@inovatech.com.br">carlos.silva@inovatech.com.br (Recrutador - InovaTech)</option>
        </select>
      </div>
      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-bold text-muted">Senha</label>
        <input type="password" required value={password} onChange={e => setPassword(e.target.value)}
          className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all" />
      </div>
      <Button type="submit" size="lg" className="w-full">Entrar</Button>
    </form>
  );
}
