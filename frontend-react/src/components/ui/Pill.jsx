export default function Pill({ children, variant = 'default', className = '' }) {
  const colors = {
    default: 'bg-teal-50 text-brand-strong',
    success: 'bg-green-50 text-ok',
    danger: 'bg-red-50 text-danger',
    warning: 'bg-amber-50 text-accent-strong'
  };
  return (
    <span className={`inline-block px-2 py-1 rounded-full text-[11px] font-bold ${colors[variant]} ${className}`}>
      {children}
    </span>
  );
}
