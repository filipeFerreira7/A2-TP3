export default function DashboardCards({ items }) {
  return (
    <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
      {items.map((item, i) => (
        <div key={i} className="bg-white border border-line rounded-xl px-5 py-4 shadow-md flex flex-col gap-1.5">
          <span className="text-xs font-bold text-muted">{item.label}</span>
          <span className="text-3xl font-extrabold text-ink">{item.value}</span>
          {item.sub && <span className="text-xs text-muted">{item.sub}</span>}
        </div>
      ))}
    </div>
  );
}
