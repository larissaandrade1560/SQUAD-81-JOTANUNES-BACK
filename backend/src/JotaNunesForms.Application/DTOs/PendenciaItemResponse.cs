namespace JotaNunesForms.Application.DTOs;

public sealed record PendenciaItemResponse(
    string Categoria,
    string CategoriaRotulo,
    Guid? ReferenciaId,
    string? Escopo,
    Guid EmpresaId,
    string EmpresaRazaoSocial,
    Guid? FuncionarioId,
    string? FuncionarioNome,
    string Titulo,
    string Descricao,
    int Severidade,
    string SeveridadeRotulo,
    DateTime? ReferenciaEm);
