# Documento de Escopo - JobConnect Pro

Data: 05/06/2026  
Tema: JobConnect Pro - Recrutamento Inteligente

## Objetivo

Desenvolver uma plataforma web fullstack para conectar empresas, recrutadores e candidatos em processos seletivos, com publicacao de vagas, candidaturas, acompanhamento de etapas, integracao externa e controle de acesso.

## Papeis

- Administrador: gerencia usuarios, empresas e configuracoes.
- Gestor: aprova vagas da empresa e acompanha indicadores.
- RH/Recrutador: cria vagas, acompanha candidatos e movimenta processos.
- Candidato: mantem curriculo, candidata-se e acompanha status.

## Escopo Entregue

- Backend ASP.NET Core com Entity Framework Core e SQL Server.
- Identity com login por cookie e endpoints autenticados.
- Frontend HTML, CSS e JavaScript responsivo.
- Versionamento de banco por EF Core Migrations.
- APIs publicas de vagas, empresas e habilidades.
- APIs autenticadas de dashboard, criacao/aprovacao de vagas e candidaturas.
- Integracao ViaCEP para consulta de endereco.
- Integracao LinkedIn para validacao de URL, compartilhamento de vaga e consumo oficial quando `LinkedIn:AccessToken` estiver configurado.

## Contas Demo

- `candidato@jobconnect.com` / `JobConnect@123`
- `recrutador@jobconnect.com` / `JobConnect@123`
- `gestor@jobconnect.com` / `JobConnect@123`

