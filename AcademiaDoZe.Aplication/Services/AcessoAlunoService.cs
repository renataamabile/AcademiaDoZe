
// Renata Amabile Basquerote
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Models;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoAlunoService : IAcessoAlunoService
{
    private readonly Func<IAcessoAlunoRepository> _acessoRepoFactory;
    private readonly Func<IAlunoRepository> _alunoRepoFactory;
    private readonly Func<IMatriculaRepository> _matriculaRepoFactory;

    public AcessoAlunoService(
        Func<IAcessoAlunoRepository> acessoRepoFactory,
        Func<IAlunoRepository> alunoRepoFactory,
        Func<IMatriculaRepository> matriculaRepoFactory)
    {
        _acessoRepoFactory = acessoRepoFactory ?? throw new ArgumentNullException(nameof(acessoRepoFactory));
        _alunoRepoFactory = alunoRepoFactory ?? throw new ArgumentNullException(nameof(alunoRepoFactory));
        _matriculaRepoFactory = matriculaRepoFactory ?? throw new ArgumentNullException(nameof(matriculaRepoFactory));
    }

    public async Task<ResultadoAcesso> RegistrarEntradaOuSaidaAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        var aluno = await _alunoRepoFactory().ObterPorId(alunoId, cancellationToken)
            ?? throw new InvalidOperationException("Aluno não encontrado.");

        var matricula = await _matriculaRepoFactory()
            .ObterMatriculaAtivaPorAluno(alunoId, cancellationToken);

        if (matricula == null || matricula.DataFim < DateOnly.FromDateTime(DateTime.Today))
            throw new InvalidOperationException("Aluno não possui matrícula ativa.");

        var repo = _acessoRepoFactory();
        var hoje = DateOnly.FromDateTime(DateTime.Today);

        var acessosHoje = (await repo.ObterAcessosPorAlunoPeriodo(
            alunoId, hoje, hoje, cancellationToken))
            .OrderBy(a => a.DataHora)
            .ToList();

        var ultimo = acessosHoje.LastOrDefault();

        if (acessosHoje.Count % 2 == 0)
        {
            var entrada = AcessoAluno.Criar(0, aluno, DateTime.Now);

            if (entrada.IsFailure)
                throw new InvalidOperationException(
                    string.Join("; ", entrada.Notifications.Select(n => n.Mensagem)));

            await repo.Adicionar(entrada.Value!, cancellationToken);

            var diasRestantes = matricula.DataFim.DayNumber - hoje.DayNumber;

            return new ResultadoAcesso
            {
                NomePessoa = aluno.Nome,
                Foto = aluno.Foto?.Conteudo,
                Mensagem = $"Entrada registrada para {aluno.Nome}. Restam {Math.Max(0, diasRestantes)} dias de matrícula."
            };
        }

        if (ultimo == null)
            throw new InvalidOperationException("Não foi possível localizar a entrada em aberto.");

        var tempo = DateTime.Now - ultimo.DataHora;

        var saida = AcessoAluno.Criar(0, aluno, DateTime.Now);

        if (saida.IsFailure)
            throw new InvalidOperationException(
                string.Join("; ", saida.Notifications.Select(n => n.Mensagem)));

        await repo.Adicionar(saida.Value!, cancellationToken);

        return new ResultadoAcesso
        {
            NomePessoa = aluno.Nome,
            Foto = aluno.Foto?.Conteudo,
            Mensagem = $"Saída registrada para {aluno.Nome}. Permanência: {tempo.Hours}h {tempo.Minutes}min."
        };
    }

    public async Task<IEnumerable<AcessoAluno>> ObterAcessosHojeAsync(
        CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);

        return await _acessoRepoFactory().ObterAcessosPorAlunoPeriodo(
            inicio: hoje,
            fim: hoje,
            cancellationToken: cancellationToken);
    }
}
