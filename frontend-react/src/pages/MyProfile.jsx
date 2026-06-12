import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/axios';
import { useAuth } from '../contexts/AuthContext';
import Button from '../components/ui/Button';
import LoadingSpinner from '../components/ui/LoadingSpinner';

const areas = [
  'Fullstack', 'Backend', 'Frontend', 'DevOps',
  'Mobile', 'Data Science', 'QA', 'UX/UI', 'Outro'
];

const nivelLabel = {
  1: 'Básico', 2: 'Intermediário', 3: 'Avançado'
};

export default function MyProfile() {
  const { user, isCandidate } = useAuth();
  const navigate = useNavigate();
  const fotoRef = useRef(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [editing, setEditing] = useState(false);

  const [form, setForm] = useState({
    fullName: '', cpf: '', birthDate: '', phone: '',
    linkedInUrl: '', portfolioUrl: '', areaAtuacao: '', summary: ''
  });
  const [fotoPreview, setFotoPreview] = useState(null);
  const [fotoFile, setFotoFile] = useState(null);
  const [resumeFile, setResumeFile] = useState(null);

  const [educations, setEducations] = useState([]);
  const [experiences, setExperiences] = useState([]);
  const [profileSkills, setProfileSkills] = useState([]);
  const [availableSkills, setAvailableSkills] = useState([]);
  const [existingSkillIds, setExistingSkillIds] = useState(new Set());
  const [stepErrors, setStepErrors] = useState([]);

  useEffect(() => {
    if (!user || !isCandidate()) {
      navigate('/dashboard', { replace: true });
      return;
    }
    loadProfile();
    loadSkills();
  }, [user]);

  const loadProfile = async () => {
    try {
      const { data } = await api.get('/perfil');
      if (data) {
        setForm({
          fullName: data.fullName || '',
          cpf: data.cpf || '',
          birthDate: data.birthDate || '',
          phone: data.phoneNumber || '',
          linkedInUrl: data.linkedInUrl || '',
          portfolioUrl: data.portfolioUrl || '',
          areaAtuacao: data.areaAtuacao || '',
          summary: data.summary || ''
        });
        setEducations(data.educations || []);
        setExperiences(data.workExperiences || []);
        const loaded = (data.skills || []).map(s => ({
          id: s.id, name: s.name,
          proficiencyLevel: Math.min(s.proficiencyLevel, 3)
        }));
        setProfileSkills(loaded);
        setExistingSkillIds(new Set(loaded.map(s => s.id)));
        if (data.fotoPerfil) setFotoPreview(`/uploads/fotos/${data.fotoPerfil}`);
      }
    } catch {}
    setLoading(false);
  };

  const loadSkills = async () => {
    try {
      const { data } = await api.get('/habilidades');
      setAvailableSkills(data || []);
    } catch {}
  };

  const set = key => e => setForm(prev => ({ ...prev, [key]: e.target.value }));

  const handleFotoChange = e => {
    const file = e.target.files?.[0];
    if (!file) return;
    setFotoFile(file);
    setFotoPreview(URL.createObjectURL(file));
  };

  const [newEducations, setNewEducations] = useState([]);
  const addNewEducacao = () => setNewEducations(prev => [...prev, { institution: '', course: '', degree: '', startDate: '', endDate: '' }]);
  const removeNewEducacao = i => setNewEducations(prev => prev.filter((_, idx) => idx !== i));
  const setNewEducacao = (i, key) => e => setNewEducations(prev => prev.map((item, idx) => idx === i ? { ...item, [key]: e.target.value } : item));

  const [newExperiences, setNewExperiences] = useState([]);
  const addNewExperiencia = () => setNewExperiences(prev => [...prev, { companyName: '', position: '', description: '', startDate: '', endDate: '', isCurrentJob: false }]);
  const removeNewExperiencia = i => setNewExperiences(prev => prev.filter((_, idx) => idx !== i));
  const setNewExperiencia = (i, key) => e => {
    const val = key === 'isCurrentJob' ? e.target.checked : e.target.value;
    setNewExperiences(prev => prev.map((item, idx) => idx === i ? { ...item, [key]: val } : item));
  };

  const toggleSkill = skill => {
    setProfileSkills(prev => {
      const exists = prev.find(s => s.id === skill.id);
      if (exists) return prev.filter(s => s.id !== skill.id);
      return [...prev, { id: skill.id, name: skill.name, proficiencyLevel: 1 }];
    });
  };

  const setSkillLevel = (i, level) => setProfileSkills(prev => prev.map((s, idx) => idx === i ? { ...s, proficiencyLevel: level } : s));

  const validate = () => {
    const errs = [];
    if (!form.fullName.trim()) errs.push('Nome completo é obrigatório.');
    if (!form.cpf.trim()) errs.push('CPF é obrigatório.');
    if (!form.areaAtuacao) errs.push('Selecione uma área de atuação.');
    for (const [i, edu] of newEducations.entries()) {
      if (edu.startDate && edu.endDate && edu.endDate < edu.startDate) {
        errs.push(`Formação #${i + 1}: a data de conclusão não pode ser anterior à data de início.`);
      }
    }
    for (const [i, exp] of newExperiences.entries()) {
      if (exp.startDate && exp.endDate && !exp.isCurrentJob && exp.endDate < exp.startDate) {
        errs.push(`Experiência #${i + 1}: a data de saída não pode ser anterior à data de início.`);
      }
    }
    setStepErrors(errs);
    return errs.length === 0;
  };

  const handleSave = async () => {
    if (!validate()) return;
    setSaving(true);
    setError('');
    setSuccess('');
    try {
      const fd = new FormData();
      fd.append('fullName', form.fullName);
      fd.append('cpf', form.cpf.replace(/\D/g, ''));
      fd.append('birthDate', form.birthDate);
      fd.append('phoneNumber', form.phone);
      fd.append('linkedInUrl', form.linkedInUrl);
      fd.append('portfolioUrl', form.portfolioUrl);
      fd.append('areaAtuacao', form.areaAtuacao);
      fd.append('summary', form.summary);
      if (fotoFile) fd.append('fotoPerfil', fotoFile);
      if (resumeFile) fd.append('resumeFile', resumeFile);
      await api.put('/perfil', fd, { headers: { 'Content-Type': 'multipart/form-data' } });

      for (const edu of newEducations) {
        if (!edu.institution || !edu.course || !edu.startDate) continue;
        await api.post('/perfil/educacao', {
          institution: edu.institution, course: edu.course, degree: edu.degree,
          startDate: edu.startDate, endDate: edu.endDate || null
        });
      }

      for (const exp of newExperiences) {
        if (!exp.companyName || !exp.position || !exp.startDate) continue;
        await api.post('/perfil/experiencia', {
          companyName: exp.companyName, position: exp.position, description: exp.description,
          startDate: exp.startDate, endDate: exp.endDate || null, isCurrentJob: exp.isCurrentJob
        });
      }

      for (const sk of profileSkills) {
        if (existingSkillIds.has(sk.id)) continue;
        await api.post('/perfil/habilidades', { skillId: sk.id, proficiencyLevel: Math.min(sk.proficiencyLevel, 3) });
      }

      setSuccess('Perfil atualizado com sucesso!');
      setEditing(false);
      setNewEducations([]);
      setNewExperiences([]);
      await loadProfile();
    } catch (err) {
      setError(err.response?.data?.error || 'Erro ao salvar perfil.');
    }
    setSaving(false);
  };

  const handleCancel = () => {
    setEditing(false);
    setStepErrors([]);
    setError('');
    setNewEducations([]);
    setNewExperiences([]);
    loadProfile();
  };

  if (loading) return <LoadingSpinner />;

  return (
    <div className="animate-fade-in max-w-2xl mx-auto py-8 px-5 flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="m-0 text-2xl font-extrabold text-ink">Meu Perfil</h1>
          <p className="m-0 text-sm text-muted mt-1">Suas informações de candidato</p>
        </div>
        {!editing ? (
          <Button onClick={() => setEditing(true)}>Editar Perfil</Button>
        ) : (
          <div className="flex gap-2">
            <Button variant="ghost" onClick={handleCancel} disabled={saving}>Cancelar</Button>
            <Button onClick={handleSave} disabled={saving}>
              {saving ? 'Salvando...' : 'Salvar'}
            </Button>
          </div>
        )}
      </div>

      {stepErrors.length > 0 && (
        <div className="bg-red-50 border border-red-200 text-danger text-xs font-bold px-3.5 py-2.5 rounded-lg flex flex-col gap-1">
          {stepErrors.map((e, i) => <span key={i}>{e}</span>)}
        </div>
      )}
      {error && stepErrors.length === 0 && <div className="bg-red-50 border border-red-200 text-danger text-xs font-bold px-3.5 py-2.5 rounded-lg">{error}</div>}
      {success && <div className="bg-green-50 border border-green-200 text-green-700 text-xs font-bold px-3.5 py-2.5 rounded-lg">{success}</div>}

      {/* Personal Info */}
      <div className="bg-surface border border-line rounded-2xl p-6 flex flex-col gap-4">
        <h3 className="m-0 text-lg font-bold text-ink">Dados Pessoais</h3>

        <div className="flex items-center gap-5">
          <div className="relative">
            <div
              className="w-24 h-24 rounded-full bg-brand/10 border-2 border-dashed border-brand flex items-center justify-center overflow-hidden cursor-pointer"
              onClick={() => editing && fotoRef.current?.click()}
            >
              {fotoPreview ? (
                <img src={fotoPreview} alt="foto" className="w-full h-full object-cover" />
              ) : (
                <span className="text-2xl text-muted">+</span>
              )}
            </div>
            {editing && <input ref={fotoRef} type="file" accept="image/*" className="hidden" onChange={handleFotoChange} />}
          </div>
          <div>
            <p className="m-0 text-sm font-bold text-ink">Foto de Perfil</p>
            <p className="m-0 text-xs text-muted">
              {editing ? 'Clique para adicionar (PNG, JPG, max 2MB)' : ''}
            </p>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div className="flex flex-col gap-1.5 col-span-2">
            <label className="text-xs font-bold text-muted">Nome completo</label>
            {editing ? (
              <input type="text" required value={form.fullName} onChange={set('fullName')}
                className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all" />
            ) : (
              <p className="m-0 text-sm text-ink font-medium">{form.fullName || '—'}</p>
            )}
          </div>
          <div className="flex flex-col gap-1.5">
            <label className="text-xs font-bold text-muted">CPF</label>
            {editing ? (
              <input type="text" required value={form.cpf} onChange={set('cpf')}
                className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all" />
            ) : (
              <p className="m-0 text-sm text-ink font-medium">{form.cpf || '—'}</p>
            )}
          </div>
          <div className="flex flex-col gap-1.5">
            <label className="text-xs font-bold text-muted">Data de Nascimento</label>
            {editing ? (
              <input type="date" value={form.birthDate} onChange={set('birthDate')}
                className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all" />
            ) : (
              <p className="m-0 text-sm text-ink font-medium">{form.birthDate || '—'}</p>
            )}
          </div>
          <div className="flex flex-col gap-1.5">
            <label className="text-xs font-bold text-muted">Telefone</label>
            {editing ? (
              <input type="tel" value={form.phone} onChange={set('phone')}
                className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all" />
            ) : (
              <p className="m-0 text-sm text-ink font-medium">{form.phone || '—'}</p>
            )}
          </div>
          <div className="flex flex-col gap-1.5">
            <label className="text-xs font-bold text-muted">Área de atuação</label>
            {editing ? (
              <select value={form.areaAtuacao} onChange={set('areaAtuacao')}
                className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all">
                <option value="">Selecione...</option>
                {areas.map(a => <option key={a} value={a}>{a}</option>)}
              </select>
            ) : (
              <p className="m-0 text-sm text-ink font-medium">{form.areaAtuacao || '—'}</p>
            )}
          </div>
          <div className="flex flex-col gap-1.5">
            <label className="text-xs font-bold text-muted">LinkedIn</label>
            {editing ? (
              <input type="url" value={form.linkedInUrl} onChange={set('linkedInUrl')} placeholder="https://linkedin.com/in/..."
                className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all" />
            ) : (
              <p className="m-0 text-sm text-ink font-medium">{form.linkedInUrl || '—'}</p>
            )}
          </div>
          <div className="flex flex-col gap-1.5">
            <label className="text-xs font-bold text-muted">Portfólio</label>
            {editing ? (
              <input type="url" value={form.portfolioUrl} onChange={set('portfolioUrl')} placeholder="https://github.com/..."
                className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all" />
            ) : (
              <p className="m-0 text-sm text-ink font-medium">{form.portfolioUrl || '—'}</p>
            )}
          </div>
          <div className="flex flex-col gap-1.5 col-span-2">
            <label className="text-xs font-bold text-muted">Resumo profissional</label>
            {editing ? (
              <textarea rows={3} value={form.summary} onChange={set('summary')} placeholder="Conte um pouco sobre sua trajetória e objetivos..."
                className="w-full px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all resize-y" />
            ) : (
              <p className="m-0 text-sm text-ink font-medium whitespace-pre-wrap">{form.summary || '—'}</p>
            )}
          </div>

        </div>
      </div>

      {/* Education */}
      <div className="bg-surface border border-line rounded-2xl p-6 flex flex-col gap-4">
        <div className="flex items-center justify-between">
          <h3 className="m-0 text-lg font-bold text-ink">Formação Acadêmica</h3>
          {editing && <Button size="sm" onClick={addNewEducacao}>+ Adicionar</Button>}
        </div>

        {educations.length === 0 && newEducations.length === 0 && (
          <p className="text-sm text-muted m-0">Nenhuma formação cadastrada.</p>
        )}

        {educations.map((edu, i) => (
          <div key={i} className="flex flex-col gap-1 p-4 bg-white border border-line rounded-xl">
            <p className="m-0 text-sm font-bold text-ink">{edu.course || 'Curso'}</p>
            <p className="m-0 text-xs text-muted">{edu.institution} {edu.degree ? `· ${edu.degree}` : ''}</p>
            <p className="m-0 text-xs text-muted">
              {edu.startDate ? edu.startDate.split('-')[0] : '?'} — {edu.endDate ? edu.endDate.split('-')[0] : 'Presente'}
            </p>
          </div>
        ))}

        {editing && newEducations.map((edu, i) => (
          <div key={`new-${i}`} className="flex flex-col gap-3 p-4 bg-white border border-line rounded-xl">
            <div className="flex justify-end">
              <button onClick={() => removeNewEducacao(i)}
                className="text-danger text-xs font-bold bg-transparent border-none cursor-pointer hover:underline">
                Remover
              </button>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div className="flex flex-col gap-1.5 col-span-2">
                <label className="text-xs font-bold text-muted">Instituição</label>
                <input type="text" value={edu.institution} onChange={setNewEducacao(i, 'institution')}
                  className="w-full min-h-[40px] px-3 py-2 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand" />
              </div>
              <div className="flex flex-col gap-1.5 col-span-2">
                <label className="text-xs font-bold text-muted">Curso</label>
                <input type="text" value={edu.course} onChange={setNewEducacao(i, 'course')}
                  className="w-full min-h-[40px] px-3 py-2 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand" />
              </div>
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-muted">Grau</label>
                <select value={edu.degree} onChange={setNewEducacao(i, 'degree')}
                  className="w-full min-h-[40px] px-3 py-2 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand">
                  <option value="">Selecione</option>
                  <option value="Ensino Médio">Ensino Médio</option>
                  <option value="Técnico">Técnico</option>
                  <option value="Graduação">Graduação</option>
                  <option value="Pós-Graduação">Pós-Graduação</option>
                  <option value="Mestrado">Mestrado</option>
                  <option value="Doutorado">Doutorado</option>
                  <option value="MBA">MBA</option>
                </select>
              </div>
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-muted">Data de início</label>
                <input type="date" value={edu.startDate} onChange={setNewEducacao(i, 'startDate')}
                  className="w-full min-h-[40px] px-3 py-2 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand" />
              </div>
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-muted">Data de conclusão</label>
                <input type="date" value={edu.endDate || ''} onChange={setNewEducacao(i, 'endDate')}
                  className="w-full min-h-[40px] px-3 py-2 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand" />
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Experience */}
      <div className="bg-surface border border-line rounded-2xl p-6 flex flex-col gap-4">
        <div className="flex items-center justify-between">
          <h3 className="m-0 text-lg font-bold text-ink">Experiência Profissional</h3>
          {editing && <Button size="sm" onClick={addNewExperiencia}>+ Adicionar</Button>}
        </div>

        {experiences.length === 0 && newExperiences.length === 0 && (
          <p className="text-sm text-muted m-0">Nenhuma experiência cadastrada.</p>
        )}

        {experiences.map((exp, i) => (
          <div key={i} className="flex flex-col gap-1 p-4 bg-white border border-line rounded-xl">
            <p className="m-0 text-sm font-bold text-ink">{exp.position || 'Cargo'}</p>
            <p className="m-0 text-xs text-muted">{exp.companyName}</p>
            {exp.description && <p className="m-0 text-xs text-muted mt-1">{exp.description}</p>}
            <p className="m-0 text-xs text-muted">
              {exp.startDate ? exp.startDate.split('-')[0] : '?'} — {exp.isCurrentJob ? 'Atual' : exp.endDate ? exp.endDate.split('-')[0] : '?'}
            </p>
          </div>
        ))}

        {editing && newExperiences.map((exp, i) => (
          <div key={`new-${i}`} className="flex flex-col gap-3 p-4 bg-white border border-line rounded-xl">
            <div className="flex justify-end">
              <button onClick={() => removeNewExperiencia(i)}
                className="text-danger text-xs font-bold bg-transparent border-none cursor-pointer hover:underline">
                Remover
              </button>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div className="flex flex-col gap-1.5 col-span-2">
                <label className="text-xs font-bold text-muted">Empresa</label>
                <input type="text" value={exp.companyName} onChange={setNewExperiencia(i, 'companyName')}
                  className="w-full min-h-[40px] px-3 py-2 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand" />
              </div>
              <div className="flex flex-col gap-1.5 col-span-2">
                <label className="text-xs font-bold text-muted">Cargo</label>
                <input type="text" value={exp.position} onChange={setNewExperiencia(i, 'position')}
                  className="w-full min-h-[40px] px-3 py-2 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand" />
              </div>
              <div className="flex flex-col gap-1.5 col-span-2">
                <label className="text-xs font-bold text-muted">Descrição</label>
                <textarea rows={2} value={exp.description} onChange={setNewExperiencia(i, 'description')}
                  className="w-full px-3 py-2 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand resize-y" />
              </div>
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-muted">Data de início</label>
                <input type="date" value={exp.startDate} onChange={setNewExperiencia(i, 'startDate')}
                  className="w-full min-h-[40px] px-3 py-2 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand" />
              </div>
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-muted">Data de saída</label>
                <input type="date" value={exp.endDate || ''} onChange={setNewExperiencia(i, 'endDate')} disabled={exp.isCurrentJob}
                  className="w-full min-h-[40px] px-3 py-2 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand disabled:opacity-40" />
              </div>
              <div className="flex flex-col gap-1.5 col-span-2">
                <label className="flex items-center gap-2 text-sm text-ink cursor-pointer">
                  <input type="checkbox" checked={exp.isCurrentJob} onChange={setNewExperiencia(i, 'isCurrentJob')} className="accent-brand" />
                  Cargo atual
                </label>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Skills */}
      <div className="bg-surface border border-line rounded-2xl p-6 flex flex-col gap-4">
        <h3 className="m-0 text-lg font-bold text-ink">Habilidades</h3>

        {profileSkills.length === 0 ? (
          <p className="text-sm text-muted m-0">Nenhuma habilidade cadastrada.</p>
        ) : (
          <>
            {editing && (
              <div className="flex flex-col gap-2">
                <span className="text-xs font-bold text-muted">NÍVEL DE PROFICIÊNCIA</span>
                <div className="flex flex-col gap-1.5">
                  {profileSkills.map((s, i) => (
                    <div key={i} className="flex items-center gap-3">
                      <span className="text-sm font-bold text-ink w-28">{s.name}</span>
                      <select value={s.proficiencyLevel} onChange={e => setSkillLevel(i, Number(e.target.value))}
                        className="min-h-[32px] px-2 py-1 bg-white border border-line rounded-lg text-sm font-bold focus:border-brand outline-none">
                        <option value={1}>Básico</option>
                        <option value={2}>Intermediário</option>
                        <option value={3}>Avançado</option>
                      </select>
                    </div>
                  ))}
                </div>
              </div>
            )}
            {!editing && (
              <div className="flex flex-wrap gap-2">
                {profileSkills.map((s, i) => (
                  <span key={i}
                    className="px-3 py-1.5 rounded-lg text-sm font-bold bg-brand text-white border border-brand">
                    {s.name} — {nivelLabel[s.proficiencyLevel]}
                  </span>
                ))}
              </div>
            )}
          </>
        )}

        {editing && (
          <>
            <span className="text-xs font-bold text-muted mt-2">ADICIONAR HABILIDADES</span>
            <div className="flex flex-wrap gap-2">
              {availableSkills.map(s => {
                const selected = profileSkills.find(ps => ps.id === s.id);
                return (
                  <button key={s.id} onClick={() => toggleSkill(s)}
                    className={`px-3 py-1.5 rounded-lg text-sm font-bold transition-all cursor-pointer ${selected
                      ? 'bg-brand text-white border border-brand'
                      : 'bg-white border border-line text-muted hover:border-brand hover:text-brand hover:bg-brand/5'
                    }`}>
                    {s.name}
                  </button>
                );
              })}
            </div>
          </>
        )}
      </div>
    </div>
  );
}
