export default function LoadingSpinner({ text = 'Carregando...' }) {
  return (
    <div className="flex items-center justify-center py-12">
      <div className="flex flex-col items-center gap-3">
        <div className="w-8 h-8 border-3 border-line border-t-brand rounded-full animate-spin"></div>
        <span className="text-sm text-muted">{text}</span>
      </div>
    </div>
  );
}
