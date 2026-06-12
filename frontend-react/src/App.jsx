import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from './contexts/AuthContext';
import AppLayout from './components/layout/AppLayout';
import Home from './pages/Home';
import Jobs from './pages/Jobs';
import Companies from './pages/Companies';
import CompaniesDetail from './pages/CompaniesDetail';
import Login from './pages/Login';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import JobDetailPage from './pages/JobDetailPage';
import CreateJob from './pages/CreateJob';
import EditJob from './pages/EditJob';
import ProcessTracking from './pages/ProcessTracking';
import KanbanPage from './pages/KanbanPage';
import Analytics from './pages/Analytics';
import CompleteProfile from './pages/CompleteProfile';
import MyProfile from './pages/MyProfile';
import AdminPage from './pages/AdminPage';
import LoadingSpinner from './components/ui/LoadingSpinner';

function ProtectedRoute({ children, roles }) {
  const { user, loading } = useAuth();
  if (loading) return <LoadingSpinner />;
  if (!user) return <Navigate to="/login" replace />;
  if (roles && !roles.some(r => user.primaryPermission === r || user.roles?.includes(r)))
    return <Navigate to="/" replace />;
  return children;
}

export default function App() {
  const { loading } = useAuth();

  if (loading) return <LoadingSpinner />;

  return (
    <Routes>
      {/* Auth pages (no layout) */}
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />

      {/* Main layout */}
      <Route element={<AppLayout />}>
        <Route path="/" element={<Home />} />
        <Route path="/vagas" element={<Jobs />} />
        <Route path="/vagas/:id" element={<JobDetailPage />} />
        <Route path="/empresas" element={<Companies />} />
        <Route path="/empresas/:id" element={<CompaniesDetail />} />

        {/* Protected routes */}
        <Route path="/dashboard" element={
          <ProtectedRoute><Dashboard /></ProtectedRoute>
        } />
        <Route path="/criar-vaga" element={
          <ProtectedRoute roles={['Recruiter', 'Manager']}><CreateJob /></ProtectedRoute>
        } />
        <Route path="/vaga-edit/:id" element={
          <ProtectedRoute roles={['Recruiter', 'Manager']}><EditJob /></ProtectedRoute>
        } />
        <Route path="/processo/:id" element={
          <ProtectedRoute roles={['Candidate']}><ProcessTracking /></ProtectedRoute>
        } />
        <Route path="/kanban" element={
          <ProtectedRoute roles={['Recruiter', 'Manager']}><KanbanPage /></ProtectedRoute>
        } />
        <Route path="/completar-perfil" element={
          <ProtectedRoute roles={['Candidate']}><CompleteProfile /></ProtectedRoute>
        } />
        <Route path="/meu-perfil" element={
          <ProtectedRoute roles={['Candidate']}><MyProfile /></ProtectedRoute>
        } />
        <Route path="/analytics" element={
          <ProtectedRoute roles={['Recruiter', 'Manager', 'Administrator']}><Analytics /></ProtectedRoute>
        } />
        <Route path="/admin" element={
          <ProtectedRoute roles={['Administrator']}><AdminPage /></ProtectedRoute>
        } />
      </Route>

      {/* Fallback */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
