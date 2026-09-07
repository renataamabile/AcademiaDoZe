// Renata Amabile Basquerote
// Renata Amabile Basquerote

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;

using InfrastructureDatabaseType =
    AcademiaDoZe.Infrastructure.Data.DatabaseType;

namespace AcademiaDoZe.Infrastructure.Tests;

public class LogradouroInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _repository;

    private string NomeRua => "Renata";
    private string SobrenomeBairro => "Basquerote";

    private string CidadeTeste => DatabaseType switch
    {
        InfrastructureDatabaseType.Sqlite => "SQLite",
        InfrastructureDatabaseType.SqlServer => "SQLServer",
        InfrastructureDatabaseType.MySql => "MySQL",
        _ => throw new ArgumentOutOfRangeException()
    };

    public LogradouroInfrastructureTests()
    {
        _repository = new LogradouroRepository(
            ConnectionString,
            DatabaseType);
    }

    internal static async Task<Logradouro> CriarEInserirLogradouroAsync(
        LogradouroRepository logradouroRepo,
        InfrastructureDatabaseType databaseType)
    {
        var cep = GerarCep();

        var cidade = databaseType switch
        {
            InfrastructureDatabaseType.Sqlite => "SQLite",
            InfrastructureDatabaseType.SqlServer => "SQLServer",
            InfrastructureDatabaseType.MySql => "MySQL",
            _ => throw new ArgumentOutOfRangeException(
                nameof(databaseType))
        };

        var resultado = Logradouro.Criar(
            0,
            cep,
            "Renata",
            "Basquerote",
            cidade,
            "SC",
            "Brasil");

        if (resultado.IsFailure)
        {
            throw new Exception(
                $"Falha ao criar Logradouro: " +
                $"{string.Join(", ", resultado.Notifications.Select(n => n.Mensagem))}");
        }

        return await logradouroRepo.Adicionar(resultado.Value!);
    }

    [Fact]
    public async Task Logradouro_Adicionar_E_ObterPorId_Sucesso()
    {
        var cep = GerarCep();

        var logradouro = Logradouro.Criar(
            0,
            cep,
            NomeRua,
            SobrenomeBairro,
            CidadeTeste,
            "SC",
            "Brasil").Value!;

        var inserido = await _repository.Adicionar(logradouro);

        Assert.NotNull(inserido);
        Assert.True(inserido.Id > 0);
        Assert.Equal(cep, inserido.Cep.Valor);
        Assert.Equal(NomeRua, inserido.Nome);
        Assert.Equal(SobrenomeBairro, inserido.Bairro);
        Assert.Equal(CidadeTeste, inserido.Cidade);

        var obtido =
            await _repository.ObterPorId(inserido.Id);

        Assert.NotNull(obtido);
        Assert.Equal(inserido.Id, obtido.Id);
        Assert.Equal(cep, obtido.Cep.Valor);
        Assert.Equal(NomeRua, obtido.Nome);
        Assert.Equal(SobrenomeBairro, obtido.Bairro);
        Assert.Equal(CidadeTeste, obtido.Cidade);
    }

    [Fact]
    public async Task Logradouro_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtido =
            await _repository.ObterPorId(999999);

        Assert.Null(obtido);
    }

    [Fact]
    public async Task Logradouro_ObterTodos_Sucesso()
    {
        await CriarEInserirLogradouroAsync(
            _repository,
            DatabaseType);

        var todos =
            await _repository.ObterTodos();

        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }

    [Fact]
    public async Task Logradouro_Atualizar_Sucesso()
    {
        var logradouro =
            await CriarEInserirLogradouroAsync(
                _repository,
                DatabaseType);

        var novoCep = GerarCep();

        var atualizado = Logradouro.Criar(
            logradouro.Id,
            novoCep,
            NomeRua,
            SobrenomeBairro,
            CidadeTeste,
            "SC",
            "Brasil").Value!;

        var resultado =
            await _repository.Atualizar(atualizado);

        Assert.NotNull(resultado);
        Assert.Equal(NomeRua, resultado.Nome);
        Assert.Equal(SobrenomeBairro, resultado.Bairro);
        Assert.Equal(CidadeTeste, resultado.Cidade);

        var noBanco =
            await _repository.ObterPorId(logradouro.Id);

        Assert.NotNull(noBanco);
        Assert.Equal(NomeRua, noBanco.Nome);
        Assert.Equal(SobrenomeBairro, noBanco.Bairro);
        Assert.Equal(CidadeTeste, noBanco.Cidade);
    }

    [Fact]
    public async Task Logradouro_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var logradouro = Logradouro.Criar(
            999999,
            GerarCep(),
            NomeRua,
            SobrenomeBairro,
            CidadeTeste,
            "SC",
            "Brasil").Value!;

        var ex =
            await Assert.ThrowsAsync<InfrastructureException>(
                () => _repository.Atualizar(logradouro));

        Assert.Equal(
            "REGISTRO_NAO_ENCONTRADO",
            ex.ErrorCode);
    }

    [Fact]
    public async Task Logradouro_Remover_Sucesso()
    {
        var logradouro =
            await CriarEInserirLogradouroAsync(
                _repository,
                DatabaseType);

        var removido =
            await _repository.Remover(logradouro.Id);

        Assert.True(removido);

        var noBanco =
            await _repository.ObterPorId(logradouro.Id);

        Assert.Null(noBanco);
    }

    [Fact]
    public async Task Logradouro_Remover_RetornaFalseQuandoInexistente()
    {
        var removida =
            await _repository.Remover(999999);

        Assert.False(removida);
    }

    [Fact]
    public async Task Logradouro_ObterPorCep_SucessoENulo()
    {
        var logradouro =
            await CriarEInserirLogradouroAsync(
                _repository,
                DatabaseType);

        var obtido =
            await _repository.ObterPorCep(logradouro.Cep);

        Assert.NotNull(obtido);
        Assert.Equal(
            logradouro.Id,
            obtido.Id);

        var inexistente =
            Cep.Criar("99999999").Value!;

        var naoObtido =
            await _repository.ObterPorCep(inexistente);

        Assert.Null(naoObtido);
    }

    [Fact]
    public async Task Logradouro_CepJaExiste_ValidaçãoCorreta()
    {
        var logradouro =
            await CriarEInserirLogradouroAsync(
                _repository,
                DatabaseType);

        Assert.True(
            await _repository.CepJaExiste(
                logradouro.Cep));

        Assert.False(
            await _repository.CepJaExiste(
                logradouro.Cep,
                logradouro.Id));

        var cepInedito =
            Cep.Criar(GerarCep()).Value!;

        Assert.False(
            await _repository.CepJaExiste(
                cepInedito));
    }

    [Fact]
    public async Task Logradouro_ObterPorCidade_FiltragemCorreta()
    {
        var cidade =
            "CidadeUnica_" +
            Guid.NewGuid().ToString("N")[..5];

        var logradouro = Logradouro.Criar(
            0,
            GerarCep(),
            NomeRua,
            SobrenomeBairro,
            cidade,
            "SC",
            "Brasil").Value!;

        await _repository.Adicionar(logradouro);

        var resultados =
            await _repository.ObterPorCidade(
                cidade.ToLower());

        Assert.NotNull(resultados);
        Assert.Single(resultados);
        Assert.Equal(
            cidade,
            resultados.First().Cidade);

        var vazios =
            await _repository.ObterPorCidade(
                "CidadeInexistente_123");

        Assert.Empty(vazios);
    }

    [Fact]
    public async Task Logradouro_ObterPorBairro_FiltragemCorreta()
    {
        var cidade =
            "Cidade_" +
            Guid.NewGuid().ToString("N")[..5];

        var bairro =
            "Bairro_" +
            Guid.NewGuid().ToString("N")[..5];

        var logradouro = Logradouro.Criar(
            0,
            GerarCep(),
            NomeRua,
            bairro,
            cidade,
            "SC",
            "Brasil").Value!;

        await _repository.Adicionar(logradouro);

        var resultados =
            await _repository.ObterPorBairro(
                cidade,
                bairro);

        Assert.NotNull(resultados);
        Assert.Single(resultados);
        Assert.Equal(
            cidade,
            resultados.First().Cidade);
        Assert.Equal(
            bairro,
            resultados.First().Bairro);

        var vazios =
            await _repository.ObterPorBairro(
                cidade,
                "BairroInexistente");

        Assert.Empty(vazios);
    }
}
