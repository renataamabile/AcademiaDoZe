// Renata Amabile Basquerote
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoColaboradorRepository : BaseRepository, IAcessoColaboradorRepository
{
    public AcessoColaboradorRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

    private const string BaseSelectQuery = """
        SELECT ac.id_acesso, ac.pessoa_id, ac.data_hora,
               c.id_colaborador, c.cpf, c.nome AS colaborador_nome, c.nascimento, c.telefone, c.email,
               c.logradouro_id, c.numero, c.complemento, c.senha, c.foto, c.admissao, c.tipo, c.vinculo,
               l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
        FROM tb_acesso ac
        INNER JOIN tb_colaborador c ON ac.pessoa_id = c.id_colaborador
        INNER JOIN tb_logradouro l ON c.logradouro_id = l.id_logradouro
        WHERE ac.pessoa_tipo = 1
        """;

    private static AcessoColaborador Map(DbDataReader reader)
    {
        var colaborador = ColaboradorRepository.Map(reader, "colaborador_nome");
        var result = AcessoColaborador.Criar(reader.GetInt32Value("id_acesso"), colaborador, reader.GetDateTimeValue("data_hora"));
        if (result.IsFailure)
            throw new InfrastructureException("ERRO_MAPEAMENTO_ACESSO_COLABORADOR", string.Join("; ", result.Notifications.Select(n => n.Mensagem)));
        return result.Value!;
    }

    public async Task<AcessoColaborador?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync($"{BaseSelectQuery} AND ac.id_acesso=@Id", cancellationToken);
        command.AddParameter("@Id", id, DbType.Int32);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterTodos(CancellationToken cancellationToken = default)
    {
        var lista = new List<AcessoColaborador>();
        await using var command = await CreateCommandAsync($"{BaseSelectQuery} ORDER BY ac.data_hora DESC", cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) lista.Add(Map(reader));
        return lista;
    }

    public async Task<AcessoColaborador> Adicionar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        string sql = FormatInsertQuery("INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (1, @PessoaId, @DataHora)");
        await using var command = await CreateCommandAsync(sql, cancellationToken);
        command.AddParameter("@PessoaId", entity.ColaboradorId, DbType.Int32);
        command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
        int id = await command.ExecuteScalarIdAsync("ERRO_INSERIR_ACESSO_COLABORADOR", "Falha ao inserir acesso do colaborador.", cancellationToken);
        var colaborador = await ObterColaboradorAsync(
            entity.ColaboradorId, cancellationToken);
        return AcessoColaborador.Criar(id, colaborador, entity.DataHora).Value!;
    }

    public async Task<AcessoColaborador> Atualizar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync("UPDATE tb_acesso SET pessoa_id=@PessoaId, data_hora=@DataHora WHERE id_acesso=@Id AND pessoa_tipo=1", cancellationToken);
        command.AddParameter("@PessoaId", entity.ColaboradorId, DbType.Int32);
        command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
        command.AddParameter("@Id", entity.Id, DbType.Int32);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync("DELETE FROM tb_acesso WHERE id_acesso=@Id AND pessoa_tipo=1", cancellationToken);
        command.AddParameter("@Id", id, DbType.Int32);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterAcessosPorColaboradorPeriodo(int? colaboradorId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        var filtros = new List<string>();
        if (colaboradorId.HasValue) filtros.Add("ac.pessoa_id=@ColaboradorId");
        if (inicio.HasValue) filtros.Add("ac.data_hora >= @Inicio");
        if (fim.HasValue) filtros.Add("ac.data_hora < @Fim");
        string query = BaseSelectQuery + (filtros.Count > 0 ? " AND " + string.Join(" AND ", filtros) : "") + " ORDER BY ac.data_hora";
        await using var command = await CreateCommandAsync(query, cancellationToken);
        if (colaboradorId.HasValue) command.AddParameter("@ColaboradorId", colaboradorId.Value, DbType.Int32);
        if (inicio.HasValue) command.AddParameter("@Inicio", inicio.Value.ToDateTime(TimeOnly.MinValue), DbType.DateTime);
        if (fim.HasValue) command.AddParameter("@Fim", fim.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), DbType.DateTime);
        var lista = new List<AcessoColaborador>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) lista.Add(Map(reader));
        return lista;
    }

    private async Task<Colaborador> ObterColaboradorAsync(int colaboradorId, CancellationToken cancellationToken)
    {
        await using var repository = new ColaboradorRepository(_connectionString, _databaseType);

        return await repository.ObterPorId(colaboradorId, cancellationToken)
            ?? throw new InfrastructureException(
                "ERRO_OBTER_COLABORADOR",
                $"Colaborador com ID {colaboradorId} não encontrado.");
    }

    public async Task<TimeSpan> ObterHorasTrabalhadasNoDia(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default)
    {
        var acessos = (await ObterAcessosPorColaboradorPeriodo(
            colaboradorId,
            data,
            data,
            cancellationToken))
            .OrderBy(a => a.DataHora)
            .ToList();

        TimeSpan total = TimeSpan.Zero;

        for (int i = 0; i + 1 < acessos.Count; i += 2)
        {
            total += acessos[i + 1].DataHora - acessos[i].DataHora;
        }

        return total;
    }

    public async Task<AcessoColaborador?> ObterUltimoAcesso(int colaboradorId, CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync(
            $"{BaseSelectQuery} AND ac.pessoa_id=@ColaboradorId ORDER BY ac.data_hora DESC",
            cancellationToken);

        command.AddParameter("@ColaboradorId", colaboradorId, DbType.Int32);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }
}
