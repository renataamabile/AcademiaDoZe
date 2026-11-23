
// Renata Amabile Basquerote
using AcademiaDoZe.Application.Models;
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Application.Interfaces;

public interface IAcessoColaboradorService
{
    Task<ResultadoAcesso> RegistrarEntradaOuSaidaAsync(
        int colaboradorId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<AcessoColaborador>> ObterAcessosHojeAsync(
        CancellationToken cancellationToken = default);
}
