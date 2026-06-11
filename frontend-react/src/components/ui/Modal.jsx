import { useEffect } from 'react';

export default function Modal({ open, onClose, title, children }) {
  useEffect(() => {
    if (open) document.body.style.overflow = 'hidden';
    else document.body.style.overflow = '';
    return () => { document.body.style.overflow = ''; };
  }, [open]);

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-5" onClick={onClose}>
      <div className="bg-white rounded-xl w-full max-w-[640px] max-h-[90vh] overflow-y-auto p-7 shadow-2xl animate-fade-in" onClick={e => e.stopPropagation()}>
        <div className="flex justify-between items-center mb-5">
          <h3 className="text-xl font-bold m-0">{title}</h3>
          <button onClick={onClose} className="bg-transparent border-none text-muted text-2xl cursor-pointer hover:text-ink leading-none">&times;</button>
        </div>
        {children}
      </div>
    </div>
  );
}
