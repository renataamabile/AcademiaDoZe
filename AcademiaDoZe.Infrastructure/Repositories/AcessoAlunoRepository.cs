// Renata Amabile Basquerote
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoAlunoRepository : BaseRepository, IAcessoAlunoRepository
{
    public AcessoAlunoRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

    private const string BaseSelectQuery = """
        SELECT ac.id_acesso, ac.pessoa_id, ac.data_hora,
               a.id_aluno, a.cpf, a.nome AS aluno_nome, a.nascimento, a.telefone, a.email,
               a.logradouro_id, a.numero, a.complemento, a.senha, a.foto,
               l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
        FROM tb_acesso ac
        INNER JOIN tb_aluno a ON ac.pessoa_id = a.id_aluno
        INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro
        WHERE ac.pessoa_tipo = 0
        """;

    private static AcessoAluno Map(DbDataReader reader)
    {
        var aluno = AlunoRepository.Map(reader, "aluno_nome");
        var result = AcessoAluno.Criar(reader.GetInt32Value("id_acesso"), aluno, reader.GetDateTimeValue("data_hora"));
        if (result.IsFailure)
            throw new InfrastructureException("ERRO_MAPEAMENTO_ACESSO_ALUNO", string.Join("; ", result.Notifications.Select(n => n.Mensagem)));
        return result.Value!;
    }

    public async Task<AcessoAluno?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync($"{BaseSelectQuery} AND ac.id_acesso = @Id", cancellationToken);
        command.AddParameter("@Id", id, DbType.Int32);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<IEnumerable<AcessoAluno>> ObterTodos(CancellationToken cancellationToken = default)
    {
        var lista = new List<AcessoAluno>();
        await using var command = await CreateCommandAsync($"{BaseSelectQuery} ORDER BY ac.data_hora DESC", cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) lista.Add(Map(reader));
        return lista;
    }

    public async Task<AcessoAluno> Adicionar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        string sql = FormatInsertQuery("INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (0, @PessoaId, @DataHora)");
        await using var command = await CreateCommandAsync(sql, cancellationToken);
        command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
        command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
        entity = AcessoAluno.Criar(await command.ExecuteScalarIdAsync("ERRO_INSERIR_ACESSO_ALUNO", "Falha ao inserir acesso do aluno.", cancellationToken),
            await ObterAlunoAsync(entity.AlunoId, cancellationToken), entity.DataHora).Value!;
        return entity;
    }

    private async Task<Aluno> ObterAlunoAsync(int alunoId, CancellationToken cancellationToken)
    {
        await using var repository = new AlunoRepository(_connectionString, _databaseType);

        return await repository.ObterPorId(alunoId, cancellationToken)
            ?? throw new InfrastructureException(
                "ERRO_OBTER_ALUNO",
                $"Aluno com ID {alunoId} não encontrado.");
    }

    public async Task<AcessoAluno> Atualizar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync("UPDATE tb_acesso SET pessoa_id=@PessoaId, data_hora=@DataHora WHERE id_acesso=@Id AND pessoa_tipo=0", cancellationToken);
        command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
        command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
        command.AddParameter("@Id", entity.Id, DbType.Int32);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync("DELETE FROM tb_acesso WHERE id_acesso=@Id AND pessoa_tipo=0", cancellationToken);
        command.AddParameter("@Id", id, DbType.Int32);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<IEnumerable<AcessoAluno>> ObterAcessosPorAlunoPeriodo(int? alunoId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        var filtros = new List<string>();
        if (alunoId.HasValue) filtros.Add("ac.pessoa_id=@AlunoId");
        if (inicio.HasValue) filtros.Add("ac.data_hora >= @Inicio");
        if (fim.HasValue) filtros.Add("ac.data_hora < @Fim");
        string query = BaseSelectQuery + (filtros.Count > 0 ? " AND " + string.Join(" AND ", filtros) : "") + " ORDER BY ac.data_hora";
        await using var command = await CreateCommandAsync(query, cancellationToken);
        if (alunoId.HasValue) command.AddParameter("@AlunoId", alunoId.Value, DbType.Int32);
        if (inicio.HasValue) command.AddParameter("@Inicio", inicio.Value.ToDateTime(TimeOnly.MinValue), DbType.DateTime);
        if (fim.HasValue) command.AddParameter("@Fim", fim.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), DbType.DateTime);
        var lista = new List<AcessoAluno>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) lista.Add(Map(reader));
        return lista;
    }

    public async Task<AcessoAluno?> ObterUltimoAcesso(
    int alunoId,
    CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync(
            $"{BaseSelectQuery} AND ac.pessoa_id = @AlunoId ORDER BY ac.data_hora DESC",
            cancellationToken);

        command.AddParameter("@AlunoId", alunoId, DbType.Int32);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<bool> EstaNaAcademia(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        var acessos = await ObterAcessosPorAlunoPeriodo(
            alunoId,
            cancellationToken: cancellationToken);

        return acessos.Count() % 2 != 0;
    }

    public Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMes(int mes, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMes(int mes, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Aluno>> ObterAlunosSemAcessoNosUltimosDias(int dias, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}