//Renata Amabile Basquerote
using AcademiaDoZe.Domain.Entities;
using Xunit;
namespace AcademiaDoZe.Domain.Tests.Entities;

public class LogradouroTests
{
    [Theory(DisplayName = "Logradouro: nome vazio -> NOME_OBRIGATORIO")]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_NomeVazio(string nome)
    {
        var result = Logradouro.Criar(1, "12345-678", nome, "Bairro", "Cidade", "SP", "Brasil");
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "NOME_OBRIGATORIO");
    }
    [Theory(DisplayName = "Logradouro: normaliza estado removendo espaços e upper")]
    [InlineData(" s p ", "SP")]
    [InlineData(" sp ", "SP")]
    public void Deve_Normalizar_Estado_Quando_InputContemEspacos(string inputEstado, string expected)
    {
        var result = Logradouro.Criar(1, "12345-678", "Rua", "Bairro", "Cidade", inputEstado, "Brasil");
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.Estado);
    }
    [Theory(DisplayName = "Logradouro: campos obrigatórios vazios -> mensagens específicas")]
    [InlineData("", "", "", "", "BAIRRO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_CamposObrigatoriosVazios(string rua, string bairro, string cidade, string estado, string expected)
    {
        var result = Logradouro.Criar(1, "12345-678", rua, bairro, cidade, estado, "");
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == expected);
    }

    [Theory(DisplayName = "Logradouro: deve normalizar nome, bairro e cidade")]
    [InlineData("  Rua Teste  ", "  Centro  ", "  Lages  ")]
    public void Deve_Normalizar_Nome_Bairro_E_Cidade(
    string nome,
    string bairro,
    string cidade)
    {
        var result = Logradouro.Criar(
            1,
            "88501-000",
            nome,
            bairro,
            cidade,
            "SC",
            "Brasil"
        );

        Assert.True(result.IsSuccess);
        Assert.Equal("Rua Teste", result.Value!.Nome);
        Assert.Equal("Centro", result.Value.Bairro);
        Assert.Equal("Lages", result.Value.Cidade);
    }

    [Fact(DisplayName = "Logradouro: deve normalizar estado para maiúsculo e remover espaços")]
    public void Deve_Normalizar_Estado()
    {
        var result = Logradouro.Criar(
            1,
            "88501-000",
            "Rua Teste",
            "Centro",
            "Lages",
            " s c ",
            "Brasil"
        );

        Assert.True(result.IsSuccess);
        Assert.Equal("SC", result.Value!.Estado);
    }
}
