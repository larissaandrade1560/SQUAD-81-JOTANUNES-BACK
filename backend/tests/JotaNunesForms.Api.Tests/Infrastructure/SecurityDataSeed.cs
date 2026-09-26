using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace JotaNunesForms.Api.Tests.Infrastructure;

public static class SecurityDataSeed
{
    public static async Task<SecurityTestData> SeedCanonicalAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<JotaNunesFormsDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var companyA = new Empresa("Empresa MO A", "11111111111111", TipoEmpresa.MaoDeObra);
        var companyB = new Empresa("Empresa MO B", "22222222222222", TipoEmpresa.MaoDeObra);
        var companyMaterials = new Empresa("Empresa Materiais", "33333333333333", TipoEmpresa.Materiais);
        var inactiveCompany = new Empresa("Empresa inativa", "44444444444444", TipoEmpresa.MaoDeObra);
        inactiveCompany.DefinirStatus(false);
        var resourceA = new Obra("Obra de teste A", "SEC-A");
        var resourceB = new Obra("Obra de teste B", "SEC-B");
        var passwordHash = passwordHasher.Hash("Security-Test-Password-Only-2026!");
        var users = new[]
        {
            new Usuario("90000000001", passwordHash, "Admin de teste", PerfilUsuario.Administrador),
            new Usuario("90000000002", passwordHash, "Analista de teste", PerfilUsuario.Analista),
            new Usuario("90000000003", passwordHash, "Terceirizado A", PerfilUsuario.Terceirizado, companyA.Id),
            new Usuario("90000000004", passwordHash, "Terceirizado B", PerfilUsuario.Terceirizado, companyB.Id),
            new Usuario("90000000005", passwordHash, "Materiais", PerfilUsuario.Terceirizado, companyMaterials.Id),
            new Usuario("90000000006", passwordHash, "Usuário inativo", PerfilUsuario.Analista),
            new Usuario("90000000007", passwordHash, "Usuário de empresa inativa", PerfilUsuario.Terceirizado, inactiveCompany.Id),
        };
        users[5].DefinirStatus(false);

        var employeeA = new Funcionario(companyA.Id, "Funcionário A", "71111111111", "Operador");
        var employeeB = new Funcionario(companyB.Id, "Funcionário B", "72222222222", "Operador");
        var materialsEmployee = new Funcionario(companyMaterials.Id, "Funcionário Materiais", "73333333333", "Técnico");
        var contractA = new Contrato(companyA.Id, resourceA.Id, "Escopo A", "SEC-CON-A");
        var contractB = new Contrato(companyB.Id, resourceB.Id, "Escopo B", "SEC-CON-B");
        var processA = new ProcessoContratacao(contractA.Id, users[0].Id, "Serviço A", false, false, false, false, 0, true);
        var processB = new ProcessoContratacao(contractB.Id, users[0].Id, "Serviço B", false, false, false, false, 0, true);
        var paymentA = new PagamentoFuncionario(employeeA.Id, companyA.Id, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 10));
        var paymentB = new PagamentoFuncionario(employeeB.Id, companyB.Id, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 10));
        var companyDocumentA = new DocumentoEmpresa(companyA.Id, TipoDocumentoEmpresarial.Outro, "company-a.pdf", "company-a/key", "application/pdf", 12);
        var companyDocumentB = new DocumentoEmpresa(companyB.Id, TipoDocumentoEmpresarial.Outro, "company-b.pdf", "company-b/key", "application/pdf", 12);
        var socioB = new Socio(companyB.Id, "Sócio B", "81111111111");
        var employeeDocumentA = new DocumentoFuncionario(employeeA.Id, TipoDocumentoFuncionario.Outro, "employee-a.pdf", "employee-a/key", "application/pdf", 12);
        var employeeDocumentB = new DocumentoFuncionario(employeeB.Id, TipoDocumentoFuncionario.Outro, "employee-b.pdf", "employee-b/key", "application/pdf", 12);
        var materialsEmployeeDocument = new DocumentoFuncionario(materialsEmployee.Id, TipoDocumentoFuncionario.Outro, "materials-employee.pdf", "materials-employee/key", "application/pdf", 12);
        var requirement = new CatalogoRequisito(
            "SEC-REQ-001",
            "Requisito de teste",
            TitularRequisito.Empresa,
            AplicacaoRequisito.Sempre,
            TipoEntregaRequisito.Upload,
            CamadaRequisito.Corporativa);
        var checklistA = new ItemChecklist(processA.Id, requirement.Id, TitularRequisito.Empresa, true);
        var checklistB = new ItemChecklist(processB.Id, requirement.Id, TitularRequisito.Empresa, true);
        var versionA = new DocumentoVersao(checklistA.Id, 1, users[0].Id, "version-a.pdf", "version-a/key", "application/pdf", 12);
        var versionB = new DocumentoVersao(checklistB.Id, 1, users[0].Id, "version-b.pdf", "version-b/key", "application/pdf", 12);
        var mobilizationA = new Mobilizacao(employeeA.Id, contractA.Id, resourceA.Id, processA.Id, "Operador");
        var mobilizationB = new Mobilizacao(employeeB.Id, contractB.Id, resourceB.Id, processB.Id, "Operador");

        await db.Empresas.AddRangeAsync([companyA, companyB, companyMaterials, inactiveCompany], cancellationToken);
        await db.Obras.AddRangeAsync([resourceA, resourceB], cancellationToken);
        await db.Usuarios.AddRangeAsync(users, cancellationToken);
        await db.Funcionarios.AddRangeAsync([employeeA, employeeB, materialsEmployee], cancellationToken);
        await db.Contratos.AddRangeAsync([contractA, contractB], cancellationToken);
        await db.ProcessosContratacao.AddRangeAsync([processA, processB], cancellationToken);
        await db.PagamentosFuncionario.AddRangeAsync([paymentA, paymentB], cancellationToken);
        await db.DocumentosEmpresa.AddRangeAsync([companyDocumentA, companyDocumentB], cancellationToken);
        await db.Socios.AddAsync(socioB, cancellationToken);
        await db.DocumentosFuncionario.AddRangeAsync([employeeDocumentA, employeeDocumentB, materialsEmployeeDocument], cancellationToken);
        await db.CatalogoRequisitos.AddAsync(requirement, cancellationToken);
        await db.ItensChecklist.AddRangeAsync([checklistA, checklistB], cancellationToken);
        await db.DocumentosVersoes.AddRangeAsync([versionA, versionB], cancellationToken);
        await db.Mobilizacoes.AddRangeAsync([mobilizationA, mobilizationB], cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new SecurityTestData(
            companyA,
            companyB,
            companyMaterials,
            inactiveCompany,
            resourceA,
            resourceB,
            users[0],
            users[1],
            users[2],
            users[3],
            users[4],
            users[5],
            users[6],
            employeeA,
            employeeB,
            materialsEmployee,
            contractA,
            contractB,
            processA,
            processB,
            paymentA,
            paymentB,
            companyDocumentA,
            companyDocumentB,
            socioB,
            employeeDocumentA,
            employeeDocumentB,
            materialsEmployeeDocument,
            checklistA,
            checklistB,
            versionA,
            versionB,
            mobilizationA,
            mobilizationB);
    }
}

public sealed record SecurityTestData(
    Empresa CompanyA,
    Empresa CompanyB,
    Empresa MaterialsCompany,
    Empresa InactiveCompany,
    Obra ResourceA,
    Obra ResourceB,
    Usuario Admin,
    Usuario Analyst,
    Usuario MoA,
    Usuario MoB,
    Usuario Materials,
    Usuario InactiveUser,
    Usuario InactiveCompanyUser,
    Funcionario EmployeeA,
    Funcionario EmployeeB,
    Funcionario MaterialsEmployee,
    Contrato ContractA,
    Contrato ContractB,
    ProcessoContratacao ProcessA,
    ProcessoContratacao ProcessB,
    PagamentoFuncionario PaymentA,
    PagamentoFuncionario PaymentB,
    DocumentoEmpresa CompanyDocumentA,
    DocumentoEmpresa CompanyDocumentB,
    Socio SocioB,
    DocumentoFuncionario EmployeeDocumentA,
    DocumentoFuncionario EmployeeDocumentB,
    DocumentoFuncionario MaterialsEmployeeDocument,
    ItemChecklist ChecklistA,
    ItemChecklist ChecklistB,
    DocumentoVersao VersionA,
    DocumentoVersao VersionB,
    Mobilizacao MobilizationA,
    Mobilizacao MobilizationB);
