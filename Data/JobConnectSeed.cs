using a2_tp3_job_connect.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace a2_tp3_job_connect.Data;

public static class JobConnectSeed
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<JobConnectDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await context.Database.MigrateAsync();
        await EnsureRolesAsync(roleManager);

        var candidate = await EnsureUserAsync(userManager, "Filipe Batista", "candidato@jobconnect.com", "JobConnect@123", UserPermission.Candidate);

        // JobConnect (plataforma)
        var jobConnectManager = await EnsureUserAsync(userManager, "Ana Oliveira", "gestor@jobconnect.com", "JobConnect@123", UserPermission.Manager);
        var jobConnectRecruiter = await EnsureUserAsync(userManager, "Pedro Santos", "recrutador@jobconnect.com", "JobConnect@123", UserPermission.Recruiter);

        // AgileMind
        var rodrigo = await EnsureUserAsync(userManager, "Rodrigo Oliveira", "rodrigo.oliveira@agilemind.com.br", "JobConnect@123", UserPermission.Manager);
        var luciana = await EnsureUserAsync(userManager, "Luciana Ferreira", "luciana.ferreira@agilemind.com.br", "JobConnect@123", UserPermission.Recruiter);

        // CloudForce
        var amanda = await EnsureUserAsync(userManager, "Amanda Costa", "amanda.costa@cloudforce.com.br", "JobConnect@123", UserPermission.Manager);
        var paulo = await EnsureUserAsync(userManager, "Paulo Henrique", "paulo.henrique@cloudforce.com.br", "JobConnect@123", UserPermission.Recruiter);

        // DataMind
        var fernanda = await EnsureUserAsync(userManager, "Fernanda Lima", "fernanda.lima@datamind.com.br", "JobConnect@123", UserPermission.Manager);
        var ricardo = await EnsureUserAsync(userManager, "Ricardo Almeida", "ricardo.almeida@datamind.com.br", "JobConnect@123", UserPermission.Recruiter);

        // InovaTech
        var mariana = await EnsureUserAsync(userManager, "Mariana Santos", "mariana.santos@inovatech.com.br", "JobConnect@123", UserPermission.Manager);
        var carlos = await EnsureUserAsync(userManager, "Carlos Silva", "carlos.silva@inovatech.com.br", "JobConnect@123", UserPermission.Recruiter);

        var skills = await EnsureSkillsAsync(context);
        var companies = await EnsureCompaniesAsync(context,
            (Manager: rodrigo, Recruiter: luciana),
            (Manager: amanda, Recruiter: paulo),
            (Manager: fernanda, Recruiter: ricardo),
            (Manager: mariana, Recruiter: carlos),
            (Manager: jobConnectManager, Recruiter: jobConnectRecruiter));

        foreach (var company in companies)
        {
            await EnsureStagesAsync(context, company);
        }
        await EnsureCandidateProfileAsync(context, candidate, skills);
        await EnsureJobsAsync(context, companies, luciana, skills);
        await EnsureNotificationsAsync(context, candidate);

        await context.SaveChangesAsync();
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roleNames = new[] { "Candidate", "Recruiter", "Manager", "Administrator" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string fullName,
        string email,
        string password,
        UserPermission permission)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null)
        {
            if (!string.IsNullOrEmpty(password) && !await userManager.CheckPasswordAsync(user, password))
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, token, password);
            }
            return user;
        }

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            PrimaryPermission = permission,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        var roleName = permission.ToString();
        if (!await userManager.IsInRoleAsync(user, roleName))
            await userManager.AddToRoleAsync(user, roleName);

        return user;
    }

    private static async Task<Dictionary<string, Habilidade>> EnsureSkillsAsync(JobConnectDbContext context)
    {
        var names = new[]
        {
            "C#", ".NET", "SQL Server", "Entity Framework", "React", "UX", "Azure", "Scrum",
            "Python", "Docker", "Machine Learning", "Power BI", "Node.js", "TypeScript",
            "Jenkins", "Git", "Kubernetes", "Figma", "Java", "Spring Boot", "PostgreSQL",
            "MongoDB", "AWS", "GCP", "Terraform", "Ansible", "Elasticsearch", "Kibana",
            "GraphQL", "Redis", "RabbitMQ", "Angular", "Vue.js", "Flutter", "Swift",
            "Kotlin", "Go", "Rust", "PHP", "Laravel", "WordPress", "SEO", "Google Ads",
            "Meta Ads", "Photoshop", "Illustrator", "After Effects", "Premiere", "AutoCAD",
            "Revit", "SketchUp", "Excel", "SAP", "Oracle", "ServiceNow", "Jira", "Confluence"
        };

        foreach (var name in names)
        {
            if (!await context.Habilidades.AnyAsync(skill => skill.Name == name))
                context.Habilidades.Add(new Habilidade { Name = name });
        }

        await context.SaveChangesAsync();
        return await context.Habilidades.ToDictionaryAsync(skill => skill.Name);
    }

    private static async Task<List<Empresa>> EnsureCompaniesAsync(
        JobConnectDbContext context,
        (ApplicationUser Manager, ApplicationUser Recruiter) agileMind,
        (ApplicationUser Manager, ApplicationUser Recruiter) cloudForce,
        (ApplicationUser Manager, ApplicationUser Recruiter) dataMind,
        (ApplicationUser Manager, ApplicationUser Recruiter) inovaTech,
        (ApplicationUser Manager, ApplicationUser Recruiter) jobConnect)
    {
        var companies = new List<Empresa>();

        async Task<Empresa> GetOrCreate(string cnpj, string legalName, string tradeName, string email, string phone, string linkedin,
            string zipCode, string street, string number, string district, string city, string state,
            string description,
            ApplicationUser manager, ApplicationUser recruiter)
        {
            var existing = await context.Empresas.Include(e => e.Address).FirstOrDefaultAsync(e => e.Cnpj == cnpj);
            if (existing is not null)
            {
                if (!await context.UsuariosEmpresa.AnyAsync(u => u.CompanyId == existing.Id && u.UserId == recruiter.Id))
                    context.UsuariosEmpresa.Add(new UsuarioEmpresa { CompanyId = existing.Id, UserId = recruiter.Id, Role = CompanyUserRole.Recruiter });
                if (!await context.UsuariosEmpresa.AnyAsync(u => u.CompanyId == existing.Id && u.UserId == manager.Id))
                    context.UsuariosEmpresa.Add(new UsuarioEmpresa { CompanyId = existing.Id, UserId = manager.Id, Role = CompanyUserRole.Manager });
                return existing;
            }

            var company = new Empresa
            {
                LegalName = legalName, TradeName = tradeName, Cnpj = cnpj, Email = email,
                PhoneNumber = phone, LinkedInUrl = linkedin, Description = description,
                Address = new EnderecoEmpresa
                {
                    ZipCode = zipCode, Street = street, Number = number, District = district,
                    City = city, State = state, ValidatedByViaCep = true, ValidatedAt = DateTime.UtcNow
                }
            };
            context.Empresas.Add(company);
            context.UsuariosEmpresa.Add(new UsuarioEmpresa { Company = company, UserId = recruiter.Id, Role = CompanyUserRole.Recruiter });
            context.UsuariosEmpresa.Add(new UsuarioEmpresa { Company = company, UserId = manager.Id, Role = CompanyUserRole.Manager });
            return company;
        }

        companies.Add(await GetOrCreate("12345678000190", "AgileMind Consultoria Ltda", "AgileMind",
            "contato@agilemind.com.br", "(11) 3333-0100", "https://linkedin.com/company/agilemind",
            "04538132", "Av. Brigadeiro Faria Lima", "4500", "Itaim Bibi", "Sao Paulo", "SP",
            @"A AgileMind e uma consultoria brasileira especializada em metodologias ageis e desenvolvimento de software sob medida. Ha mais de 6 anos no mercado, ajudamos empresas a transformar suas entregas por meio de praticas ageis, produtos digitais inovadores e equipes de alta performance.

Nosso time e composto por mais de 150 profissionais apaixonados por tecnologia e inovacao, distribuidos em 4 estados brasileiros. Atendemos clientes de diversos segmentos, do varejo a saude, da educacao a financas.

Valorizamos a diversidade, o aprendizado continuo e a colaboracao entre areas. Aqui, voce encontrara um ambiente descontraido, com autonomia para criar e espaco para crescer.",
            agileMind.Manager, agileMind.Recruiter));

        companies.Add(await GetOrCreate("22345678000191", "CloudForce Tecnologia S.A.", "CloudForce",
            "contato@cloudforce.com.br", "(31) 3333-0200", "https://linkedin.com/company/cloudforce",
            "30140071", "Av. do Contorno", "8500", "Savassi", "Belo Horizonte", "MG",
            @"A CloudForce e uma empresa de tecnologia especializada em infraestrutura em nuvem e DevOps. Ha mais de 8 anos no mercado, ajudamos empresas a modernizar suas operacoes de TI por meio de arquiteturas cloud native, automacao e boas praticas de engenharia de infraestrutura.

Nosso time e composto por mais de 80 profissionais apaixonados por tecnologia, incluindo engenheiros de nuvem, DevOps, arquitetos e especialistas em seguranca. Atendemos clientes de medio e grande porte em todo o Brasil.

Valorizamos o aprendizado continuo, a autonomia e a colaboracao. Aqui, voce encontrara projetos desafiadores, liberdade para experimentar novas tecnologias e um ambiente que incentiva a inovacao.",
            cloudForce.Manager, cloudForce.Recruiter));

        companies.Add(await GetOrCreate("32345678000192", "DataMind Analytics Ltda", "DataMind",
            "contato@datamind.com.br", "(41) 3333-0300", "https://linkedin.com/company/datamind",
            "80420130", "Rua Padre Anchieta", "2500", "Bigorrilho", "Curitiba", "PR",
            @"A DataMind e uma empresa de inteligencia de dados com sede em Curitiba, Parana. Somos especialistas em transformar dados brutos em decisoes estrategicas para empresas dos setores financeiro, varejo e saude.

Ha mais de 8 anos no mercado, nossa equipe de 70 profissionais inclui cientistas de dados, engenheiros, analistas e especialistas em machine learning. Trabalhamos com tecnologia de ponta: Python, Azure ML, SQL Server, Power BI e ferramentas de big data.

Acreditamos que os dados sao o ativo mais valioso das empresas, e nossa missao e ajudar organizacoes a extrair o maximo valor de suas informacoes. Valorizamos o rigor tecnico, a curiosidade intelectual e o trabalho em equipe.",
            dataMind.Manager, dataMind.Recruiter));

        companies.Add(await GetOrCreate("42345678000193", "InovaTech Solucoes Ltda", "InovaTech",
            "contato@inovatech.com.br", "(21) 3333-0400", "https://linkedin.com/company/inovatech",
            "20040002", "Av. Rio Branco", "200", "Centro", "Rio de Janeiro", "RJ",
            @"A InovaTech e uma empresa de tecnologia focada em inovacao digital e transformacao de negocios. Com mais de 10 anos de experiencia, ajudamos empresas a criar produtos digitais inovadores que geram impacto real.

Nossa equipe multidisciplinar combina expertise em design, engenharia e estrategia para entregar solucoes completas, desde aplicativos mobile ate plataformas web complexas.

Acreditamos na tecnologia como motor de transformacao e buscamos pessoas apaixonadas por criar produtos que fazem a diferenca na vida das pessoas.",
            inovaTech.Manager, inovaTech.Recruiter));

        companies.Add(await GetOrCreate("52345678000194", "JobConnect Plataforma Ltda", "JobConnect",
            "contato@jobconnect.com", "(11) 3000-0000", "https://linkedin.com/company/jobconnect",
            "01310100", "Av. Paulista", "1000", "Bela Vista", "Sao Paulo", "SP",
            @"O JobConnect e a plataforma mais inteligente para conectar talentos as oportunidades certas. Nossa missao e transformar o processo de recrutamento e selecao, tornando-o mais humano, eficiente e baseado em dados.

Com recursos como匹配 inteligente de candidatos, pipeline Kanban e analytics avançados, ajudamos empresas a encontrar os melhores profissionais e candidatos a conquistar a vaga dos sonhos.

Somos uma equipe jovem e inovadora, comprometida em revolucionar o mercado de recrutamento no Brasil.",
            jobConnect.Manager, jobConnect.Recruiter));

        return companies;
    }

    private static async Task EnsureCandidateProfileAsync(
        JobConnectDbContext context,
        ApplicationUser candidate,
        IReadOnlyDictionary<string, Habilidade> skills)
    {
        if (await context.PerfisCandidatos.AnyAsync(p => p.UserId == candidate.Id)) return;

        context.PerfisCandidatos.Add(new PerfilCandidato
        {
            UserId = candidate.Id, FullName = candidate.FullName, Cpf = "12345678901",
            BirthDate = new DateOnly(1998, 5, 27), PhoneNumber = "(63) 99999-2026",
            LinkedInUrl = "https://linkedin.com/in/filipe-batista",
            PortfolioUrl = "https://github.com/filipebatista",
            Resume = new Curriculo
            {
                Summary = "Desenvolvedor fullstack com foco em .NET, SQL Server e interfaces web responsivas.",
                Educations =
                {
                    new Formacao { Institution = "Faculdade Anhanguera", Course = "Analise e Desenvolvimento de Sistemas", Degree = "Tecnologo", StartDate = new DateOnly(2024, 2, 1) }
                },
                WorkExperiences =
                {
                    new ExperienciaProfissional { CompanyName = "AgileMind", Position = "Desenvolvedor Junior", Description = "Construcao de APIs REST, telas responsivas e integracoes.", StartDate = new DateOnly(2025, 1, 1), IsCurrentJob = true }
                },
                Skills =
                {
                    new CurriculoHabilidade { Skill = skills["C#"], ProficiencyLevel = 4 },
                    new CurriculoHabilidade { Skill = skills[".NET"], ProficiencyLevel = 4 },
                    new CurriculoHabilidade { Skill = skills["SQL Server"], ProficiencyLevel = 3 }
                }
            }
        });
    }

    private static async Task EnsureStagesAsync(JobConnectDbContext context, Empresa company)
    {
        if (await context.EtapasSelecao.AnyAsync(s => s.CompanyId == company.Id)) return;

        context.EtapasSelecao.AddRange(
            new EtapaSelecao { Company = company, Name = "Inscricao Recebida", Order = 1, IsDefaultInitialStage = true },
            new EtapaSelecao { Company = company, Name = "Triagem", Order = 2 },
            new EtapaSelecao { Company = company, Name = "Entrevista Tecnica", Order = 3 },
            new EtapaSelecao { Company = company, Name = "Feedback Final", Order = 4 });
    }

    private static async Task EnsureJobsAsync(
        JobConnectDbContext context,
        IReadOnlyList<Empresa> companies,
        ApplicationUser recruiter,
        IReadOnlyDictionary<string, Habilidade> skills)
    {
        var existingJobs = await context.Vagas.ToListAsync();

        Vaga FindOrCreate(string title, Empresa company) {
            var match = existingJobs.FirstOrDefault(j => j.Title == title && j.CompanyId == company.Id);
            if (match is not null) return match;
            var job = new Vaga { Company = company, CreatedByUserId = recruiter.Id, Title = title };
            existingJobs.Add(job);
            context.Vagas.Add(job);
            return job;
        }

        var agileMind = companies[0];
        var cloudForce = companies[1];
        var dataMind = companies[2];
        var inovaTech = companies[3];

        // ═══════════════════════════════════════════
        // COMPANY 1: AGILEMIND (Sao Paulo - SP)
        // ═══════════════════════════════════════════
        var agileMindDesc = @"A AgileMind e uma consultoria brasileira especializada em metodologias ageis e desenvolvimento de software sob medida. Ha mais de 6 anos no mercado, ajudamos empresas a transformar suas entregas por meio de praticas ageis, produtos digitais inovadores e equipes de alta performance.

Nosso time e composto por mais de 150 profissionais apaixonados por tecnologia e inovacao, distribuidos em 4 estados brasileiros. Atendemos clientes de diversos segmentos, do varejo a saude, da educacao a financas.

Valorizamos a diversidade, o aprendizado continuo e a colaboracao entre areas. Aqui, voce encontrara um ambiente descontraido, com autonomia para criar e espaco para crescer.

Acreditamos que pessoas motivadas e times multidisciplinares sao a chave para entregar resultados excepcionais para nossos clientes.";

        var agileMindBenefits = @"Vale Refeicao/Alimentacao
Vale Transporte
Assistencia Medica
Assistencia Odontologica
Totalpass (Wellhub)
Day off de aniversario
Horario flexivel
Auxilio Home Office";

        {
            var job = FindOrCreate("Desenvolvedor Fullstack .NET", agileMind);
            job.CompanyDescription = agileMindDesc;
            job.Description = @"Buscamos uma pessoa apaixonada por tecnologia para fazer parte do nosso time de desenvolvimento. Voce atuara em squads multidisciplinares construindo solucoes que impactam diretamente a experiencia dos nossos clientes.

Trabalhamos com projetos de consultoria em desenvolvimento de sistemas sob medida, utilizando .NET, React e arquiteturas modernas. Aqui voce encontrara um ambiente que estimula a inovacao, com liberdade para experimentar novas tecnologias e contribuir com ideias que fazem a diferenca.";
            job.Responsibilities = @"📝 Como sera seu dia a dia:

Desenvolver e manter APIs REST em ASP.NET Core
Construir interfaces web responsivas com React e TypeScript
Participar de reunioes de planejamento e refinamento com o time de produto
Realizar code review e contribuir para a qualidade do codigo
Escrever testes unitarios e de integracao
Auxiliar na documentacao tecnica das funcionalidades
Colaborar com o time de UX para garantir a melhor experiencia do usuario";
            job.Requirements = @"🎯 E para tirar de letra, voce precisara:

Superior completo (ou cursando) em Ciencia da Computacao, Analise de Sistemas ou areas afins
Conhecimento em C# e .NET (Core ou Framework)
Experiencia com bancos de dados relacionais (SQL Server)
Conhecimento em Git e versionamento de codigo
Familiaridade com APIs REST e padrao MVC
Disposicao para aprender React no dia a dia
Habilidade de comunicacao e trabalho em equipe

🚀 E se tiver isso tambem, melhor ainda:
Experiencia com Entity Framework Core
Conhecimento em Azure ou nuvem
Vivencia com metodologias ageis (Scrum)";
            job.Benefits = agileMindBenefits;
            job.Location = "Sao Paulo - SP (Escritorio no Itaim Bibi, modelo Hibrido)";
            job.Schedule = "Segunda a sexta, 08:00 as 17:00 (horario flexivel)";
            job.MinimumSalary = 4200; job.MaximumSalary = 6800;
            job.WorkModel = WorkModel.Hybrid; job.Level = JobLevel.Junior;
            job.OpenPositions = 2; job.Status = JobStatus.Published; job.PublishedAt = DateTime.UtcNow.AddDays(-4);
            job.ClosingDate = DateTime.UtcNow.AddDays(25); job.Tags = "C#, .NET, SQL Server, Fullstack";
            job.Skills.Add(new VagaHabilidade { Skill = skills["C#"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills[".NET"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["SQL Server"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Azure"], RequirementType = SkillRequirementType.Differential });
        }

        {
            var job = FindOrCreate("Product Owner", agileMind);
            job.CompanyDescription = agileMindDesc;
            job.Description = @"Buscamos uma pessoa estrategica e comunicativa para atuar como Product Owner em nossos projetos de consultoria. Voce sera a ponte entre o cliente e o time de desenvolvimento, garantindo que o produto entregue valor real para o negocio.

Em nossos projetos, voce tera autonomia para definir prioridades, validar hipoteses e acompanhar metricas de sucesso junto com squads ageis e multidisciplinares.

Se voce gosta de resolver problemas complexos, dialogar com diferentes areas e ver o impacto do seu trabalho, essa posicao e para voce.";
            job.Responsibilities = @"📝 Como sera seu dia a dia:

Definir e priorizar o backlog do produto junto com stakeholders
Escrever historias de usuario com criterios de aceitacao claros
Conduzir reunioes de refinamento, planning e review com o squad
Validar entregas e garantir que atendem aos requisitos de negocio
Acompanhar metricas do produto e propor melhorias continuas
Realizar pesquisas com usuarios para identificar dores e oportunidades
Comunicar roadmap e progresso para a lideranca";
            job.Requirements = @"🎯 E para tirar de letra, voce precisara:

Experiencia como Product Owner ou Analista de Produto em produtos digitais
Conhecimento em metodologias ageis (Scrum, Kanban)
Capacidade de tomar decisoes baseadas em dados e metricas
Excelente comunicacao e negociacao com diferentes stakeholders
Ferramentas: Azure DevOps, Jira, ou similares

🚀 E se tiver isso tambem, melhor ainda:
Conhecimento do mercado de consultoria ou desenvolvimento de software
Experiencia com descoberta de produto (Product Discovery)
Certificacao PO (CSPO, PSPO)";
            job.Benefits = agileMindBenefits + @"
Bonus anual por resultados
Participacao em eventos de produto e inovacao";
            job.Location = "Remoto";
            job.Schedule = "Segunda a sexta, horario flexivel";
            job.MinimumSalary = 10000; job.MaximumSalary = 16000;
            job.WorkModel = WorkModel.Remote; job.Level = JobLevel.Senior;
            job.OpenPositions = 1; job.Status = JobStatus.Published; job.PublishedAt = DateTime.UtcNow.AddDays(-7);
            job.ClosingDate = DateTime.UtcNow.AddDays(25); job.Tags = "Produto, Backlog, Stakeholder, SCRUM";
            job.Skills.Add(new VagaHabilidade { Skill = skills["Scrum"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["UX"], RequirementType = SkillRequirementType.Differential });
        }

        {
            var job = FindOrCreate("Analista de Qualidade (QA)", agileMind);
            job.CompanyDescription = agileMindDesc;
            job.Description = @"Estamos procurando uma pessoa analista de qualidade para garantir a excelencia das entregas dos nossos projetos de consultoria. Voce atuara na definicao de estrategias de teste, automacao e controle de qualidade em squads ageis.

Trabalhamos com projetos variados para diferentes clientes, e sua atuacao sera fundamental para assegurar que cada entrega atenda aos mais altos padroes de qualidade antes de chegar ao usuario final.

Se voce tem olhar critico, gosta de quebrar sistemas e proposital e tem paixao por qualidade de software, essa vaga e para voce.";
            job.Responsibilities = @"📝 Como sera seu dia a dia:

Planejar e executar testes funcionais, de integracao e regressao
Criar e manter scripts de automacao de testes
Participar de refinamentos e revisoes com foco em qualidade
Registrar e acompanhar bugs utilizando ferramentas como Jira
Trabalhar junto com desenvolvedores para prevenir defeitos
Propor melhorias nos processos de qualidade do time
Criar documentacao de cenario de testes e evidencias";
            job.Requirements = @"🎯 E para tirar de letra, voce precisara:

Experiencia com testes de software (funcionais, exploratorios, regressao)
Conhecimento em automacao de testes (Selenium, Cypress ou similar)
Familiaridade com metodologias ageis (Scrum)
Capacidade analitica e atencao aos detalhes
Boa comunicacao para reportar evidencias e resultados

🚀 E se tiver isso tambem, melhor ainda:
Conhecimento em C# e .NET para automacao de testes
Experiencia com testes de API (Postman, RestSharp)
Conhecimento em SQL para validacao de dados em testes";
            job.Benefits = agileMindBenefits;
            job.Location = "Sao Paulo - SP (Hibrido - escritorio no Itaim Bibi)";
            job.Schedule = "Segunda a sexta, 08:00 as 17:00 (horario flexivel)";
            job.MinimumSalary = 5000; job.MaximumSalary = 8000;
            job.WorkModel = WorkModel.Hybrid; job.Level = JobLevel.Mid;
            job.OpenPositions = 1; job.Status = JobStatus.Published; job.PublishedAt = DateTime.UtcNow.AddDays(-3);
            job.ClosingDate = DateTime.UtcNow.AddDays(28); job.Tags = "QA, Testes, Automacao, Qualidade";
            job.Skills.Add(new VagaHabilidade { Skill = skills["C#"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills[".NET"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["SQL Server"], RequirementType = SkillRequirementType.Differential });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Scrum"], RequirementType = SkillRequirementType.Required });
        }

        {
            var job = FindOrCreate("Scrum Master", agileMind);
            job.CompanyDescription = agileMindDesc;
            job.Description = @"Buscamos uma pessoa Scrum Master para facilitar e evoluir as praticas ageis nos times de consultoria da AgileMind. Voce sera responsavel por garantir que os squads estejam funcionando de forma saudavel, produtiva e alinhada com os principios ageis.

Atuando como facilitador e mentor, voce ajudara os times a remover impedimentos, melhorar continuamente e entregar valor de forma consistente para os clientes.

Se voce acredita no poder das metodologias ageis e tem habilidades de facilitacao e lideranca servil, essa posicao e para voce.";
            job.Responsibilities = @"📝 Como sera seu dia a dia:

Facilitar as cerimonias ageis (daily, planning, review, retrospectiva)
Auxiliar os times na remocao de impedimentos
Mentorar product owners e desenvolvedores em praticas ageis
Promover a melhoria continua atraves de retrospectivas acionaveis
Acompanhar metricas do time (velocidade, lead time, burndown)
Propor ferramentas e praticas para aumentar a produtividade do squad
Garantir que os principios do Scrum sejam seguidos e adaptados conforme necessario";
            job.Requirements = @"🎯 E para tirar de letra, voce precisara:

Experiencia como Scrum Master em times ageis
Conhecimento profundo do framework Scrum e metodologias ageis
Certificacao Scrum Master (CSM, PSM ou similar)
Habilidade de facilitacao e comunicacao
Capacidade de resolver conflitos e promover colaboracao
Experiencia com ferramentas ageis (Jira, Trello, Azure DevOps)

🚀 E se tiver isso tambem, melhor ainda:
Experiencia em consultoria ou atendimento a clientes externos
Conhecimento em Kanban e Lean
Formacao em Psicologia, Administracao ou areas correlatas";
            job.Benefits = agileMindBenefits + @"
Bonus por certificacoes
Verba anual para cursos e eventos ageis";
            job.Location = "Remoto";
            job.Schedule = "Segunda a sexta, horario flexivel";
            job.MinimumSalary = 8000; job.MaximumSalary = 12000;
            job.WorkModel = WorkModel.Remote; job.Level = JobLevel.Mid;
            job.OpenPositions = 1; job.Status = JobStatus.Published; job.PublishedAt = DateTime.UtcNow.AddDays(-5);
            job.ClosingDate = DateTime.UtcNow.AddDays(30); job.Tags = "Scrum, Agile, Facilitacao, Metodologias Ageis";
            job.Skills.Add(new VagaHabilidade { Skill = skills["Scrum"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Jira"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Confluence"], RequirementType = SkillRequirementType.Differential });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Azure"], RequirementType = SkillRequirementType.Differential });
        }

        // ═══════════════════════════════════════════
        // COMPANY 2: CLOUDFORCE (Belo Horizonte, MG)
        // ═══════════════════════════════════════════
        var cloudForceDesc = @"A CloudForce e uma empresa de tecnologia especializada em infraestrutura em nuvem e DevOps. Ha mais de 8 anos no mercado, ajudamos empresas a modernizar suas operacoes de TI por meio de arquiteturas cloud native, automacao e boas praticas de engenharia de infraestrutura.

Nosso time e composto por mais de 80 profissionais apaixonados por tecnologia, incluindo engenheiros de nuvem, DevOps, arquitetos e especialistas em seguranca. Atendemos clientes de medio e grande porte em todo o Brasil.

Valorizamos o aprendizado continuo, a autonomia e a colaboracao. Aqui, voce encontrara projetos desafiadores, liberdade para experimentar novas tecnologias e um ambiente que incentiva a inovacao.

Acreditamos que a infraestrutura como codigo e a automacao sao o caminho para construir sistemas mais robustos, escalaveis e seguros.";

        var cloudForceBenefits = @"Vale Refeicao/Alimentacao
Vale Transporte
Assistencia Medica e Odontologica
Seguro de Vida
Auxilio Home Office
Bonus por desempenho
Horario flexivel
Day off de aniversario";

        {
            var job = FindOrCreate("DevOps Engineer", cloudForce);
            job.CompanyDescription = cloudForceDesc;
            job.Description = @"Estamos montando um time de plataforma para elevar a maturidade DevOps dos nossos clientes. Precisamos de uma pessoa que ajude a construir e manter a infraestrutura que suporta sistemas criticos em ambientes de nuvem.

Voce sera responsavel por automatizar processos, garantir a disponibilidade dos ambientes e implementar as melhores praticas de CI/CD, seguranca e observabilidade.

Trabalhamos com AWS e Azure como provedores principais, Docker e Kubernetes para orquestracao, e Terraform para infraestrutura como codigo.";
            job.Responsibilities = @"📝 Como sera seu dia a dia:

Automatizar pipelines de CI/CD com Azure DevOps e Jenkins
Gerenciar ambientes em nuvem (AWS, Azure)
Configurar e manter ferramentas de monitoramento e observabilidade
Implementar infraestrutura como codigo (Terraform)
Garantir a seguranca dos ambientes e processos de deploy
Documentar procedimentos e arquiteturas de infraestrutura
Participar de planteos de suporte a incidentes";
            job.Requirements = @"🎯 E para tirar de letra, voce precisara:

Experiencia com AWS ou Azure em ambientes de producao
Conhecimento em Docker e orquestracao (Kubernetes)
Experiencia com pipelines CI/CD (Azure DevOps, Jenkins ou similar)
Infraestrutura como codigo (Terraform)
Conhecimento em Linux e scripts shell
Familiaridade com Git e GitFlow

🚀 E se tiver isso tambem, melhor ainda:
Certificacao AWS (SA Associate) ou Azure (AZ-104)
Conhecimento em monitoramento (Prometheus, Grafana, ELK)
Experiencia com banco de dados em producao";
            job.Benefits = cloudForceBenefits + @"
Auxilio Home Office
Bonus por disponibilidade";
            job.Location = "Belo Horizonte - MG (Hibrido - escritorio na Savassi)";
            job.Schedule = "Segunda a sexta, horario flexivel (com plantao eventual)";
            job.MinimumSalary = 8000; job.MaximumSalary = 13000;
            job.WorkModel = WorkModel.Hybrid; job.Level = JobLevel.Mid;
            job.OpenPositions = 2; job.Status = JobStatus.Published; job.PublishedAt = DateTime.UtcNow.AddDays(-5);
            job.ClosingDate = DateTime.UtcNow.AddDays(30); job.Tags = "DevOps, Docker, AWS, CI/CD";
            job.Skills.Add(new VagaHabilidade { Skill = skills["Docker"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["AWS"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Azure"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Terraform"], RequirementType = SkillRequirementType.Differential });
        }

        {
            var job = FindOrCreate("Analista de Infraestrutura Senior", cloudForce);
            job.CompanyDescription = cloudForceDesc;
            job.Description = @"Estamos fortalecendo nossa area de infraestrutura e precisamos de um profissional experiente para liderar a operacao dos ambientes que suportam nossos clientes.

Voce sera responsavel por projetar, implementar e manter a infraestrutura de TI, garantindo disponibilidade, performance e seguranca.

Trabalhamos com ambientes on-premises e em nuvem (AWS e Azure), e buscamos alguem que tenha visao estrategica de infraestrutura e capacidade de liderar projetos de modernizacao.";
            job.Responsibilities = @"📝 Como sera seu dia a dia:

Projetar e implementar solucoes de infraestrutura para clientes
Administrar servidores Windows Server e Linux (Ubuntu, CentOS)
Gerenciar ambientes em nuvem (AWS, Azure)
Implementar e manter politicas de backup e recuperacao de desastres
Monitorar a saude dos sistemas e propor melhorias
Liderar projetos de migracao e modernizacao de infraestrutura
Mentorar analistas de infraestrutura mais juniores
Documentar topologias, procedimentos e politicas";
            job.Requirements = @"🎯 E para tirar de letra, voce precisara:

Experiencia comprovada em administracao de servidores Windows e Linux
Conhecimento avancado em redes (TCP/IP, DNS, VPN, firewall)
Experiencia com nuvem publica (AWS ou Azure)
Conhecimento em virtualizacao (VMware, Hyper-V)
Habilidade com scripting (PowerShell, Bash)
Experiencia em projetos de backup e DR (Veeam, Bacula ou similares)
Capacidade de lideranca e gestao de projetos

🚀 E se tiver isso tambem, melhor ainda:
Certificacoes AWS, Azure ou VMware
Conhecimento em Docker e Kubernetes
Experiencia com monitoramento (Zabbix, Nagios, PRTG)";
            job.Benefits = cloudForceBenefits + @"
Auxilio Home Office premium
Bonus por disponibilidade
Participacao em resultados (PLR)";
            job.Location = "Belo Horizonte - MG (Presencial - escritorio na Savassi)";
            job.Schedule = "Segunda a sexta, horario flexivel (com plantao eventual)";
            job.MinimumSalary = 6500; job.MaximumSalary = 10000;
            job.WorkModel = WorkModel.OnSite; job.Level = JobLevel.Senior;
            job.OpenPositions = 1; job.Status = JobStatus.Published; job.PublishedAt = DateTime.UtcNow.AddDays(-8);
            job.ClosingDate = DateTime.UtcNow.AddDays(28); job.Tags = "Infraestrutura, Cloud, Redes, Lideranca";
            job.Skills.Add(new VagaHabilidade { Skill = skills["AWS"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Azure"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Docker"], RequirementType = SkillRequirementType.Differential });
        }

        {
            var job = FindOrCreate("Desenvolvedor Backend Java", cloudForce);
            job.CompanyDescription = cloudForceDesc;
            job.Description = @"Estamos expandindo nossa equipe de desenvolvimento e precisamos de um desenvolvedor backend Java para construir e manter APIs e integracoes para nossos clientes.

Voce atuara no desenvolvimento de microservicos, integracoes com sistemas de terceros e na evolucao de plataformas de gestao empresarial.

Trabalhamos com Java 17, Spring Boot, PostgreSQL e arquitetura de microservicos. Se voce gosta de resolver problemas complexos e tem paixao por codigo bem escrito, essa vaga e para voce.";
            job.Responsibilities = @"📝 Como sera seu dia a dia:

Desenvolver e manter APIs REST em Java com Spring Boot
Implementar integracoes com sistemas de clientes e parceiros
Realizar manutencao evolutiva e corretiva nos modulos existentes
Escrever testes unitarios e de integracao
Participar de code reviews e contribuir para a qualidade do codigo
Documentar APIs e fluxos dos sistemas
Auxiliar na estimativa e planejamento das entregas";
            job.Requirements = @"🎯 E para tirar de letra, voce precisara:

Experiencia em desenvolvimento Java (pelo menos 2 anos)
Conhecimento em Spring Boot e Spring Data JPA
Experiencia com bancos relacionais (PostgreSQL ou MySQL)
Conhecimento em Git e versionamento de codigo
Familiaridade com APIs REST e formatos JSON/XML
Capacidade de resolver problemas de forma autonoma

🚀 E se tiver isso tambem, melhor ainda:
Conhecimento em Docker e conteinerizacao
Experiencia com filas (RabbitMQ ou Kafka)
Conhecimento em nuvem (AWS ou Azure)";
            job.Benefits = cloudForceBenefits;
            job.Location = "Belo Horizonte - MG (Hibrido - 2x por semana no escritorio)";
            job.Schedule = "Segunda a sexta, 08:00 as 17:00 (horario flexivel)";
            job.MinimumSalary = 5000; job.MaximumSalary = 8000;
            job.WorkModel = WorkModel.Hybrid; job.Level = JobLevel.Mid;
            job.OpenPositions = 2; job.Status = JobStatus.Published; job.PublishedAt = DateTime.UtcNow.AddDays(-6);
            job.ClosingDate = DateTime.UtcNow.AddDays(30); job.Tags = "Java, Spring Boot, APIs, PostgreSQL";
            job.Skills.Add(new VagaHabilidade { Skill = skills["Java"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Spring Boot"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["PostgreSQL"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Docker"], RequirementType = SkillRequirementType.Differential });
        }

        {
            var job = FindOrCreate("Arquiteto de Nuvem (Cloud)", cloudForce);
            job.CompanyDescription = cloudForceDesc;
            job.Description = @"Buscamos uma pessoa arquiteta de nuvem para liderar a estrategia cloud dos nossos clientes. Voce sera responsavel por desenhar arquiteturas escalaveis, seguras e resilientes em provedores de nuvem publica.

Como arquiteto, voce atuara na definicao de melhores praticas, escolha de servicos, estimativas de custo e migracao de workloads on-premises para a nuvem.

Se voce tem visao holistica de infraestrutura, experiencia em projetos de grande porte e paixao por arquiteturas cloud native, essa posicao e para voce.";
            job.Responsibilities = @"📝 Como sera seu dia a dia:

Desenhar arquiteturas de solucao em AWS e Azure para clientes
Realizar avaliacoes de ambiente e propor planos de migracao para nuvem
Criar estimativas de custo e otimizacao de recursos cloud
Definir estrategias de seguranca, rede e identidade na nuvem
Liderar provas de conceito (PoC) de novas tecnologias
Documentar arquiteturas de referencia e melhores praticas
Participar de reunioes com clientes para apresentacao de propostas tecnicas
Mentorar engenheiros de infraestrutura e DevOps";
            job.Requirements = @"🎯 E para tirar de letra, voce precisara:

Experiencia comprovada como arquiteto de nuvem ou engenheiro cloud senior
Conhecimento aprofundado em AWS e Azure (servicos de computacao, rede, armazenamento, banco de dados)
Experiencia com Docker, Kubernetes e arquitetura de microservicos
Infraestrutura como codigo (Terraform, CloudFormation ou Bicep)
Capacidade de desenhar arquiteturas altamente disponiveis e resilientes
Comunicacao clara para apresentar propostas tecnicas

🚀 E se tiver isso tambem, melhor ainda:
Certificacoes AWS Solutions Architect Professional ou Azure Solutions Architect Expert
Conhecimento em GCP
Experiencia com migracoes de grande porte (lift-and-shift, replatforming, rearchitecting)";
            job.Benefits = cloudForceBenefits + @"
Bonus anual generoso
PLR (Participacao nos Lucros e Resultados)
Auxilio Home Office premium
Verba anual para conferencias e certificacoes";
            job.Location = "Belo Horizonte - MG (Hibrido - escritorio na Savassi)";
            job.Schedule = "Segunda a sexta, horario flexivel";
            job.MinimumSalary = 12000; job.MaximumSalary = 18000;
            job.WorkModel = WorkModel.Hybrid; job.Level = JobLevel.Senior;
            job.OpenPositions = 1; job.Status = JobStatus.Published; job.PublishedAt = DateTime.UtcNow.AddDays(-4);
            job.ClosingDate = DateTime.UtcNow.AddDays(28); job.Tags = "Cloud, AWS, Azure, Arquitetura, Kubernetes";
            job.Skills.Add(new VagaHabilidade { Skill = skills["AWS"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Azure"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Docker"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Kubernetes"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Terraform"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["GCP"], RequirementType = SkillRequirementType.Differential });
        }

        // ═══════════════════════════════════════════
        // COMPANY 3: DATAMIND (Curitiba, PR)
        // ═══════════════════════════════════════════
        var dataMindDesc = @"A DataMind e uma empresa de inteligencia de dados com sede em Curitiba, Parana. Somos especialistas em transformar dados brutos em decisoes estrategicas para empresas dos setores financeiro, varejo e saude.

Ha mais de 8 anos no mercado, nossa equipe de 70 profissionais inclui cientistas de dados, engenheiros, analistas e especialistas em machine learning. Trabalhamos com tecnologia de ponta: Python, Azure ML, SQL Server, Power BI e ferramentas de big data.

Acreditamos que os dados sao o ativo mais valioso das empresas, e nossa missao e ajudar organizacoes a extrair o maximo valor de suas informacoes. Valorizamos o rigor tecnico, a curiosidade intelectual e o trabalho em equipe.

Oferecemos um ambiente estimulante, com projetos desafiadores, incentivo a publicacao de artigos e participacao em conferencias nacionais e internacionais.";

        var dataMindBenefits = @"Vale Refeicao/Alimentacao
Vale Transporte
Assistencia Medica e Odontologica
Seguro de Vida
Totalpass (Wellhub)
Day off de aniversario
Horario flexivel
Auxilio Home Office
PLR (Participacao nos Lucros e Resultados)
Verba anual para cursos e conferencias";

        {
            var job = FindOrCreate("Cientista de Dados Pleno", dataMind);
            job.CompanyDescription = dataMindDesc;
            job.Description = @"Buscamos uma pessoa analitica e criativa para extrair insights dos dados de nossos clientes e construir modelos preditivos que geram valor real para os negocios. Voce atuara em projetos de ponta a ponta: desde a compreensao do problema de negocio ate a entrega e monitoramento de modelos em producao.

Nossos projetos envolvem analise de grandes volumes de dados, desenvolvimento de algoritmos de machine learning, criacao de dashboards interativos e comunicacao de resultados para stakeholders.

Se voce e apaixonado por dados, tem pensamento critico e gosta de transformar numeros em historias, essa posicao e para voce.";
            job.Responsibilities = @"📝 Como sera seu dia a dia:

Realizar analise exploratoria de dados para gerar hipoteses de negocio
Desenvolver modelos preditivos e de recomendacao (classificacao, regressao, clusterizacao)
Criar dashboards e relatorios para comunicar insights ao negocio
Projetar e analisar experimentos com metricas de impacto
Trabalhar junto com engenharia para colocar modelos em producao
Apresentar resultados para stakeholders tecnicos e nao tecnicos
Documentar experimentos, metricas e decisoes tecnicas";
            job.Requirements = @"🎯 E para tirar de letra, voce precisara:

Formacao em Estatistica, Ciencia da Computacao, Matematica ou areas afins
Experiencia com Python e bibliotecas de dados (pandas, numpy, scikit-learn)
Conhecimento em SQL para extracao e manipulacao de dados
Experiencia com visualizacao de dados (Power BI, matplotlib, ou similar)
Conhecimento em estatistica descritiva e inferencial
Boa comunicacao e habilidades de storytelling com dados

🚀 E se tiver isso tambem, melhor ainda:
Experiencia com Azure ML Services
Conhecimento em Deep Learning (TensorFlow, PyTorch)
Publicacao de artigos ou participacao em kaggle";
            job.Benefits = dataMindBenefits;
            job.Location = "Curitiba - PR (Hibrido - 2x por semana no escritorio do Bigorrilho)";
            job.Schedule = "Segunda a sexta, horario flexivel";
            job.MinimumSalary = 8000; job.MaximumSalary = 13000;
            job.WorkModel = WorkModel.Hybrid; job.Level = JobLevel.Mid;
            job.OpenPositions = 2; job.Status = JobStatus.Published; job.PublishedAt = DateTime.UtcNow.AddDays(-6);
            job.ClosingDate = DateTime.UtcNow.AddDays(30); job.Tags = "Dados, Python, Machine Learning, Analytics";
            job.Skills.Add(new VagaHabilidade { Skill = skills["Python"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["SQL Server"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Machine Learning"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Power BI"], RequirementType = SkillRequirementType.Differential });
        }

        {
            var job = FindOrCreate("Analista de BI Senior", dataMind);
            job.CompanyDescription = dataMindDesc;
            job.Description = @"Buscamos um analista de BI senior para transformar dados em decisoes estrategicas para nossos clientes. Voce sera responsavel por criar e manter dashboards, relatorios e analises que guiam as decisoes de negocios dos nossos clientes, desde metricas operacionais ate indicadores de performance.

Seu trabalho sera essencial para que os tomadores de decisao tenham visibilidade clara e acionavel do desempenho dos negocios. Trabalhamos principalmente com Power BI, SQL Server e Azure Analysis Services.";
            job.Responsibilities = @"📝 Como sera seu dia a dia:

Criar dashboards interativos no Power BI com DAX e medidas complexas
Desenvolver consultas SQL para extracao e tratamento de dados
Modelar dados para otimizar performance dos relatorios
Realizar analises de metricas de negocio e gerar insights
Automatizar relatorios recorrentes para as areas de negocios
Apresentar resultados e recomendacoes para stakeholders
Mentorar analistas de BI mais juniores";
            job.Requirements = @"🎯 E para tirar de letra, voce precisara:

Experiencia comprovada com Power BI (DAX, modelagem, visualizacao)
Conhecimento avancado em SQL (joins, subqueries, CTEs, procedures)
Capacidade de transformar dados brutos em insights de negocio
Experiencia com modelagem dimensional (star schema, snowflake)
Boa comunicacao e apresentacao de resultados

🚀 E se tiver isso tambem, melhor ainda:
Conhecimento em Azure Analysis Services ou SSAS
Experiencia com Python para analise de dados
Conhecimento em ferramentas de ETL (SSIS, Azure Data Factory)";
            job.Benefits = dataMindBenefits + @"
Bonus anual por projetos entregues
Participacao em eventos de BI e dados";
            job.Location = "Curitiba - PR (Hibrido - escritorio no Bigorrilho)";
            job.Schedule = "Segunda a sexta, horario flexivel";
            job.MinimumSalary = 6500; job.MaximumSalary = 9500;
            job.WorkModel = WorkModel.Hybrid; job.Level = JobLevel.Senior;
            job.OpenPositions = 1; job.Status = JobStatus.Published; job.PublishedAt = DateTime.UtcNow.AddDays(-7);
            job.ClosingDate = DateTime.UtcNow.AddDays(28); job.Tags = "BI, Power BI, SQL, Dashboards";
            job.Skills.Add(new VagaHabilidade { Skill = skills["Power BI"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["SQL Server"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Azure"], RequirementType = SkillRequirementType.Differential });
        }

        {
            var job = FindOrCreate("Engenheiro de Dados", dataMind);
            job.CompanyDescription = dataMindDesc;
            job.Description = @"Queremos um engenheiro de dados para estruturar e escalar a camada de dados da DataMind. Voce atuara na construcao de pipelines de ingestao, transformacao e disponibilizacao de dados para times de produto, negocios e ciencia de dados.

Nossas bases processam terabytes de dados por mes de fontes variadas: bancos relacionais, APIs, arquivos e streams. Precisamos de alguem para transformar esse volume em informacao estruturada, confiavel e acessivel.";
            job.Responsibilities = @"📝 Como sera seu dia a dia:

Construir e manter pipelines ETL em Azure Data Factory e Python
Modelar tabelas e views em SQL Server e PostgreSQL
Criar scripts de ingestao de dados de fontes externas (APIs, arquivos)
Monitorar a qualidade e consistencia dos dados
Documentar linhagem e dicionario de dados
Apoiar cientistas e analistas no acesso as bases
Otimizar performance de queries e processos batch";
            job.Requirements = @"🎯 E para tirar de letra, voce precisara:

Formacao em Ciencia da Computacao, Engenharia, Estatistica ou areas afins
Conhecimento avancado em SQL (joins, aggregations, window functions)
Experiencia com Python para pipelines de dados
Familiaridade com conceitos de ETL e modelagem dimensional
Conhecimento em nuvem (Azure ou AWS)

🚀 E se tiver isso tambem, melhor ainda:
Experiencia com Spark ou Databricks
Conhecimento em MongoDB ou bancos NoSQL
Vivencia com ferramentas de mensageria (Kafka, RabbitMQ)";
            job.Benefits = dataMindBenefits;
            job.Location = "Curitiba - PR (Hibrido - escritorio no Bigorrilho)";
            job.Schedule = "Segunda a sexta, 08:00 as 17:00";
            job.MinimumSalary = 7000; job.MaximumSalary = 11000;
            job.WorkModel = WorkModel.Hybrid; job.Level = JobLevel.Mid;
            job.OpenPositions = 1; job.Status = JobStatus.Published; job.PublishedAt = DateTime.UtcNow.AddDays(-4);
            job.ClosingDate = DateTime.UtcNow.AddDays(32); job.Tags = "Dados, ETL, Azure, SQL";
            job.Skills.Add(new VagaHabilidade { Skill = skills["Python"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["SQL Server"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Azure"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["MongoDB"], RequirementType = SkillRequirementType.Differential });
        }

        {
            var job = FindOrCreate("Analista de Marketing Digital", dataMind);
            job.CompanyDescription = dataMindDesc;
            job.Description = @"Buscamos um analista de marketing digital para planejar, executar e otimizar campanhas de performance para os canais da DataMind. Voce atuara com gestao de trafego pago, analise de metricas e otimizacao de campanhas em Google Ads, Meta Ads e outras plataformas.

Nosso time de marketing e responsavel por gerir o orcamento de aquisicao de leads e promover a marca DataMind no mercado de inteligencia de dados.

Se voce e apaixonado por dados, criatividade e resultados, essa posicao e para voce.";
            job.Responsibilities = @"📝 Como sera seu dia a dia:

Planejar, criar e otimizar campanhas no Google Ads e Meta Ads
Realizar analise de metricas e gerar relatorios de performance
Propor testes A/B e novas estrategias de segmentacao
Acompanhar o funil de conversao e identificar oportunidades de melhoria
Gerenciar orcamentos de midia e garantir o melhor ROI
Participar de reunioes de alinhamento estrategico com o time de vendas
Manter-se atualizado sobre tendencias e novidades das plataformas";
            job.Requirements = @"🎯 E para tirar de letra, voce precisara:

Experiencia com gestao de trafego pago (Google Ads, Meta Ads)
Conhecimento em analise de metricas e ferramentas de BI
Pensamento analitico e orientacao a resultados
Habilidade com Excel/Google Sheets para analise de dados
Boa comunicacao para apresentacao de resultados

🚀 E se tiver isso tambem, melhor ainda:
Conhecimento em SEO
Experiencia com ferramentas de automacao (RD Station, HubSpot)
Nocoes de HTML e CSS";
            job.Benefits = dataMindBenefits;
            job.Location = "Curitiba - PR (Hibrido - 2x por semana no escritorio do Bigorrilho)";
            job.Schedule = "Segunda a sexta, 08:30 as 17:30 (horario flexivel)";
            job.MinimumSalary = 3000; job.MaximumSalary = 5000;
            job.WorkModel = WorkModel.Hybrid; job.Level = JobLevel.Junior;
            job.OpenPositions = 1; job.Status = JobStatus.Published; job.PublishedAt = DateTime.UtcNow.AddDays(-5);
            job.ClosingDate = DateTime.UtcNow.AddDays(25); job.Tags = "Marketing, Google Ads, Meta Ads, Performance";
            job.Skills.Add(new VagaHabilidade { Skill = skills["Google Ads"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Meta Ads"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["SEO"], RequirementType = SkillRequirementType.Differential });
        }

        // ═══════════════════════════════════════════
        // COMPANY 4: INOVATECH (Rio de Janeiro, RJ)
        // ═══════════════════════════════════════════
        var inovaTechDesc = @"A InovaTech e uma empresa de tecnologia e inovacao com sede no Rio de Janeiro, especializada em solucoes de engenharia, desenvolvimento de software e transformacao digital. Ha mais de 12 anos no mercado, entregamos projetos de alto padrao para clientes dos setores de construcao civil, energia, infraestrutura e servicos.

Nosso time e composto por mais de 100 profissionais, incluindo engenheiros, desenvolvedores, analistas e designers. Utilizamos metodologias modernas de gestao de projetos e tecnologias emergentes para garantir eficiencia, qualidade e inovacao em cada entrega.

Valorizamos a colaboracao, o aprendizado continuo e a sustentabilidade. Acreditamos que a tecnologia pode transformar industrias tradicionais e estamos comprometidos em liderar essa transformacao.

Oferecemos um ambiente de trabalho profissional e acolhedor, com oportunidades reais de crescimento e desenvolvimento profissional continuo.";

        var inovaTechBenefits = @"Vale Transporte
Vale Refeicao/Alimentacao
Assistencia Medica e Odontologica
Seguro de Vida
Day off de aniversario
Horario flexivel
Auxilio creche
Convenio com academias";

        {
            var job = FindOrCreate("Engenheiro Civil", inovaTech);
            job.CompanyDescription = inovaTechDesc;
            job.Description = @"Buscamos um engenheiro civil para integrar nossa equipe de projetos e obras. Voce atuara em todas as fases do empreendimento: desde a concepcao e projetos executivos ate o acompanhamento da execucao e entrega.

Trabalhamos com projetos de medio e grande porte nos segmentos residencial, comercial e industrial. Utilizamos tecnologia BIM, ferramentas de gestao de obras e metodologias modernas de gerenciamento.

Se voce e engenheiro civil com experiencia, tem olhar critico para qualidade e gosta de trabalhar em equipe, essa vaga e para voce.";
            job.Responsibilities = @"📝 Como sera seu dia a dia:

Elaborar e revisar projetos executivos de engenharia (estrutural, hidraulico, eletrico)
Acompanhar a execucao de obras garantindo qualidade e cumprimento de prazos
Realizar vistorias tecnicas e emitir relatorios de acompanhamento
Gerenciar cronogramas, orcamentos e recursos das obras
Coordenar equipes de tecnicos e empreiteiras
Garantir o cumprimento das normas de seguranca (NR-18, NR-35)
Aprovar materiais e servicos conforme especificacoes tecnicas";
            job.Requirements = @"🎯 E para tirar de letra, voce precisara:

Formacao em Engenharia Civil com CREA ativo
Experiencia comprovada em obras residenciais ou comerciais
Conhecimento em AutoCAD e Revit (BIM)
Capacidade de leitura e interpretacao de projetos
Conhecimento em normas tecnicas (ABNT, NBR)
Habilidade de lideranca e gestao de equipes
Excel para orcamentos e controle de obras

🚀 E se tiver isso tambem, melhor ainda:
Pos-graduacao em Engenharia de Producao ou Gestao de Obras
Experiencia com MS Project ou Primavera
Conhecimento em BIM 4D e 5D (cronograma e custos)";
            job.Benefits = inovaTechBenefits + @"
Participacao em resultados por obra entregue no prazo
Auxilio combustivel ou veiculo
Seguro de engenharia";
            job.Location = "Rio de Janeiro - RJ (Escritorio no Centro + visitas a obras no Grande Rio)";
            job.Schedule = "Segunda a sexta, 08:00 as 17:00 (com visitas tecnicas eventuais)";
            job.MinimumSalary = 6000; job.MaximumSalary = 9000;
            job.WorkModel = WorkModel.Hybrid; job.Level = JobLevel.Mid;
            job.OpenPositions = 2; job.Status = JobStatus.Published; job.PublishedAt = DateTime.UtcNow.AddDays(-8);
            job.ClosingDate = DateTime.UtcNow.AddDays(30); job.Tags = "Engenharia, Obras, Projetos, BIM";
            job.Skills.Add(new VagaHabilidade { Skill = skills["AutoCAD"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Revit"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Excel"], RequirementType = SkillRequirementType.Required });
        }

        {
            var job = FindOrCreate("Analista de Suprimentos", inovaTech);
            job.CompanyDescription = inovaTechDesc;
            job.Description = @"Buscamos um analista de suprimentos para gerenciar as compras e o relacionamento com fornecedores da InovaTech. Voce sera responsavel por garantir que materiais, equipamentos e servicos sejam adquiridos com qualidade, no prazo e com o melhor custo-beneficio para cada projeto.

A area de suprimentos e estrategica para o sucesso dos nossos empreendimentos. Uma boa gestao de fornecedores e compras impacta diretamente o cronograma, o orcamento e a qualidade final das entregas.

Se voce tem experiencia em compras, negociacao e gestao de estoques, e quer fazer parte de uma empresa solida e em crescimento, essa vaga e para voce.";
            job.Responsibilities = @"📝 Como sera seu dia a dia:

Realizar cotacoes e negociacoes com fornecedores de materiais e servicos
Gerenciar o processo de compras desde a solicitacao ate a entrega
Manter cadastro de fornecedores atualizado e avaliar performance
Controlar estoques de materiais nos projetos e no almoxarifado central
Acompanhar prazos de entrega e resolver inconformidades
Elaborar relatorios de indicadores de suprimentos (economia, prazo, qualidade)
Participar do planejamento de compras junto a engenharia e obras";
            job.Requirements = @"🎯 E para tirar de letra, voce precisara:

Formacao em Administracao, Engenharia, Logistica ou areas afins
Experiencia em compras e negociacao (de preferencia na construcao civil)
Conhecimento em sistemas ERP (SAP, Oracle, ou similares)
Habilidade em Excel e analise de dados
Capacidade de negociacao e bom relacionamento interpessoal
Organizacao e atencao aos detalhes

🚀 E se tiver isso tambem, melhor ainda:
Conhecimento em materiais de construcao e fornecedores do RJ
Experiencia com gestao de contratos
Conhecimento em Power BI para analise de indicadores";
            job.Benefits = inovaTechBenefits;
            job.Location = "Rio de Janeiro - RJ (Presencial - Escritorio no Centro)";
            job.Schedule = "Segunda a sexta, 08:00 as 17:00";
            job.MinimumSalary = 3500; job.MaximumSalary = 5200;
            job.WorkModel = WorkModel.OnSite; job.Level = JobLevel.Mid;
            job.OpenPositions = 1; job.Status = JobStatus.Published; job.PublishedAt = DateTime.UtcNow.AddDays(-4);
            job.ClosingDate = DateTime.UtcNow.AddDays(30); job.Tags = "Suprimentos, Compras, Negociacao, ERP";
            job.Skills.Add(new VagaHabilidade { Skill = skills["Excel"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["SAP"], RequirementType = SkillRequirementType.Differential });
        }

        {
            var job = FindOrCreate("Desenvolvedor Mobile React Native", inovaTech);
            job.CompanyDescription = inovaTechDesc;
            job.Description = @"Estamos procurando um desenvolvedor mobile para criar e manter aplicativos em React Native para nossos clientes. Voce atuara no desenvolvimento de apps para iOS e Android, desde a prototipacao ate a publicacao nas lojas.

Trabalhamos com projetos de transformacao digital para clientes dos setores de energia, infraestrutura e servicos. Seus apps serao utilizados por centenas de usuarios em todo o Brasil.

Se voce tem experiencia com React Native, gosta de criar interfaces fluidas e se preocupa com a experiencia do usuario, essa vaga e para voce.";
            job.Responsibilities = @"📝 Como sera seu dia a dia:

Desenvolver e manter aplicativos mobile em React Native
Implementar integracoes com APIs REST e GraphQL
Publicar e gerenciar aplicativos na App Store e Google Play
Colaborar com o time de UX para garantir a melhor experiencia do usuario
Escrever testes unitarios e de integracao para os apps
Participar de code reviews e contribuir para a qualidade do codigo
Acompanhar metricas de desempenho e crash reporting";
            job.Requirements = @"🎯 E para tirar de letra, voce precisara:

Experiencia com React Native e TypeScript
Conhecimento em consumo de APIs REST
Familiaridade com Git e versionamento de codigo
Capacidade de publicar aplicativos nas lojas (App Store e Google Play)
Conhecimento em componentes nativos e bridges
Habilidade de resolver problemas de forma autonoma

🚀 E se tiver isso tambem, melhor ainda:
Conhecimento em Node.js para camada de backend
Experiencia com GraphQL
Conhecimento em MongoDB ou Firebase";
            job.Benefits = inovaTechBenefits + @"
Auxilio Home Office
Bonus por entrega de projetos";
            job.Location = "Rio de Janeiro - RJ (Hibrido - 2x por semana no escritorio do Centro)";
            job.Schedule = "Segunda a sexta, 08:00 as 17:00 (horario flexivel)";
            job.MinimumSalary = 6000; job.MaximumSalary = 10000;
            job.WorkModel = WorkModel.Hybrid; job.Level = JobLevel.Mid;
            job.OpenPositions = 1; job.Status = JobStatus.Published; job.PublishedAt = DateTime.UtcNow.AddDays(-3);
            job.ClosingDate = DateTime.UtcNow.AddDays(28); job.Tags = "React Native, Mobile, TypeScript, iOS, Android";
            job.Skills.Add(new VagaHabilidade { Skill = skills["React"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["TypeScript"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Git"], RequirementType = SkillRequirementType.Required });
            job.Skills.Add(new VagaHabilidade { Skill = skills["Node.js"], RequirementType = SkillRequirementType.Differential });
            job.Skills.Add(new VagaHabilidade { Skill = skills["MongoDB"], RequirementType = SkillRequirementType.Differential });
        }

        {
            var job = FindOrCreate("Analista de Suporte Tecnico", inovaTech);
            job.CompanyDescription = inovaTechDesc;
            job.Description = @"Buscamos um analista de suporte tecnico para atuar no atendimento aos usuarios internos e clientes da InovaTech. Voce sera responsavel por prestar suporte remoto e presencial, resolvendo problemas, orientando usuarios e garantindo a continuidade das operacoes.

Trabalhamos com sistemas de gestao empresarial, ferramentas de engenharia e infraestrutura de TI. Sua atuacao sera fundamental para manter a produtividade dos times e a satisfacao dos nossos clientes.

Se voce tem habilidades de comunicacao, gosta de ajudar pessoas e tem interesse por tecnologia, essa vaga e para voce.";
            job.Responsibilities = @"📝 Como sera seu dia a dia:

Realizar suporte remoto e presencial aos usuarios internos e clientes
Atender, analisar e acompanhar chamados via sistema de help desk
Configurar e manter estacoes de trabalho e equipamentos de TI
Auxiliar em duvidas sobre sistemas corporativos e ferramentas
Criar e manter documentacao de procedimentos e solucoes
Participar de projetos de implantacao de novos sistemas
Apoiar a equipe de infraestrutura em demandas diversas";
            job.Requirements = @"🎯 E para tirar de letra, voce precisara:

Experiencia em funcoes de suporte tecnico ou atendimento ao cliente
Conhecimento basico em hardware e software
Familiaridade com sistemas Windows e Office 365
Habilidade de comunicacao e empatia
Senso de urgencia e organizacao
Facilidade para aprender novas ferramentas

🚀 E se tiver isso tambem, melhor ainda:
Conhecimento em redes de computadores
Experiencia com ServiceNow ou ferramentas de help desk
Cursando ou graduado na area de TI";
            job.Benefits = inovaTechBenefits;
            job.Location = "Rio de Janeiro - RJ (Presencial - Escritorio no Centro)";
            job.Schedule = "Segunda a sexta, 08:00 as 18:00 (com horario de almoco)";
            job.MinimumSalary = 2500; job.MaximumSalary = 4000;
            job.WorkModel = WorkModel.OnSite; job.Level = JobLevel.Junior;
            job.OpenPositions = 2; job.Status = JobStatus.Published; job.PublishedAt = DateTime.UtcNow.AddDays(-3);
            job.ClosingDate = DateTime.UtcNow.AddDays(35); job.Tags = "Suporte, Atendimento, TI, Help Desk";
        }
    }

    private static async Task EnsureNotificationsAsync(JobConnectDbContext context, ApplicationUser candidate)
    {
        if (await context.Notificacoes.AnyAsync(n => n.UserId == candidate.Id)) return;

        context.Notificacoes.Add(new Notificacao
        {
            UserId = candidate.Id, Type = NotificationType.StageAdvanced,
            Title = "Perfil pronto para candidaturas",
            Message = "Seu curriculo principal esta disponivel para novas inscricoes."
        });
    }
}
