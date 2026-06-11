const variants = {
  primary: 'bg-accent-strong text-white hover:opacity-90 border-transparent',
  secondary: 'bg-white text-ink border-line hover:bg-soft',
  danger: 'bg-danger text-white hover:opacity-90 border-transparent',
  ghost: 'bg-transparent text-muted border-transparent hover:text-ink'
};

const sizes = {
  sm: 'px-3 py-1.5 text-xs min-h-[32px]',
  md: 'px-4 py-2.5 text-sm min-h-[44px]',
  lg: 'px-6 py-3 text-base min-h-[52px]'
};

export default function Button({ variant = 'primary', size = 'md', className = '', children, ...props }) {
  return (
    <button
      className={`inline-flex items-center justify-center font-bold rounded-lg border transition-all duration-150 cursor-pointer ${variants[variant]} ${sizes[size]} ${className}`}
      {...props}
    >
      {children}
    </button>
  );
}
