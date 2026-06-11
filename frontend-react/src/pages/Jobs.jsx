import { useState, useEffect } from 'react';
import api from '../api/axios';
import SearchBar from '../components/ui/SearchBar';
import JobCard from '../components/jobs/JobCard';
import LoadingSpinner from '../components/ui/LoadingSpinner';

export default function Jobs() {
  const [jobs, setJobs] = useState([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.get('/vagas').then(r => setJobs(r.data.vagas || r.data || [])).catch(() => {}).finally(() => setLoading(false));
  }, []);

  const filtered = jobs.filter(j =>
    !search || (j.title || '').toLowerCase().includes(search.toLowerCase()) ||
    (j.description || '').toLowerCase().includes(search.toLowerCase()) ||
    (j.location || '').toLowerCase().includes(search.toLowerCase())
  );

  if (loading) return <LoadingSpinner />;

  return (
    <div className="animate-fade-in flex flex-col gap-5">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
        <div>
          <h1 className="m-0 text-2xl font-extrabold text-ink">Vagas</h1>
          <p className="m-0 text-sm text-muted mt-0.5">{filtered.length} vagas encontradas</p>
        </div>
        <SearchBar value={search} onChange={setSearch} placeholder="Buscar vagas..." />
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {filtered.map(job => <JobCard key={job.id} job={job} />)}
        {filtered.length === 0 && <p className="col-span-full text-sm text-muted py-8 text-center">Nenhuma vaga encontrada.</p>}
      </div>
    </div>
  );
}
