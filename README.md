<div align="center">
  <img src="frontend-react/public/logoo.png" alt="JobConnect Pro" width="80" height="80" style="border-radius:12px" />
  <h1 align="center">JobConnect Pro</h1>
  <p align="center">Plataforma inteligente de <strong>Tech Recruiter</strong> — conectando talentos de TI às melhores oportunidades.</p>
  <p align="center">
    <img src="https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet" alt=".NET 10" />
    <img src="https://img.shields.io/badge/React-19-61DAFB?logo=react" alt="React 19" />
    <img src="https://img.shields.io/badge/Vite-8-646CFF?logo=vite" alt="Vite 8" />
    <img src="https://img.shields.io/badge/Tailwind-4-06B6D4?logo=tailwindcss" alt="Tailwind 4" />
    <img src="https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver" alt="SQL Server" />
    <img src="https://img.shields.io/badge/Status-Em%20Desenvolvimento-yellow" alt="Status" />
  </p>
</div>

---

## Sobre

O **JobConnect Pro** é uma plataforma full-stack de recrutamento inteligente focada em tecnologia. Ela conecta **candidatos de TI**, **recrutadores** e **empresas** em um só lugar, com um pipeline visual estilo Kanban, analytics, aprovação hierárquica de vagas e integração com LinkedIn.

### Funcionalidades Principais

- **Gestão de Vagas de TI** — ciclo completo: rascunho → aprovação → publicação → fechamento
- **Kanban de Candidatos** — painel visual por etapas do processo seletivo
- **Autenticação via LinkedIn** — login social com importação automática do perfil
- **Analytics & Relatórios** — dashboards por papel (admin, empresa, candidato)
- **Perfil Tech de Candidatos** — histórico, stack, habilidades, linkedin, upload de currículo PDF
- **Empresas & Localização** — cadastro com endereço via busca por CEP (ViaCEP)
- **Registro de Auditoria** — rastreabilidade completa de ações no sistema
- **Soft Delete** — exclusão lógica em todas as entidades

---

## Stack

### Backend

| Camada | Tecnologia |
|---|---|
| Framework | **ASP.NET Core** (.NET 10) |
| ORM | **Entity Framework Core** 10 |
| Banco | **SQL Server** 2022 |
| Autenticação | **ASP.NET Core Identity** + **JWT Bearer** |
| API | REST JSON |

### Frontend

| Camada | Tecnologia |
|---|---|
| Build | **Vite** 8 |
| UI | **React** 19 + **JSX** |
| Estilo | **Tailwind CSS** 4 |
| Roteamento | **react-router-dom** 7 |
| HTTP | **Axios** |

---

## Arquitetura

```
├── Controllers/            # 7 controllers (Auth, Jobs, Applications, Dashboard, LinkedIn, Public, Integrations)
├── Services/               # TokenService, LinkedInService, ViaCepService, UnitOfWork
├── Repositories/           # Generic Repository<T> com soft delete
├── Entities/               # 24 entidades + enums
├── Dtos/                   # Request/Response records
├── Data/                   # DbContext + Seed (JobConnectSeed)
├── Migrations/             # 5 migrações EF Core
├── docs/                   # Diagramas e documentação
├── uploads/resumes/        # Upload de currículos PDF
└── frontend-react/         # SPA React + landing page
    ├── src/
    │   ├── api/            # Axios instance com interceptor JWT
    │   ├── contexts/       # AuthContext (estado global de autenticação)
    │   ├── pages/          # 13 páginas (Login, Dashboard, Vagas, Kanban, etc.)
    │   ├── components/     # 19 componentes reutilizáveis
    │   └── assets/         # Imagens e ícones
    └── landing-page/       # Página institucional estática (HTML/CSS/JS)
```

### Papéis (Roles)

| Papel | Permissões |
|---|---|
| **Candidate** | Visualizar vagas, candidatar-se, acompanhar processos |
| **Recruiter** | Gerenciar vagas, mover candidatos no kanban, avaliar |
| **Manager** | Aprovar vagas, dashboard gerencial, tudo do Recruiter |
| **Administrator** | Acesso total ao sistema |

---

## Como Executar

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/)
- SQL Server (LocalDB, Express ou superior)

### 1. Banco de Dados

O projeto usa `localhost\SQLEXPRESS` com **Windows Authentication** (Integrated Security). Para alterar, edite `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=job-connect-db;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### 2. Backend

```bash
# Restaurar pacotes
dotnet restore

# Executar (aplica migrations + seed automático na primeira execução)
dotnet run
```

A API sobe em `http://localhost:5205`.

### 3. Frontend

```bash
cd frontend-react
npm install
npm run dev
```

O Vite sobe em `http://localhost:5173` com proxy para `:5205`.

---

### Empresas parceiras

As empresas abaixo vêm pré-cadastradas com vagas publicadas, etapas de seleção e usuários vinculados:

| Empresa | Localização | Segmento |
|---|---|---|
| **AgileMind** | Curitiba, PR | Consultoria Ágil |
| **CloudForce** | Belo Horizonte, MG | Cloud & DevOps |
| **DataMind** | Rio de Janeiro, RJ | Inteligência de Dados |
| **InovaTech** | São Paulo, SP | Inovação Digital |
| **JobConnect Labs** | São Paulo, SP | Plataforma |

---

## API Endpoints

### Públicos (sem autenticação)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/vagas` | Listar vagas publicadas |
| `GET` | `/api/vagas/{id}` | Detalhes da vaga |
| `GET` | `/api/vagas/stats` | Estatísticas de vagas |
| `GET` | `/api/empresas` | Listar empresas |
| `GET` | `/api/empresas/{id}` | Detalhes da empresa |
| `GET` | `/api/habilidades` | Listar habilidades |
| `POST` | `/api/auth/login` | Login (email + senha) |
| `POST` | `/api/auth/register` | Cadastro |
| `GET` | `/api/integrations/viacep/{cep}` | Buscar endereço por CEP |

### Autenticados (requer JWT)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/auth/me` | Dados do usuário logado |
| `POST` | `/api/auth/logout` | Logout |
| `GET` | `/api/dashboard` | Dashboard por papel |
| `POST` | `/api/vagas` | Criar vaga |
| `PUT` | `/api/vagas/{id}` | Atualizar vaga |
| `POST` | `/api/vagas/{id}/aprovar` | Aprovar vaga (Manager) |
| `POST` | `/api/candidaturas` | Candidatar-se a vaga |
| `GET` | `/api/candidaturas/minhas` | Minhas candidaturas |
| `GET` | `/api/candidaturas/vaga/{id}` | Candidatos de uma vaga |
| `PUT` | `/api/candidaturas/{id}/status` | Avançar etapa no kanban |

---

## Integrações

### LinkedIn OAuth

Login social via LinkedIn OpenID Connect. O fluxo:

1. Usuário clica em "Entrar com LinkedIn"
2. Redireciona para `https://www.linkedin.com/oauth/v2/authorization`
3. LinkedIn redireciona de volta com `authorization_code`
4. Backend troca o código por um `access_token`
5. Busca perfil (`/userinfo`) — nome, email, foto
6. Cria ou localiza usuário e emite JWT

> **Nota:** Para usar em produção, configure `LinkedIn:ClientSecret` e `LinkedIn:ClientId` em `appsettings.json`.

### ViaCEP

Busca automática de endereço brasileiro a partir do CEP durante o cadastro de empresas.

---

## Landing Page

Uma landing page institucional estática está disponível em `frontend-react/landing-page/`:

```bash
cd frontend-react/landing-page
npx serve .
```

Ou abra o `index.html` diretamente no navegador.

---

## Documentação

Na pasta [`docs/`](docs/) você encontra:

- [`scope.md`](docs/scope.md) — Escopo do projeto
- [`presentation.md`](docs/presentation.md) — Roteiro de apresentação
- [`use-case-diagram.md`](docs/use-case-diagram.md) — Diagrama de casos de uso (Mermaid)
- [`database-diagram.md`](docs/database-diagram.md) — Diagrama do banco de dados (Mermaid)

---

## Licença

Projeto acadêmico — todos os direitos reservados.
