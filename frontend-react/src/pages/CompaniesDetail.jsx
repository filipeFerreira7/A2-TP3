import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../api/axios';
import Button from '../components/ui/Button';
import LoadingSpinner from '../components/ui/LoadingSpinner';

export default function CompaniesDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [company, setCompany] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.get(`/empresas/${id}`).then(r => {
      setCompany(r.data);
    }).catch(() => navigate('/empresas'))
      .finally(() => setLoading(false));
  }, [id, navigate]);

  if (loading) return <LoadingSpinner />;
  if (!company) return <p className="text-sm text-muted">Empresa não encontrada.</p>;

  return (
    <div className="animate-fade-in max-w-3xl mx-auto flex flex-col gap-6">
      <Button variant="ghost" size="sm" onClick={() => navigate('/empresas')} style={{ alignSelf: 'flex-start' }}>&larr; Voltar</Button>

      <div className="bg-white border border-line rounded-2xl p-6 shadow-lg">
        <div className="flex items-start gap-4 mb-6">
          <div className="grid w-14 h-14 place-items-center bg-brand/10 text-brand font-bold rounded-xl text-xl flex-shrink-0">
            {(company.tradeName || 'E')[0]}
          </div>
          <div className="flex-1">
            <h1 className="m-0 text-2xl font-extrabold text-ink">{company.tradeName}</h1>
            <p className="m-0 mt-1 text-sm text-muted">{company.legalName}</p>
            {company.cnpj && <p className="m-0 mt-0.5 text-xs text-muted">CNPJ: {company.cnpj.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, '$1.$2.$3/$4-$5')}</p>}
          </div>
        </div>

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
      </div>
    </div>
  );
}
