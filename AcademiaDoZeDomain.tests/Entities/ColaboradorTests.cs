//Renata Amabile Basquerote
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using Xunit;
namespace AcademiaDoZe.Domain.Tests.Entities;

public class ColaboradorTests
{
    private static Logradouro GetValidLogradouro() => Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;
    private static Arquivo GetValidArquivo() => Arquivo.Criar(new byte[] { 1, 2, 3 }).Value!;
    private static Colaborador GetValidColaborador() => Colaborador.Criar(1, "Fulano", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", string.Empty, "Abcdef", GetValidArquivo(),
    DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
    ColaboradorTipo.Atendente,
    ColaboradorVinculo.Clt
    ).Value!;
    [Theory(DisplayName = "AcessoColaborador: colaborador nulo -> COLABORADOR_INVALIDO")]
    [InlineData(true)]
    [InlineData(false)]
    public void Deve_Falhar_Criacao_Quando_ColaboradorEhNulo(bool colaboradorNull)
    {
        var colaborador = colaboradorNull ? null : GetValidColaborador();
        var result = AcessoColaborador.Criar(1, colaborador!, DateTime.Today.AddHours(10));
        if (colaboradorNull)
        {
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Notifications);
            Assert.Contains(result.Notifications, n => n.Mensagem == "COLABORADOR_INVALIDO");
        }
        else
        {
            Assert.True(result.IsSuccess);
        }
    }
    [Theory(DisplayName = "AcessoColaborador: horario fora do intervalo -> DATAHORA_INTERVALO")]
    [InlineData(5, 59)]
    [InlineData(22, 1)]
    public void Deve_Falhar_Criacao_Quando_HorarioForaDoIntervalo(int hour, int minute)
    {
        var colaborador = GetValidColaborador();
        var dataHora = DateTime.Today.AddHours(hour).AddMinutes(minute);
        var result = AcessoColaborador.Criar(1, colaborador, dataHora);
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "DATA_HORA_INTERVALO_INVALIDO");
    }
    [Theory(DisplayName = "AcessoColaborador: criação bem-sucedida em horários permitidos")]
    [InlineData(10)]
    [InlineData(14)]
    public void Deve_Criar_Com_Sucesso_Quando_HorarioValido(int hour)
    {
        var colaborador = GetValidColaborador();
        var dataHora = DateTime.Today.AddHours(hour); // 10:00 or 14:00
        var result = AcessoColaborador.Criar(1, colaborador, dataHora);
        Assert.True(result.IsSuccess);
        Assert.Equal(colaborador.Id, result.Value!.ColaboradorId);
        Assert.Equal(dataHora, result.Value.DataHora);
    }
    [Theory(DisplayName = "AcessoColaborador: permite horários de borda 06:00 e 22:00")]
    [InlineData(6)]
    [InlineData(22)]
    public void Deve_Permitir_HorariosDeBorda_06_00_e_22_00(int hour)
    {
        var colaborador = GetValidColaborador();
        var time = DateTime.Today.AddHours(hour);
        var r = AcessoColaborador.Criar(1, colaborador, time);
        Assert.True(r.IsSuccess);
    }

    [Fact(DisplayName = "Colaborador: deve criar quando data de admissão for a data atual")]
    public void Deve_Criar_Colaborador_Quando_DataAdmissao_ForHoje()
    {
        var logradouro = Logradouro.Criar(
            1,
            "12345-678",
            "Rua Teste",
            "Centro",
            "Cidade",
            "SC",
            "Brasil").Value!;

        var result = Colaborador.Criar(
            1,
            "Colaborador Teste",
            "52998224725",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "11912345678",
            "colaborador@email.com",
            logradouro,
            "10",
            "",
            "Senha123",
            GetValidArquivo(),
            DateOnly.FromDateTime(DateTime.Today),
            ColaboradorTipo.Administrador,
            ColaboradorVinculo.Clt
        );

        Assert.True(result.IsSuccess);
    }

    [Fact(DisplayName = "Colaborador: deve falhar quando data de admissão for futura")]
    public void Deve_Falhar_Quando_DataAdmissao_ForFutura()
    {
        var logradouro = Logradouro.Criar(
            1,
            "12345-678",
            "Rua Teste",
            "Centro",
            "Cidade",
            "SC",
            "Brasil").Value!;

        var dataAdmissao = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        var result = Colaborador.Criar(
            1,
            "Colaborador Teste",
            "52998224725",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "11912345678",
            "colaborador@email.com",
            logradouro,
            "10",
            "",
            "Senha123",
            GetValidArquivo(),
            dataAdmissao,
            ColaboradorTipo.Professor,
            ColaboradorVinculo.Clt
        );

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications,
            n => n.Mensagem == "DATA_ADMISSAO_MAIOR_QUE_ATUAL");
    }

    [Fact(DisplayName = "Colaborador: deve falhar quando tipo for inválido")]
    public void Deve_Falhar_Quando_Tipo_ForInvalido()
    {
        var logradouro = Logradouro.Criar(
            1,
            "12345-678",
            "Rua Teste",
            "Centro",
            "Cidade",
            "SC",
            "Brasil").Value!;

        var result = Colaborador.Criar(
            1,
            "Colaborador Teste",
            "52998224725",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "11912345678",
            "colaborador@email.com",
            logradouro,
            "10",
            "",
            "Senha123",
            GetValidArquivo(),
            DateOnly.FromDateTime(DateTime.Today),
            (ColaboradorTipo)999,
            ColaboradorVinculo.Clt
        );

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications,
            n => n.Mensagem == "TIPO_COLABORADOR_INVALIDO");
    }

    [Fact(DisplayName = "Colaborador: administrador deve possuir vínculo CLT")]
    public void Deve_Falhar_Quando_Administrador_Nao_ForClt()
    {
        var logradouro = Logradouro.Criar(
            1,
            "12345-678",
            "Rua Teste",
            "Centro",
            "Cidade",
            "SC",
            "Brasil").Value!;

        var result = Colaborador.Criar(
            1,
            "Colaborador Teste",
            "52998224725",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            "11912345678",
            "colaborador@email.com",
            logradouro,
            "10",
            "",
            "Senha123",
            Arquivo.Criar(Array.Empty<byte>()).Value!,
            DateOnly.FromDateTime(DateTime.Today),
            ColaboradorTipo.Administrador,
            (ColaboradorVinculo)999
        );

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications,
            n => n.Mensagem == "VINCULO_ADMINISTRADOR_INVALIDO");
    }
}
