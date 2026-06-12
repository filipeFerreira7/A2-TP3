import { useState, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import api from '../../api/axios';

export default function ProfileBanner() {
  const { user, isCandidate } = useAuth();
  const location = useLocation();
  const [incomplete, setIncomplete] = useState(false);
  const [dismissed, setDismissed] = useState(false);

  useEffect(() => {
    if (!user || !isCandidate()) { setIncomplete(false); return; }

    let cancelled = false;

    const check = async () => {
      try {
        const { data } = await api.get('/perfil?_=' + Date.now());
        if (cancelled) return;

        if (!data) { setIncomplete(true); return; }

        const hasArea = !!data.areaAtuacao;
        const hasEducacao = (data.educations?.length || 0) > 0;
        const hasExperiencia = (data.workExperiences?.length || 0) > 0;
        const hasSkills = (data.skills?.length || 0) > 0;
        const hasSummary = !!data.summary;

        setIncomplete(!hasArea || (!hasEducacao && !hasExperiencia && !hasSkills && !hasSummary));
      } catch {
        if (!cancelled) setIncomplete(true);
      }
    };

    check();

    const interval = setInterval(check, 5000);
    return () => { cancelled = true; clearInterval(interval); };
  }, [user, isCandidate, location.pathname]);

  if (dismissed || !incomplete) return null;

  return (
    <div className="fixed top-4 right-4 z-[60] max-w-sm animate-fade-in">
      <div className="bg-amber-50 border border-amber-300 rounded-xl shadow-lg px-4 py-3 flex items-start gap-3">
        <span className="text-lg flex-shrink-0 mt-0.5">⚠️</span>
        <div className="flex-1 min-w-0">
          <p className="m-0 text-sm font-bold text-amber-800">Perfil incompleto</p>
          <p className="m-0 text-xs text-amber-700 mt-0.5">
            Preencha suas informações para se destacar para as empresas.
          </p>
          <Link to="/completar-perfil"
            className="inline-block mt-1.5 text-xs font-bold text-amber-800 underline-offset-2 hover:underline">
            Completar perfil →
          </Link>
        </div>
        <button onClick={() => setDismissed(true)}
          className="bg-transparent border-none text-amber-600 cursor-pointer text-sm font-bold flex-shrink-0 hover:text-amber-800">
          ✕
        </button>
      </div>
    </div>
  );
}
