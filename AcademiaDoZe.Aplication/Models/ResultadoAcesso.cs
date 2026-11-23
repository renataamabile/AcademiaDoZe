// Renata Amabile Basquerote
namespace AcademiaDoZe.Application.Models;

public class ResultadoAcesso
{
    public string Mensagem { get; set; } = string.Empty;

    public byte[]? Foto { get; set; }

    public string NomePessoa { get; set; } = string.Empty;
}