import { useState, useEffect } from 'react';
import api from '../api/axios';
import SearchBar from '../components/ui/SearchBar';
import CompanyCard from '../components/companies/CompanyCard';
import LoadingSpinner from '../components/ui/LoadingSpinner';

export default function Companies() {
  const [companies, setCompanies] = useState([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.get('/empresas').then(r => setCompanies(r.data.empresas || r.data || [])).catch(() => {}).finally(() => setLoading(false));
  }, []);

  const filtered = companies.filter(c =>
    !search || (c.tradeName || '').toLowerCase().includes(search.toLowerCase())
  );

  if (loading) return <LoadingSpinner />;

  return (
    <div className="animate-fade-in flex flex-col gap-5">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
        <div>
          <h1 className="m-0 text-2xl font-extrabold text-ink">Empresas</h1>
          <p className="m-0 text-sm text-muted mt-0.5">{filtered.length} empresas cadastradas</p>
        </div>
        <SearchBar value={search} onChange={setSearch} placeholder="Buscar empresas..." />
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {filtered.map(c => <CompanyCard key={c.id} company={c} />)}
        {filtered.length === 0 && <p className="col-span-full text-sm text-muted py-8 text-center">Nenhuma empresa encontrada.</p>}
      </div>
    </div>
  );
}
