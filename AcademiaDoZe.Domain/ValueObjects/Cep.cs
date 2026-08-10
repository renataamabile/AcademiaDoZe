// Renata Amabile Basquerote
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Cep
{
    public string Valor { get; }

    private Cep(string valor)
    {
        Valor = valor;
    }

    public static Result<Cep> Criar(string valor)
    {
        if (NormalizadoService.TextoVazioOuNulo(valor))
            return Result<Cep>.Failure(
                "Cep",
                "CEP_OBRIGATORIO");

        var cepNormalizado =
            NormalizadoService.LimparEDigitos(valor);

        if (cepNormalizado.Length != 8)
            return Result<Cep>.Failure(
                "Cep",
                "CEP_DIGITOS");

        return Result<Cep>.Success(
            new Cep(cepNormalizado));
    }

    public override string ToString() => Valor;
}