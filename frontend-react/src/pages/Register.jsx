import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import RegisterForm from '../components/auth/RegisterForm';

export default function Register() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState('');

  const handleRegister = async (fullName, email, password, cpf, phone, areaAtuacao) => {
    try {
      setError('');
      await register(fullName, email, password, cpf, phone, areaAtuacao);
      navigate('/completar-perfil', { replace: true });
    } catch (err) {
      const data = err.response?.data;
      if (Array.isArray(data)) {
        setError(data.join('\n'));
      } else {
        setError(data?.error || data?.message || 'Erro ao cadastrar. Tente novamente.');
      }
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-soft p-5">
      <div className="bg-surface border border-line rounded-2xl p-8 shadow-xl w-full max-w-sm flex flex-col items-center gap-5">
        <img src="/logoo.png" alt="JobConnect Pro" className="w-16 h-16 rounded-xl object-contain" />
        <RegisterForm onSubmit={handleRegister} error={error} />
        <p className="m-0 text-xs text-muted">
          Já tem conta? <Link to="/login" className="text-brand font-bold underline-offset-2 hover:underline">Entrar</Link>
        </p>
      </div>
    </div>
  );
}
