import { useState, useEffect } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import LoginForm from '../components/auth/LoginForm';

export default function Login() {
  const { login, loginWithToken } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [error, setError] = useState('');

  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const token = params.get('token');
    const errorMsg = params.get('error');
    const returnUrl = params.get('returnUrl') || '/dashboard';

    if (errorMsg) {
      setError(decodeURIComponent(errorMsg));
      navigate('/login', { replace: true });
      return;
    }

    if (token) {
      loginWithToken(token)
        .then(() => {
          navigate(returnUrl, { replace: true });
        })
        .catch(() => {
          setError('Erro ao autenticar com LinkedIn.');
          navigate('/login', { replace: true });
        });
    }
  }, []);

  const handleLogin = async (email, password) => {
    try {
      setError('');
      await login(email, password);
      const from = location.state?.from?.pathname || '/dashboard';
      navigate(from, { replace: true });
    } catch (err) {
      setError(err.response?.data?.error || err.response?.data?.message || 'Erro ao entrar. Verifique suas credenciais.');
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-soft p-5">
      <div className="bg-surface border border-line rounded-2xl p-8 shadow-xl w-full max-w-sm flex flex-col items-center gap-5">
        <img src="/logoo.png" alt="JobConnect Pro" className="w-16 h-16 rounded-xl object-contain" />
        <LoginForm onSubmit={handleLogin} error={error} />
        <div className="flex items-center gap-3 w-full">
          <hr className="flex-1 border-line" />
          <span className="text-xs text-muted">ou</span>
          <hr className="flex-1 border-line" />
        </div>
        <a
          href={`/api/auth/linkedin/login?returnUrl=${encodeURIComponent(location.state?.from?.pathname || '/dashboard')}`}
          className="flex items-center justify-center gap-2 w-full min-h-[44px] px-4 py-2.5 bg-[#0A66C2] hover:bg-[#004182] text-white text-sm font-bold rounded-lg transition-colors no-underline"
        >
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" className="w-5 h-5 fill-current flex-shrink-0">
            <path d="M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433c-1.144 0-2.063-.926-2.063-2.065 0-1.138.92-2.063 2.063-2.063 1.14 0 2.064.925 2.064 2.063 0 1.139-.925 2.065-2.064 2.065zm1.782 13.019H3.555V9h3.564v11.452zM22.225 0H1.771C.792 0 0 .774 0 1.729v20.542C0 23.227.792 24 1.771 24h20.451C23.2 24 24 23.227 24 22.271V1.729C24 .774 23.2 0 22.222 0h.003z"/>
          </svg>
          Entrar com LinkedIn
        </a>
        <p className="m-0 text-xs text-muted">
          Não tem conta? <Link to="/register" className="text-brand font-bold underline-offset-2 hover:underline">Cadastre-se</Link>
        </p>
      </div>
    </div>
  );
}
