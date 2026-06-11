const tips = {
  Applied: 'Parab\u00e9ns pela candidatura! Prepare-se revisando seus dados e portf\u00f3lio.',
  Screening: 'Sua candidatura est\u00e1 sendo analisada. Mantenha seu curr\u00edculo atualizado.',
  Interview: 'Prepare-se para a entrevista. Pesquise sobre a empresa e pratique suas respostas.',
  Test: 'Voc\u00ea foi convidado para um teste t\u00e9cnnico. Leia atentamente as instru\u00e7\u00f5es.',
  Offer: 'Parab\u00e9ns! Voc\u00ea recebeu uma proposta. Revise os termos com aten\u00e7\u00e3o.',
  Hired: 'Bem-vindo ao time! Voc\u00ea foi contratado com sucesso.',
};

export default function ProcessTips({ currentStage = 'Applied' }) {
  const tip = tips[currentStage] || 'Continue acompanhando seu processo.';
  return (
    <div className="bg-brand/5 border border-brand/20 rounded-xl px-5 py-3.5 flex items-start gap-3">
      <span className="text-xl flex-shrink-0">\uD83D\uDCA1</span>
      <p className="m-0 text-sm text-ink leading-relaxed">{tip}</p>
    </div>
  );
}
