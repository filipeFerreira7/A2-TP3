import { createContext, useContext, useState, useEffect, useCallback } from 'react';
import api from '../api/axios';

const TOKEN_KEY = 'jc_token';

function decodeToken() {
  const token = localStorage.getItem(TOKEN_KEY);
  if (!token) return null;
  try {
    return JSON.parse(atob(token.split('.')[1]));
  } catch { return null; }
}
const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  const fetchUser = useCallback(async () => {
    const token = localStorage.getItem(TOKEN_KEY);
    if (!token) { setUser(null); setLoading(false); return; }
    try {
      const { data } = await api.get('/auth/me');
      setUser(data);
    } catch {
      localStorage.removeItem(TOKEN_KEY);
      setUser(null);
    }
    setLoading(false);
  }, []);

  useEffect(() => { fetchUser(); }, [fetchUser]);

  const login = async (email, password) => {
    const { data } = await api.post('/auth/login', { email, password });
    localStorage.setItem(TOKEN_KEY, data.token);
    await fetchUser();
    return data;
  };

  const register = async (fullName, email, password, primaryPermission) => {
    const { data } = await api.post('/auth/register', { fullName, email, password, primaryPermission });
    localStorage.setItem(TOKEN_KEY, data.token);
    await fetchUser();
    return data;
  };

  const loginWithToken = async (token) => {
    localStorage.setItem(TOKEN_KEY, token);
    await fetchUser();
  };

  const logout = async () => {
    try { await api.post('/auth/logout'); } catch {}
    localStorage.removeItem(TOKEN_KEY);
    setUser(null);
  };

  const hasRole = role => user?.roles?.includes(role) || user?.primaryPermission === role;
  const isCandidate = () => hasRole('Candidate');
  const isRecruiter = () => hasRole('Recruiter');
  const isManager = () => hasRole('Manager');
  const isAdmin = () => hasRole('Administrator');
  const isCompanyUser = () => isRecruiter() || isManager() || isAdmin();
  const isLinkedInLogin = () => decodeToken()?.LoginProvider === 'LinkedIn';

  return (
    <AuthContext.Provider value={{ user, loading, login, register, loginWithToken, logout, fetchUser, isCandidate, isRecruiter, isManager, isAdmin, isCompanyUser, hasRole, isLinkedInLogin }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
