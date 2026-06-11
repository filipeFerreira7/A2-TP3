export default function SearchBar({ value, onChange, placeholder = 'Buscar...' }) {
  return (
    <div className="flex flex-col gap-1.5 w-full max-w-xs">
      <span className="text-xs font-bold text-muted">Buscar</span>
      <input
        type="search"
        value={value}
        onChange={e => onChange(e.target.value)}
        placeholder={placeholder}
        className="w-full min-h-[44px] px-3 py-2.5 bg-white border border-line rounded-lg outline-none text-sm focus:border-brand focus:ring-3 focus:ring-brand/15 transition-all"
      />
    </div>
  );
}
