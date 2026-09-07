//Renata Amabile Basquerote
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using Xunit;
namespace AcademiaDoZe.Domain.Tests.Entities;

public class MatriculaTests
{
    private static Logradouro GetValidLogradouro() => Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;
    private static Arquivo GetValidArquivo() => Arquivo.Criar(new byte[] { 1, 2, 3 }).Value!;
    private static Aluno GetValidAluno(DateOnly? dataNascimento = null)
    {
        var nascimento = dataNascimento ?? DateOnly.FromDateTime(DateTime.Today.AddYears(-20));
        return Aluno.Criar(1, "João da Silva", "529.982.247-25", nascimento, "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", string.Empty, "Abcdef", GetValidArquivo()).Value!;
    }
    [Theory(DisplayName = "Matricula: plano inválido -> PLANO_INVALIDO; válido -> sucesso")]
    [InlineData(999)]
    [InlineData((int)MatriculaPlano.Mensal)]
    public void Deve_FalharOuPassar_Criacao_Quando_ValorDoPlano(int planoValue)
    {
        var aluno = GetValidAluno();
        var plano = (MatriculaPlano)planoValue;
        var result = Matricula.Criar(1, aluno, plano, DateOnly.FromDateTime(DateTime.Today), "Objetivo", MatriculaRestricoes.None, null, string.Empty);
        if (planoValue == 999)
        {
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Notifications);
            Assert.Contains(result.Notifications, n => n.Mensagem == "PLANO_INVALIDO");
        }
        else
        {
            Assert.True(result.IsSuccess);
            Assert.Equal(aluno.Id, result.Value!.AlunoId);
        }
    }
    [Theory(DisplayName = "Matricula: data inicio obrigatória -> DATA_INICIO_OBRIGATORIO")]
    [InlineData(true)]
    [InlineData(false)]
    public void Deve_Falhar_Criacao_Quando_DataInicioPadrao(bool useDefault)
    {
        var aluno = GetValidAluno();
        var inicio = useDefault ? default(DateOnly) : DateOnly.FromDateTime(DateTime.Today);
        var result = Matricula.Criar(1, aluno, MatriculaPlano.Mensal, inicio, "Objetivo", MatriculaRestricoes.None, null, string.Empty);
        if (useDefault)
        {
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Notifications);
            Assert.Contains(result.Notifications, n => n.Mensagem == "DATA_INICIO_OBRIGATORIO");
        }
        else
        {
            Assert.True(result.IsSuccess);
        }
    }
    [Theory(DisplayName = "Matricula: calcular DataFim por plano (mensal/trimestral/semestral/anual)")]
    [InlineData(MatriculaPlano.Mensal, 1)]
    [InlineData(MatriculaPlano.Trimestral, 3)]
    [InlineData(MatriculaPlano.Semestral, 6)]
    [InlineData(MatriculaPlano.Anual, 12)]
    public void Deve_Calcular_DataFim_Corretamente(MatriculaPlano plano, int meses)
    {
        var aluno = GetValidAluno();
        var inicio = DateOnly.FromDateTime(DateTime.Today);
        var result = Matricula.Criar(1, aluno, plano, inicio, "Objetivo", MatriculaRestricoes.None, null, string.Empty);
        Assert.True(result.IsSuccess);
        Assert.Equal(inicio.AddMonths(meses), result.Value!.DataFim);
    }
    [Theory(DisplayName = "Matricula: restrições -> quando presentes, laudo e observações devem existir")]
    [InlineData(MatriculaRestricoes.None, true)]
    [InlineData(MatriculaRestricoes.Diabetes, true)]
    [InlineData(MatriculaRestricoes.Diabetes | MatriculaRestricoes.Alergias, true)]
    public void Deve_Tratar_Restricoes_ComOuSemLaudo(MatriculaRestricoes restricoes, bool expectSuccess)
    {
        var aluno = GetValidAluno();
        Arquivo? laudo = null;
        string? observacoes = null;
        if (restricoes != MatriculaRestricoes.None)
        {
            laudo = GetValidArquivo();
            observacoes = "Observacoes";
        }
        var result = Matricula.Criar(
        1,
        aluno,
        MatriculaPlano.Mensal,
        DateOnly.FromDateTime(DateTime.Today),
        "Objetivo",
        restricoes,
        laudo,
        observacoes ?? string.Empty
        );
        Assert.Equal(expectSuccess, result.IsSuccess);
    }
    [Theory(DisplayName = "Matricula: menor de 16 anos exige laudo -> MENOR16_LAUDO_OBRIGATORIO")]
    [InlineData(15, true)]
    [InlineData(20, false)]
    public void Deve_Falhar_Criacao_Quando_Menor16_ExigeLaudo(int age, bool expectFailure)
    {
        var aluno = GetValidAluno(DateOnly.FromDateTime(DateTime.Today.AddYears(-age)));
        var result = Matricula.Criar(
        1,
        aluno,
        MatriculaPlano.Mensal,
        DateOnly.FromDateTime(DateTime.Today),
        "Melhorar condicionamento",
        MatriculaRestricoes.None,
        null,
        string.Empty
        );
        Assert.Equal(expectFailure, result.IsFailure);
        if (expectFailure)
        {
            Assert.NotEmpty(result.Notifications);
            Assert.Contains(result.Notifications, n => n.Mensagem == "MENOR_16_LAUDO_OBRIGATORIO");
        }
    }
    [Theory(DisplayName = "Matricula: criação e cálculo de DataFim para planos comuns")]
    [InlineData(MatriculaPlano.Mensal, 1)]
    [InlineData(MatriculaPlano.Trimestral, 3)]
    public void Deve_Criar_Com_Sucesso_E_Calcular_DataFim(MatriculaPlano plano, int meses)
    {
        var aluno = GetValidAluno();
        var inicio = DateOnly.FromDateTime(DateTime.Today);
        var result = Matricula.Criar(
        1,
        aluno,
        plano,
        inicio,
        "Melhorar condicionamento",
        MatriculaRestricoes.None,
        null,
        string.Empty
        );
        Assert.True(result.IsSuccess);
        Assert.Equal(inicio.AddMonths(meses), result.Value!.DataFim);
    }
    [Theory(DisplayName = "Matricula: restrições sem laudo -> RESTRICOES_LAUDO_OBRIGATORIO quando aplicável")]
    [InlineData(true)]
    [InlineData(false)]
    public void Deve_Falhar_Criacao_Quando_RestricoesSemLaudo(bool provideLaudo)
    {
        var aluno = GetValidAluno();
        Arquivo? laudo = provideLaudo ? GetValidArquivo() : null;
        var observacoes = provideLaudo ? "obs" : "";
        var result = Matricula.Criar(
        1,
        aluno,
        MatriculaPlano.Mensal,
        DateOnly.FromDateTime(DateTime.Today),
        "Objetivo",
        MatriculaRestricoes.Diabetes,
        laudo,
        observacoes
        );
        if (provideLaudo)
        {
            Assert.True(result.IsSuccess);
        }
        else
        {
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Notifications);
            Assert.Contains(result.Notifications, n => n.Mensagem == "RESTRICOES_LAUDO_OBRIGATORIO");
        }
    }
    [Theory(DisplayName = "Matricula: normaliza observações de restrições removendo espaços extras")]
    [InlineData(" observa testo ", "observa testo")]
    [InlineData(" obs outro ", "obs outro")]
    public void Deve_Normalizar_ObservacoesRestricoes_Quando_InputTemEspacosExtras(string input, string expected)
    {
        var aluno = GetValidAluno();
        var result = Matricula.Criar(
        1,
        aluno,
        MatriculaPlano.Mensal,
        DateOnly.FromDateTime(DateTime.Today),
        "Objetivo",
        MatriculaRestricoes.None,
        null,
        input
        );
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.ObservacoesRestricoes);
    }
    [Fact(DisplayName = "Matricula: deve criar plano mensal")]
    public void Deve_Criar_Matricula_Com_Plano_Mensal()
    {
        var aluno = Aluno.Criar(
            1,
            "Aluno Teste",
            "52998224725",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "11912345678",
            "aluno@email.com",
            Logradouro.Criar(
                1,
                "12345-678",
                "Rua Teste",
                "Centro",
                "Cidade",
                "SC",
                "Brasil").Value!,
            "10",
            "",
            "Senha123",
            new Arquivo(Array.Empty<byte>())
        ).Value!;

        var dataInicio = DateOnly.FromDateTime(DateTime.Today);

        var result = Matricula.Criar(
            1,
            aluno,
            MatriculaPlano.Mensal,
            dataInicio,
            "Condicionamento",
            MatriculaRestricoes.None,
            null
        );

        Assert.True(result.IsSuccess);
        Assert.Equal(dataInicio.AddMonths(1), result.Value!.DataFim);
    }

    [Fact(DisplayName = "Matricula: deve criar plano trimestral")]
    public void Deve_Criar_Matricula_Com_Plano_Trimestral()
    {
        var aluno = Aluno.Criar(
            1,
            "Aluno Teste",
            "52998224725",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "11912345678",
            "aluno@email.com",
            Logradouro.Criar(
                1,
                "12345-678",
                "Rua Teste",
                "Centro",
                "Cidade",
                "SC",
                "Brasil").Value!,
            "10",
            "",
            "Senha123",
            new Arquivo(Array.Empty<byte>())
        ).Value!;

        var dataInicio = DateOnly.FromDateTime(DateTime.Today);

        var result = Matricula.Criar(
            1,
            aluno,
            MatriculaPlano.Trimestral,
            dataInicio,
            "Condicionamento",
            MatriculaRestricoes.None,
            null
        );

        Assert.True(result.IsSuccess);
        Assert.Equal(dataInicio.AddMonths(3), result.Value!.DataFim);
    }

    [Fact(DisplayName = "Matricula: deve normalizar objetivo")]
    public void Deve_Normalizar_Objetivo_Da_Matricula()
    {
        var aluno = Aluno.Criar(
            1,
            "Aluno Teste",
            "52998224725",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "11912345678",
            "aluno@email.com",
            Logradouro.Criar(
                1,
                "12345-678",
                "Rua Teste",
                "Centro",
                "Cidade",
                "SC",
                "Brasil").Value!,
            "10",
            "",
            "Senha123",
            new Arquivo(Array.Empty<byte>())
        ).Value!;

        var result = Matricula.Criar(
            1,
            aluno,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "  Hipertrofia  ",
            MatriculaRestricoes.None,
            null
        );

        Assert.True(result.IsSuccess);
        Assert.Equal("Hipertrofia", result.Value!.Objetivo);
    }

    [Fact(DisplayName = "Matricula: deve normalizar observações das restrições")]
    public void Deve_Normalizar_Observacoes_Das_Restricoes()
    {
        var aluno = Aluno.Criar(
            1,
            "Aluno Teste",
            "52998224725",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "11912345678",
            "aluno@email.com",
            Logradouro.Criar(
                1,
                "12345-678",
                "Rua Teste",
                "Centro",
                "Cidade",
                "SC",
                "Brasil").Value!,
            "10",
            "",
            "Senha123",
            new Arquivo(Array.Empty<byte>())
        ).Value!;

        var result = Matricula.Criar(
            1,
            aluno,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Hipertrofia",
            MatriculaRestricoes.None,
            null,
            "  Nenhuma observação  "
        );

        Assert.True(result.IsSuccess);
        Assert.Equal(
            "Nenhuma observação",
            result.Value!.ObservacoesRestricoes
        );
    }
}
