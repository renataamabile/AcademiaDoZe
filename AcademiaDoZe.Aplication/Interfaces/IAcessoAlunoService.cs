
// Renata Amabile Basquerote
using AcademiaDoZe.Application.Models;
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Application.Interfaces;

public interface IAcessoAlunoService
{
    Task<ResultadoAcesso> RegistrarEntradaOuSaidaAsync(
        int alunoId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<AcessoAluno>> ObterAcessosHojeAsync(
        CancellationToken cancellationToken = default);
}
