using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public class AlunoDuplicateCpfTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepo;
    private readonly AlunoRepository _alunoRepo;

    public AlunoDuplicateCpfTests()
    {
        _logradouroRepo = new LogradouroRepository(ConnectionString, DatabaseType);
        _alunoRepo = new AlunoRepository(ConnectionString, DatabaseType);
    }

    [Fact]
    public async Task AdicionarAluno_ComCpfDuplicado_DeveLancarCpfDuplicado()
    {
        // Arrange: criar e inserir um aluno
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        RegistrarAlunoCriado(aluno.Cpf.Valor);
        RegistrarLogradouroCriado(aluno.Endereco.LogradouroId);

        // Act: tentar inserir outro aluno com mesmo CPF
        var foto = Arquivo.Criar(new byte[] { 9, 9, 9, 9 }).Value!;

        string senha = this.DatabaseType switch
        {
            global::AcademiaDoZe.Infrastructure.Data.DatabaseType.Sqlite => "SenhaSQLite123",
            global::AcademiaDoZe.Infrastructure.Data.DatabaseType.MySql => "SenhaMySQL123",
            global::AcademiaDoZe.Infrastructure.Data.DatabaseType.SqlServer => "SenhaSQLServer123",
            _ => "SenhaSQLite123"
        };

        var outroAlunoResult = Aluno.Criar(
            id: 0,
            nome: "Aluno Duplicado",
            cpf: aluno.Cpf.Valor,
            dataNascimento: new DateOnly(1990, 1, 1),
            telefone: GerarTelefone(),
            email: GerarEmail(),
            endereco: (await _logradouroRepo.ObterPorId(aluno.Endereco.LogradouroId))!,
            numero: "1",
            complemento: "",
            senha: senha,
            foto: foto
        );

        Assert.True(outroAlunoResult.IsSuccess);

        var outroAluno = outroAlunoResult.Value!;

        // Assert: espera InfrastructureException com código CPF_DUPLICADO
        var ex = await Assert.ThrowsAsync<InfrastructureException>(async () =>
            await _alunoRepo.Adicionar(outroAluno));

        Assert.Equal("CPF_DUPLICADO", ex.ErrorCode);
    }
}
