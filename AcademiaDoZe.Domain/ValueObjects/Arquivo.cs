// Renata Amabile Basquerote
using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Arquivo
{
    public byte[] Conteudo { get; }

    private Arquivo(byte[] conteudo)
    {
        Conteudo = conteudo.ToArray();
    }

    public static Result<Arquivo> Criar(byte[] conteudo)
    {
        if (conteudo is null)
            return Result<Arquivo>.Failure(
                "Arquivo",
                "ARQUIVO_OBRIGATORIO");

        const int tamanhoMaximoBytes = 15 * 1024 * 1024;

        if (conteudo.Length > tamanhoMaximoBytes)
            return Result<Arquivo>.Failure(
                "Arquivo",
                "ARQUIVO_TAMANHO");

        return Result<Arquivo>.Success(
            new Arquivo(conteudo));
    }
}