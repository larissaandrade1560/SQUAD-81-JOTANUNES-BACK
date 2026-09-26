using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

/// <summary>Tracked user plus the current company state loaded in the same database query.</summary>
public sealed record UsuarioSecurityState(Usuario Usuario, TipoEmpresa? EmpresaTipo, bool? EmpresaAtiva);
