// Renata Amabile Basquerote
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Cpf
{
    public string Valor { get; }

    private Cpf(string valor)
    {
        Valor = valor;
    }

    public static Result<Cpf> Criar(string cpf)
    {
        if (NormalizadoService.TextoVazioOuNulo(cpf))
            return Result<Cpf>.Failure(
                "Cpf",
                "CPF_OBRIGATORIO");

        var cpfNormalizado = NormalizadoService.LimparEDigitos(cpf);

        if (cpfNormalizado.Length != 11)
            return Result<Cpf>.Failure(
                "Cpf",
                "CPF_DIGITOS");

        return Result<Cpf>.Success(
            new Cpf(cpfNormalizado));
    }

    public override string ToString() => Valor;
}