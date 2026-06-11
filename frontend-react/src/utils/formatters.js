export const money = v =>
  v ? `R$ ${Number(v).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}` : 'A combinar';

export const date = (v) => {
    if (!v) return '—';                    // ou 'Data não informada'

    const data = new Date(v);

    // Verifica se a data é inválida
    if (isNaN(data.getTime())) {
        return 'Data inválida';
    }

    return data.toLocaleDateString('pt-BR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
    });
};

export const splitList = v => (v || '').split(',').map(s => s.trim()).filter(Boolean);

export const translateWorkModel = v =>
  ({ Remote: 'Remoto', Hybrid: 'Híbrido', OnSite: 'Presencial' }[v] || v);

export const translateLevel = v =>
  ({ Internship: 'Estágio', Junior: 'Júnior', Mid: 'Pleno', Senior: 'Sênior', Specialist: 'Especialista', Leadership: 'Liderança' }[v] || v);

export const translateAppStatus = v =>
  ({ Received: 'Recebida', InProgress: 'Em Andamento', Approved: 'Aprovada', Rejected: 'Recusada', Withdrawn: 'Cancelada' }[v] || v);

export const formatText = text => (text || '').replace(/\n/g, '<br>');
