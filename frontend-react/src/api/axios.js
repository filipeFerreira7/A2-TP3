import axios from 'axios';

const TOKEN_KEY = 'jc_token';

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' }
});

api.interceptors.request.use(config => {
  const token = localStorage.getItem(TOKEN_KEY);
  if (token) config.headers.Authorization = `Bearer ${token}`;
  if (config.data instanceof FormData) delete config.headers['Content-Type'];
  return config;
});

api.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401 && !error.config.url.includes('/auth/')) {
      localStorage.removeItem(TOKEN_KEY);
      window.location.hash = '#/login';
    }
    return Promise.reject(error);
  }
);

export default api;
