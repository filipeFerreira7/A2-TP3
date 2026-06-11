export default function Card({ className = '', children, onClick, ...props }) {
  return (
    <div
      className={`bg-white border border-line rounded-lg shadow-md p-4 ${onClick ? 'cursor-pointer' : ''} ${className}`}
      onClick={onClick}
      {...props}
    >
      {children}
    </div>
  );
}
