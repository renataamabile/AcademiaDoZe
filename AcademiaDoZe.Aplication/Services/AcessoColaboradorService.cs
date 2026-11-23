// Renata Amabile Basquerote
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Models;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoColaboradorService : IAcessoColaboradorService
{
    private readonly Func<IAcessoColaboradorRepository> _acessoRepoFactory;
    private readonly Func<IColaboradorRepository> _colaboradorRepoFactory;

    public AcessoColaboradorService(
        Func<IAcessoColaboradorRepository> acessoRepoFactory,
        Func<IColaboradorRepository> colaboradorRepoFactory)
    {
        _acessoRepoFactory = acessoRepoFactory ?? throw new ArgumentNullException(nameof(acessoRepoFactory));
        _colaboradorRepoFactory = colaboradorRepoFactory ?? throw new ArgumentNullException(nameof(colaboradorRepoFactory));
    }

    public async Task<ResultadoAcesso> RegistrarEntradaOuSaidaAsync(
        int colaboradorId,
        CancellationToken cancellationToken = default)
    {
        var colaborador = await _colaboradorRepoFactory().ObterPorId(
            colaboradorId,
            cancellationToken: cancellationToken);

        if (colaborador == null)
            throw new InvalidOperationException("Colaborador não encontrado.");

        var repo = _acessoRepoFactory();
        var hoje = DateOnly.FromDateTime(DateTime.Today);

        var acessos = (await repo.ObterAcessosPorColaboradorPeriodo(
            colaboradorId, hoje, hoje, cancellationToken))
            .OrderBy(a => a.DataHora)
            .ToList();

        var limite = colaborador.Vinculo == ColaboradorVinculo.Clt
            ? TimeSpan.FromHours(8)
            : TimeSpan.FromHours(6);

        if (acessos.Count % 2 == 0)
        {
            var horas = await repo.ObterHorasTrabalhadasNoDia(
                colaboradorId, hoje, cancellationToken);

            if (horas >= limite)
                throw new InvalidOperationException(
                    $"Limite diário de {limite.TotalHours} horas já atingido.");

            var entrada = AcessoColaborador.Criar(0, colaborador, DateTime.Now);

            if (entrada.IsFailure)
                throw new InvalidOperationException(
                    string.Join("; ", entrada.Notifications.Select(n => n.Mensagem)));

            await repo.Adicionar(entrada.Value!, cancellationToken);

            return new ResultadoAcesso
            {
                NomePessoa = colaborador.Nome,
                Foto = colaborador.Foto?.Conteudo,
                Mensagem = $"Entrada registrada para {colaborador.Nome}. Horas trabalhadas hoje: {horas.TotalHours:0.##} de {limite.TotalHours} horas."
            };
        }

        var ultimo = acessos.LastOrDefault()
            ?? throw new InvalidOperationException("Não foi possível localizar a entrada em aberto.");

        var tempo = DateTime.Now - ultimo.DataHora;

        var saida = AcessoColaborador.Criar(0, colaborador, DateTime.Now);

        if (saida.IsFailure)
            throw new InvalidOperationException(
                string.Join("; ", saida.Notifications.Select(n => n.Mensagem)));

        await repo.Adicionar(saida.Value!, cancellationToken);

        var total = await repo.ObterHorasTrabalhadasNoDia(
            colaboradorId, hoje, cancellationToken);

        return new ResultadoAcesso
        {
            NomePessoa = colaborador.Nome,
            Foto = colaborador.Foto?.Conteudo,
            Mensagem = $"Saída registrada para {colaborador.Nome}. Permanência: {tempo.Hours}h {tempo.Minutes}min. Total trabalhado hoje: {total.Hours}h {total.Minutes}min."
        };
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterAcessosHojeAsync(
        CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);

        return await _acessoRepoFactory().ObterAcessosPorColaboradorPeriodo(
            inicio: hoje,
            fim: hoje,
            cancellationToken: cancellationToken);
    }
}