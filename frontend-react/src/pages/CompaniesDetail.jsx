import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../api/axios';
import Button from '../components/ui/Button';
import LoadingSpinner from '../components/ui/LoadingSpinner';
import { useAuth } from '../contexts/AuthContext';

export default function CompaniesDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { isAdmin } = useAuth();
  const [company, setCompany] = useState(null);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(false);
  const [form, setForm] = useState({});
  const [message, setMessage] = useState('');

  useEffect(() => {
    api.get(`/empresas/${id}`).then(r => {
      setCompany(r.data);
      setForm(r.data);
    }).catch(() => navigate('/empresas'))
      .finally(() => setLoading(false));
  }, [id, navigate]);

  const set = key => e => setForm(p => ({ ...p, [key]: e.target.value }));

  const handleSave = async () => {
    try {
      await api.put(`/admin/empresas/${id}`, form);
      setCompany(form);
      setEditing(false);
      setMessage('Empresa atualizada com sucesso!');
    } catch {
      setMessage('Erro ao atualizar empresa.');
    }
  };

  const handleDelete = async () => {
    if (!window.confirm('Você tem certeza que deseja excluir esta empresa?')) return;
    try {
      await api.delete(`/admin/empresas/${id}`);
      navigate('/empresas');
    } catch {
      setMessage('Erro ao excluir empresa.');
    }
  };

  if (loading) return <LoadingSpinner />;
  if (!company) return <p className="text-sm text-muted">Empresa não encontrada.</p>;

  const inputClass = 'w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all';

  return (
    <div className="animate-fade-in max-w-3xl mx-auto flex flex-col gap-6">
      <Button variant="ghost" size="sm" onClick={() => navigate('/empresas')} style={{ alignSelf: 'flex-start' }}>&larr; Voltar</Button>

      {message && (
        <div className="bg-ok/10 border border-ok/30 text-ok text-xs font-bold px-3.5 py-2.5 rounded-lg flex items-center justify-between">
          <span>{message}</span>
          <button onClick={() => setMessage('')} className="bg-transparent border-none cursor-pointer text-ok text-sm">&times;</button>
        </div>
      )}

      <div className="bg-white border border-line rounded-2xl p-6 shadow-lg">
        <div className="flex items-start gap-4 mb-6">
          <div className="grid w-14 h-14 place-items-center bg-brand/10 text-brand font-bold rounded-xl text-xl flex-shrink-0">
            {(company.tradeName || 'E')[0]}
          </div>
          <div className="flex-1">
            {editing ? (
              <input value={form.tradeName} onChange={set('tradeName')} className={`${inputClass} text-xl font-extrabold`} />
            ) : (
              <h1 className="m-0 text-2xl font-extrabold text-ink">{company.tradeName}</h1>
            )}
            {editing ? (
              <input value={form.legalName} onChange={set('legalName')} className={`${inputClass} mt-1`} />
            ) : (
              <p className="m-0 mt-1 text-sm text-muted">{company.legalName}</p>
            )}
            {company.cnpj && <p className="m-0 mt-0.5 text-xs text-muted">CNPJ: {company.cnpj.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, '$1.$2.$3/$4-$5')}</p>}
          </div>
          {isAdmin() && !editing && (
            <div className="flex gap-2">
              <Button size="sm" onClick={() => setEditing(true)}>Editar</Button>
              <Button size="sm" variant="danger" onClick={handleDelete}>Excluir</Button>
            </div>
          )}
          {isAdmin() && editing && (
            <div className="flex gap-2">
              <Button size="sm" onClick={handleSave}>Salvar</Button>
              <Button size="sm" variant="secondary" onClick={() => { setEditing(false); setForm(company); }}>Cancelar</Button>
            </div>
          )}
        </div>

        {editing ? (
          <>
            <div className="mb-6">
              <h2 className="m-0 text-lg font-bold text-ink mb-2">Sobre a empresa</h2>
              <textarea rows="4" value={form.description || ''} onChange={set('description')} className={`${inputClass} min-h-[100px] resize-y`} />
            </div>

            <hr className="border-line my-6" />

            <div className="mb-6">
              <h2 className="m-0 text-lg font-bold text-ink mb-3">Informações de contato</h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                <div className="flex flex-col gap-1">
                  <label className="text-xs font-bold text-muted">Email</label>
                  <input value={form.email} onChange={set('email')} className={inputClass} />
                </div>
                <div className="flex flex-col gap-1">
                  <label className="text-xs font-bold text-muted">Telefone</label>
                  <input value={form.phoneNumber || ''} onChange={set('phoneNumber')} className={inputClass} />
                </div>
                <div className="flex flex-col gap-1 md:col-span-2">
                  <label className="text-xs font-bold text-muted">LinkedIn</label>
                  <input value={form.linkedInUrl || ''} onChange={set('linkedInUrl')} className={inputClass} />
                </div>
              </div>
            </div>

            <hr className="border-line my-6" />

            <div className="mb-6">
              <h2 className="m-0 text-lg font-bold text-ink mb-3">Endereço</h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                <div className="flex flex-col gap-1 md:col-span-2">
                  <label className="text-xs font-bold text-muted">Logradouro</label>
                  <input value={form.street || ''} onChange={set('street')} className={inputClass} />
                </div>
                <div className="flex flex-col gap-1">
                  <label className="text-xs font-bold text-muted">Número</label>
                  <input value={form.number || ''} onChange={set('number')} className={inputClass} />
                </div>
                <div className="flex flex-col gap-1">
                  <label className="text-xs font-bold text-muted">Complemento</label>
                  <input value={form.complement || ''} onChange={set('complement')} className={inputClass} />
                </div>
                <div className="flex flex-col gap-1">
                  <label className="text-xs font-bold text-muted">Bairro</label>
                  <input value={form.district || ''} onChange={set('district')} className={inputClass} />
                </div>
                <div className="flex flex-col gap-1">
                  <label className="text-xs font-bold text-muted">Município</label>
                  <input value={form.city || ''} onChange={set('city')} className={inputClass} />
                </div>
                <div className="flex flex-col gap-1">
                  <label className="text-xs font-bold text-muted">Estado</label>
                  <input value={form.state || ''} onChange={set('state')} className={inputClass} />
                </div>
                <div className="flex flex-col gap-1">
                  <label className="text-xs font-bold text-muted">CEP</label>
                  <input value={form.zipCode || ''} onChange={set('zipCode')} className={inputClass} />
                </div>
                <div className="flex flex-col gap-1">
                  <label className="text-xs font-bold text-muted">Ativa</label>
                  <select value={form.isActive} onChange={e => setForm(p => ({ ...p, isActive: e.target.value === 'true' }))} className={inputClass}>
                    <option value="true">Sim</option>
                    <option value="false">Não</option>
                  </select>
                </div>
              </div>
            </div>
          </>
        ) : (
          <>
            {company.description && (
              <div className="mb-6">
                <h2 className="m-0 text-lg font-bold text-ink mb-2">Sobre a empresa</h2>
                <p className="m-0 text-sm text-muted leading-relaxed whitespace-pre-line">{company.description}</p>
              </div>
            )}

            <hr className="border-line my-6" />

            <div className="mb-6">
              <h2 className="m-0 text-lg font-bold text-ink mb-2">Informações de contato</h2>
              <div className="flex flex-col gap-2 text-sm text-muted">
                {company.email && (
                  <div className="flex items-center gap-2">
                    <span className="font-medium text-ink w-20">Email:</span>
                    <span>{company.email}</span>
                  </div>
                )}
                {company.phoneNumber && (
                  <div className="flex items-center gap-2">
                    <span className="font-medium text-ink w-20">Telefone:</span>
                    <span>{company.phoneNumber}</span>
                  </div>
                )}
                {company.linkedInUrl && (
                  <div className="flex items-center gap-2">
                    <span className="font-medium text-ink w-20">LinkedIn:</span>
                    <a href={company.linkedInUrl} target="_blank" rel="noopener noreferrer" className="text-brand hover:underline">{company.linkedInUrl}</a>
                  </div>
                )}
              </div>
            </div>

            {(company.street || company.city || company.state) && (
              <>
                <hr className="border-line my-6" />
                <div className="mb-6">
                  <h2 className="m-0 text-lg font-bold text-ink mb-2">Endereço</h2>
                  <div className="flex flex-col gap-2 text-sm text-muted">
                    {company.street && (
                      <div className="flex items-center gap-2">
                        <span className="font-medium text-ink w-20">Logradouro:</span>
                        <span>{company.street}{company.number ? `, ${company.number}` : ''}{company.complement ? ` - ${company.complement}` : ''}</span>
                      </div>
                    )}
                    {company.district && (
                      <div className="flex items-center gap-2">
                        <span className="font-medium text-ink w-20">Bairro:</span>
                        <span>{company.district}</span>
                      </div>
                    )}
                    {company.city && (
                      <div className="flex items-center gap-2">
                        <span className="font-medium text-ink w-20">Município:</span>
                        <span>{company.city}</span>
                      </div>
                    )}
                    {company.state && (
                      <div className="flex items-center gap-2">
                        <span className="font-medium text-ink w-20">Estado:</span>
                        <span>{company.state}</span>
                      </div>
                    )}
                    {company.zipCode && (
                      <div className="flex items-center gap-2">
                        <span className="font-medium text-ink w-20">CEP:</span>
                        <span>{company.zipCode.replace(/^(\d{5})(\d{3})$/, '$1-$2')}</span>
                      </div>
                    )}
                  </div>
                </div>
              </>
            )}
          </>
        )}
      </div>
    </div>
  );
}
