import { useState, useRef } from 'react';
import api from '../../api/axios';
import { useAuth } from '../../contexts/AuthContext';
import Button from '../ui/Button';

export default function ApplicationForm({ user, jobId, onSuccess, onCancel }) {
  const { isLinkedInLogin } = useAuth();
  const [fullName, setFullName] = useState(user?.fullName || '');
  const [email, setEmail] = useState(user?.email || '');
  const [cpf, setCpf] = useState('');
  const [resumeFile, setResumeFile] = useState(null);
  const [availabilityPreference, setAvailabilityPreference] = useState('');
  const [salaryExpectation, setSalaryExpectation] = useState('');
  const [experienceNotes, setExperienceNotes] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [linkedInLoading, setLinkedInLoading] = useState(false);
  const [error, setError] = useState('');
  const [linkedInMessage, setLinkedInMessage] = useState('');
  const fileRef = useRef(null);

  const handleLinkedInApply = async () => {
    if (!isLinkedInLogin()) {
      window.location.href = `/api/auth/linkedin/login?returnUrl=/vagas/${jobId}`;
      return;
    }
    setLinkedInLoading(true);
    setError('');
    setLinkedInMessage('');
    try {
      const { data } = await api.get('/auth/linkedin/profile');
      if (data.fullName) setFullName(data.fullName);
      if (data.email) setEmail(data.email);
      setLinkedInMessage('Dados do LinkedIn importados com sucesso! Preencha o restante.');
    } catch (err) {
      const msg = err.response?.data?.message || 'Erro ao importar dados do LinkedIn. Preencha manualmente.';
      setError(msg);
    }
    setLinkedInLoading(false);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!fullName.trim()) { setError('Nome completo é obrigatório.'); return; }
    if (!email.trim()) { setError('Email é obrigatório.'); return; }
    if (!cpf.trim()) { setError('CPF é obrigatório.'); return; }
    if (!availabilityPreference) { setError('Selecione a disponibilidade.'); return; }

    const formData = new FormData();
    formData.append('jobId', jobId);
    formData.append('fullName', fullName.trim());
    formData.append('cpf', cpf.trim().replace(/\D/g, ''));
    formData.append('availabilityPreference', availabilityPreference);
    if (salaryExpectation.trim()) formData.append('salaryExpectation', salaryExpectation.trim());
    if (experienceNotes.trim()) formData.append('experienceNotes', experienceNotes.trim());
    if (resumeFile) formData.append('resumeFile', resumeFile);

    setSubmitting(true);
    try {
      await api.post('/candidaturas/aplicar-com-perfil', formData);
      onSuccess();
    } catch (err) {
      setError(err.response?.data?.error || 'Erro ao enviar candidatura.');
    }
    setSubmitting(false);
  };

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4 border-t border-line pt-6 mt-6 animate-fade-in">
      <div className="flex items-center justify-between gap-4">
        <h3 className="m-0 text-lg font-bold text-ink">Formulário de Candidatura</h3>
        <button
          type="button"
          onClick={handleLinkedInApply}
          disabled={linkedInLoading}
          className="flex items-center gap-2 px-4 py-2 bg-[#0A66C2] text-white text-sm font-bold rounded-lg hover:bg-[#004182] transition-colors disabled:opacity-60 shrink-0 cursor-pointer border-none"
        >
          {linkedInLoading ? (
            <span>Carregando...</span>
          ) : (
            <>
              <svg className="w-4 h-4 fill-current" viewBox="0 0 24 24">
                <path d="M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433c-1.144 0-2.063-.926-2.063-2.065 0-1.138.92-2.063 2.063-2.063 1.14 0 2.064.925 2.064 2.063 0 1.139-.925 2.065-2.064 2.065zm1.782 13.019H3.555V9h3.564v11.452zM22.225 0H1.771C.792 0 0 .774 0 1.729v20.542C0 23.227.792 24 1.771 24h20.451c.979 0 1.771-.773 1.771-1.729V1.729C24 .774 23.204 0 22.225 0z"/>
              </svg>
              <span>Candidatar com LinkedIn</span>
            </>
          )}
        </button>
      </div>

      {error && <div className="bg-red-50 border border-red-200 text-danger text-xs font-bold px-3.5 py-2.5 rounded-lg">{error}</div>}
      {linkedInMessage && <div className="bg-green-50 border border-green-200 text-ok text-xs font-bold px-3.5 py-2.5 rounded-lg">{linkedInMessage}</div>}

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-bold text-muted">Nome completo *</label>
        <input type="text" required value={fullName} onChange={e => setFullName(e.target.value)}
          className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all" />
      </div>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-bold text-muted">Email *</label>
        <input type="email" required value={email} onChange={e => setEmail(e.target.value)}
          className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all" />
      </div>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-bold text-muted">CPF *</label>
        <input type="text" required value={cpf} onChange={e => setCpf(e.target.value)} placeholder="000.000.000-00"
          className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all" />
      </div>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-bold text-muted">Currículo (PDF, opcional)</label>
        <input type="file" ref={fileRef} accept=".pdf,application/pdf" onChange={e => setResumeFile(e.target.files[0] || null)}
          className="w-full text-sm text-muted file:mr-3 file:min-h-[36px] file:px-3 file:py-1.5 file:rounded-lg file:border file:border-line file:bg-soft file:text-sm file:font-bold file:text-ink file:cursor-pointer hover:file:bg-line transition-all" />
      </div>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-bold text-muted">Disponibilidade *</label>
        <select required value={availabilityPreference} onChange={e => setAvailabilityPreference(e.target.value)}
          className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all">
          <option value="">Selecione...</option>
          <option value="Remoto">Remoto</option>
          <option value="Híbrido">Híbrido</option>
          <option value="Presencial">Presencial</option>
        </select>
      </div>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-bold text-muted">Pretensão salarial</label>
        <textarea value={salaryExpectation} onChange={e => setSalaryExpectation(e.target.value)} rows={2} placeholder="Ex: R$ 5.000,00 — R$ 7.000,00"
          className="w-full px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all resize-none" />
      </div>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-bold text-muted">Experiências</label>
        <textarea value={experienceNotes} onChange={e => setExperienceNotes(e.target.value)} rows={3} placeholder="Conte um pouco sobre sua experiência profissional..."
          className="w-full px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all resize-none" />
      </div>

      <div className="flex gap-3 mt-2">
        <Button type="submit" disabled={submitting} size="lg" className="flex-1">
          {submitting ? 'Enviando...' : 'Enviar Candidatura'}
        </Button>
        <Button type="button" variant="secondary" size="lg" onClick={onCancel}>Cancelar</Button>
      </div>
    </form>
  );
}
