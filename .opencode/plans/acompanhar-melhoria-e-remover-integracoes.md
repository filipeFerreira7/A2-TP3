# Plano: Melhorias na tela de acompanhamento + remover Integrações

## Objetivo
1. Adicionar dicas contextuais na tela `#acompanhar` (modelo de trabalho, nível da vaga, etapa atual)
2. Remover o item "Integrações" da sidebar (não funcional)

---

## 1. Remover "Integrações" da sidebar

### `frontend/index.html`
- Remover as 3 linhas do `<a href="#integracoes">` que incluem o ícone e o texto "Integracoes"

### `frontend/app.js` (linha 46)
- Remover `"integracoes"` do array `pages`

> **Impacto:** O hash `#integracoes` não será mais reconhecido pelo navegador — ao tentar acessá-lo, o usuário será redirecionado para `#inicio` (comportamento existente no `navigate()`).

---

## 2. Adicionar dados da vaga no endpoint `/process`

### `Dtos/ApplicationDtos.cs`
Adicionar 3 campos no record `ProcessResponse`:
```csharp
string WorkModel,   // "Remote", "Hybrid", "OnSite"
string Level,       // "Junior", "Mid", "Senior", etc.
string Description  // descrição da vaga
```

### `Controllers/ApplicationsController.cs` (linhas 367-376)
Atualizar a chamada do construtor de `ProcessResponse` para passar:
- `application.JobPosting.WorkModel.ToString()`
- `application.JobPosting.Level.ToString()`
- `application.JobPosting.Description`

---

## 3. Renderizar dicas no frontend

### `frontend/styles.css`
Adicionar estilos para `.process-tips` e `.tip-card`:
- Layout vertical com cards
- Cada card: padding 16px, borda arredondada, fundo claro (`var(--soft)`), margem inferior 12px
- Título em negrito, texto descritivo abaixo

### `frontend/app.js` — função `loadProcess()`
Após renderizar o fluxograma (`#processFlow`), adicionar uma seção de dicas no elemento `#processTips`:

```
📌 Dicas para o processo seletivo

💼 Modelo: {Remote|Hybrid|OnSite}
  Remote → "Prepare um ambiente silencioso e com boa iluminação
            para as videochamadas. Teste sua internet e câmera
            com antecedência."
  Hybrid → "Confirme os dias presenciais e remotos. Planeje seu
            deslocamento nos dias presenciais."
  OnSite → "Planeje sua rota e tempo de deslocamento. Separe
            documentos e itens necessários para o dia."

📊 Nível: {Junior|Mid|Senior|...}
  Junior/Internship → "Destaque sua vontade de aprender e sua
                       capacidade de resolver problemas."
  Mid/Senior → "Prepare exemplos concretos de projetos anteriores
                e métricas de impacto."
  Specialist/Leadership → "Esteja pronto para discutir arquitetura,
                           estratégia e mentoria de times."

🎯 Etapa atual: {Inscricao Realizada|Entrevista RH|...}
  Inscricao Realizada → "Revise seu currículo e prepare uma
                         apresentação pessoal."
  Entrevista RH       → "Pesquise a cultura e os valores da
                         empresa. Prepare um pitch pessoal de 2-3 min."
  Entrevista Gestor   → "Prepare exemplos técnicos e estudos de caso
                         alinhados com a descrição da vaga."
  Proposta Final      → "Reflita sobre suas expectativas salariais e
                         benefícios. Prepare perguntas sobre o pacote."
```

### `frontend/index.html`
Adicionar um container vazio `#processTips` na seção `#page-acompanhar`, após o `#processStatus`.

---

## Arquivos afetados (resumo)

| Arquivo | Tipo | Mudança |
|---|---|---|
| `frontend/index.html` | HTML | Remover nav + adicionar `#processTips` |
| `frontend/app.js` | JS | Remover `"integracoes"` do array + renderizar dicas |
| `frontend/styles.css` | CSS | Adicionar `.process-tips` e `.tip-card` |
| `Dtos/ApplicationDtos.cs` | C# | Adicionar 3 campos ao `ProcessResponse` |
| `Controllers/ApplicationsController.cs` | C# | Passar novos campos no endpoint |

## Ordem de execução sugerida
1. Backend: DTO → Controller
2. Frontend: HTML → CSS → JS
