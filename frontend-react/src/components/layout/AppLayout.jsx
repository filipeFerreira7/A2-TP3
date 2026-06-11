import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';

export default function AppLayout() {
  return (
    <div className="flex min-h-screen">
      <Sidebar />
      <main className="ml-[240px] flex-1 min-h-screen p-8 md:p-12 lg:p-16">
        <Outlet />
      </main>
    </div>
  );
}
