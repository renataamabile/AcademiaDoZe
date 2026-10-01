// Renata Amabile Basquerote
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public class AlunoInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepo;
    private readonly AlunoRepository _alunoRepo;

    public AlunoInfrastructureTests()
    {
        _logradouroRepo =
            new LogradouroRepository(
                ConnectionString,
                DatabaseType);

        _alunoRepo =
            new AlunoRepository(
                ConnectionString,
                DatabaseType);
    }

    private static string SenhaDoBanco(
        DatabaseType databaseType)
    {
        return databaseType switch
        {
            DatabaseType.Sqlite => "SenhaSQLite123",
            DatabaseType.SqlServer => "SenhaSQLServer123",
            DatabaseType.MySql => "SenhaMySQL123",
            _ => throw new ArgumentOutOfRangeException(
                nameof(databaseType))
        };
    }

    internal static async Task<Aluno> CriarEInserirAlunoAsync(
        AlunoRepository alunoRepo,
        LogradouroRepository logradouroRepo,
        DatabaseType databaseType = DatabaseType.Sqlite)
    {
        var logradouro =
            await LogradouroInfrastructureTests
                .CriarEInserirLogradouroAsync(
                    logradouroRepo,
                    databaseType);

        var foto =
            Arquivo.Criar(
                new byte[] { 5, 6, 7, 8 })
            .Value!;

        // Gera CPF único contra o banco para reduzir chances de colisão intermitente nos testes
        string cpf;
        do
        {
            cpf = GerarCpf();
        }
        while (await alunoRepo.CpfJaExiste(Cpf.Criar(cpf).Value!));

        // Prepara campos para poder recriar Aluno em caso de CPF duplicado ao inserir
        var nomeAluno = "Renata Amabile " + Guid.NewGuid().ToString("N")[..5];
        var dataNascimento = new DateOnly(1995, 5, 15);
        var telefone = GerarTelefone();
        var email = GerarEmail();
        var numero = "200";
        var complementoAluno = "Basquerote";
        var senhaBanco = SenhaDoBanco(databaseType);

        var alunoResult = Aluno.Criar(
            id: 0,
            nome: nomeAluno,
            cpf: cpf,
            dataNascimento: dataNascimento,
            telefone: telefone,
            email: email,
            endereco: logradouro,
            numero: numero,
            complemento: complementoAluno,
            senha: senhaBanco,
            foto: foto
        );

        if (alunoResult.IsFailure)
        {
            throw new Exception(
                $"Falha ao criar Aluno: " +
                $"{string.Join(", ", alunoResult.Notifications.Select(n => n.Mensagem))}");
        }

        // Tenta inserir, se houver CPF duplicado gera novo CPF e re-tenta (até 5 tentativas)
        int tentativas = 0;
        while (true)
        {
            try
            {
                return await alunoRepo.Adicionar(alunoResult.Value!);
            }
            catch (InfrastructureException iex) when (iex.ErrorCode == "CPF_DUPLICADO" && tentativas < 4)
            {
                tentativas++;
                // Gera novo CPF que não existe no banco
                string novoCpf;
                do
                {
                    novoCpf = GerarCpf();
                }
                while (await alunoRepo.CpfJaExiste(Cpf.Criar(novoCpf).Value!));

                // recria o objeto Aluno com novo CPF
                alunoResult = Aluno.Criar(
                    id: 0,
                    nome: nomeAluno,
                    cpf: novoCpf,
                    dataNascimento: dataNascimento,
                    telefone: telefone,
                    email: email,
                    endereco: logradouro,
                    numero: numero,
                    complemento: complementoAluno,
                    senha: senhaBanco,
                    foto: foto
                );

                if (alunoResult.IsFailure)
                {
                    throw new Exception(
                        $"Falha ao recriar Aluno após CPF duplicado: " +
                        $"{string.Join(", ", alunoResult.Notifications.Select(n => n.Mensagem))}");
                }
            }
        }
    }

    // Cria o aluno e registra os dados criados
    // para que o TestBase faça a limpeza ao final do teste.
    private async Task<Aluno> CriarAlunoParaTesteAsync()
    {
        var aluno = await CriarEInserirAlunoAsync(
            _alunoRepo,
            _logradouroRepo,
            DatabaseType);

        RegistrarAlunoCriado(aluno.Cpf.Valor);
        RegistrarLogradouroCriado(aluno.Endereco.LogradouroId);

        return aluno;
    }

    [Fact]
    public async Task Aluno_Adicionar_E_ObterPorId_Sucesso()
    {
        var aluno =
            await CriarAlunoParaTesteAsync();

        Assert.NotNull(aluno);
        Assert.True(aluno.Id > 0);

        var obtido =
            await _alunoRepo.ObterPorId(aluno.Id);

        Assert.NotNull(obtido);
        Assert.Equal(aluno.Id, obtido.Id);
        Assert.Equal(aluno.Cpf.Valor, obtido.Cpf.Valor);
        Assert.Equal(aluno.Nome, obtido.Nome);
        Assert.Equal(aluno.Email.Valor, obtido.Email.Valor);

        Assert.NotNull(obtido.Endereco);

        Assert.Equal(
            aluno.Endereco.LogradouroId,
            obtido.Endereco.LogradouroId);
    }

    [Fact]
    public async Task Aluno_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtido =
            await _alunoRepo.ObterPorId(999999);

        Assert.Null(obtido);
    }

    [Fact]
    public async Task Aluno_ObterTodos_Sucesso()
    {
        await CriarAlunoParaTesteAsync();

        var todos =
            await _alunoRepo.ObterTodos();

        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }

    [Fact]
    public async Task Aluno_Atualizar_Sucesso()
    {
        var logradouro =
            await LogradouroInfrastructureTests
                .CriarEInserirLogradouroAsync(
                    _logradouroRepo,
                    DatabaseType);

        RegistrarLogradouroCriado(logradouro.Id);

        var foto =
            Arquivo.Criar(
                new byte[] { 5, 6, 7, 8 })
            .Value!;

        var aluno =
            await _alunoRepo.Adicionar(
                Aluno.Criar(
                    0,
                    "Aluno Original",
                    GerarCpf(),
                    new DateOnly(1995, 5, 15),
                    GerarTelefone(),
                    GerarEmail(),
                    logradouro,
                    "200",
                    "Basquerote",
                    SenhaDoBanco(DatabaseType),
                    foto
                ).Value!);

        RegistrarAlunoCriado(aluno.Cpf.Valor);

        var novoNome =
            "Renata Amabile Editado " +
            Guid.NewGuid().ToString("N")[..5];

        var alunoAtualizado =
            Aluno.Criar(
                id: aluno.Id,
                nome: novoNome,
                cpf: aluno.Cpf.Valor,
                dataNascimento: aluno.DataNascimento,
                telefone: aluno.Telefone.Valor,
                email: aluno.Email.Valor,
                endereco: logradouro,
                numero: "300",
                complemento: "Basquerote",
                senha: aluno.Senha.Valor,
                foto: aluno.Foto
            ).Value!;

        var resultado =
            await _alunoRepo.Atualizar(
                alunoAtualizado);

        Assert.NotNull(resultado);

        Assert.Equal(
            novoNome,
            resultado.Nome);

        var noBanco =
            await _alunoRepo.ObterPorId(
                aluno.Id);

        Assert.NotNull(noBanco);

        Assert.Equal(
            novoNome,
            noBanco.Nome);

        Assert.Equal(
            "Basquerote",
            noBanco.Endereco.Complemento);
    }

    [Fact]
    public async Task Aluno_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var logradouro =
            await LogradouroInfrastructureTests
                .CriarEInserirLogradouroAsync(
                    _logradouroRepo,
                    DatabaseType);

        RegistrarLogradouroCriado(logradouro.Id);

        var foto =
            Arquivo.Criar(
                new byte[] { 1, 2 })
            .Value!;

        var alunoInexistente =
            Aluno.Criar(
                id: 999999,
                nome: "Renata Amabile",
                cpf: GerarCpf(),
                dataNascimento: new DateOnly(1990, 1, 1),
                telefone: GerarTelefone(),
                email: GerarEmail(),
                endereco: logradouro,
                numero: "1",
                complemento: "Basquerote",
                senha: SenhaDoBanco(DatabaseType),
                foto: foto
            ).Value!;

        var ex =
            await Assert.ThrowsAsync<InfrastructureException>(
                () =>
                    _alunoRepo.Atualizar(
                        alunoInexistente));

        Assert.Equal(
            "REGISTRO_NAO_ENCONTRADO",
            ex.ErrorCode);
    }

    [Fact]
    public async Task Aluno_Remover_Sucesso()
    {
        var aluno =
            await CriarAlunoParaTesteAsync();

        var removido =
            await _alunoRepo.Remover(
                aluno.Id);

        Assert.True(removido);

        var noBanco =
            await _alunoRepo.ObterPorId(
                aluno.Id);

        Assert.Null(noBanco);
    }

    [Fact]
    public async Task Aluno_Remover_RetornaFalseQuandoInexistente()
    {
        var removido =
            await _alunoRepo.Remover(
                999999);

        Assert.False(removido);
    }

    [Fact]
    public async Task Aluno_ObterPorCpf_SucessoENulo()
    {
        var aluno =
            await CriarAlunoParaTesteAsync();

        var obtido =
            await _alunoRepo.ObterPorCpf(
                aluno.Cpf);

        Assert.NotNull(obtido);

        Assert.Equal(
            aluno.Id,
            obtido.Id);

        var cpfInexistente =
            Cpf.Criar(
                GerarCpf()).Value!;

        var naoObtido =
            await _alunoRepo.ObterPorCpf(
                cpfInexistente);

        Assert.Null(naoObtido);
    }

    [Fact]
    public async Task Aluno_ObterPorEmail_SucessoENulo()
    {
        var aluno =
            await CriarAlunoParaTesteAsync();

        var obtido =
            await _alunoRepo.ObterPorEmail(
                aluno.Email);

        Assert.NotNull(obtido);

        Assert.Equal(
            aluno.Id,
            obtido.Id);

        var emailInexistente =
            Email.Criar(
                GerarEmail()).Value!;

        var naoObtido =
            await _alunoRepo.ObterPorEmail(
                emailInexistente);

        Assert.Null(naoObtido);
    }

    [Fact]
    public async Task Aluno_CpfJaExiste_ValidacaoCorreta()
    {
        var aluno =
            await CriarAlunoParaTesteAsync();

        var existe =
            await _alunoRepo.CpfJaExiste(
                aluno.Cpf);

        Assert.True(existe);

        var existeIgnorandoId =
            await _alunoRepo.CpfJaExiste(
                aluno.Cpf,
                aluno.Id);

        Assert.False(existeIgnorandoId);

        var cpfInedito =
            Cpf.Criar(
                GerarCpf()).Value!;

        var existeInedito =
            await _alunoRepo.CpfJaExiste(
                cpfInedito);

        Assert.False(existeInedito);
    }

    [Fact]
    public async Task Aluno_EmailJaExiste_ValidacaoCorreta()
    {
        var aluno =
            await CriarAlunoParaTesteAsync();

        var existe =
            await _alunoRepo.EmailJaExiste(
                aluno.Email);

        Assert.True(existe);

        var existeIgnorandoId =
            await _alunoRepo.EmailJaExiste(
                aluno.Email,
                aluno.Id);

        Assert.False(existeIgnorandoId);

        var emailInedito =
            Email.Criar(
                GerarEmail()).Value!;

        var existeInedito =
            await _alunoRepo.EmailJaExiste(
                emailInedito);

        Assert.False(existeInedito);
    }

    [Fact]
    public async Task Aluno_ObterPorNome_FiltragemCorreta()
    {
        var aluno =
            await CriarAlunoParaTesteAsync();

        var resultados =
            await _alunoRepo.ObterPorNome(
                "Renata Amabile");

        Assert.NotNull(resultados);

        Assert.Contains(
            resultados,
            a => a.Id == aluno.Id);
    }

    [Fact]
    public async Task Aluno_TrocarSenha_SucessoEFalha()
    {
        var aluno =
            await CriarAlunoParaTesteAsync();

        string novaSenhaTexto =
            SenhaDoBanco(DatabaseType) +
            "Nova";

        var novaSenha =
            Senha.Criar(
                novaSenhaTexto).Value!;

        var alterou =
            await _alunoRepo.TrocarSenha(
                aluno.Id,
                novaSenha);

        Assert.True(alterou);

        var atualizado =
            await _alunoRepo.ObterPorId(
                aluno.Id);

        Assert.NotNull(atualizado);

        Assert.Equal(
            novaSenhaTexto,
            atualizado.Senha.Valor);

        var alterouInexistente =
            await _alunoRepo.TrocarSenha(
                999999,
                novaSenha);

        Assert.False(alterouInexistente);
    }
}