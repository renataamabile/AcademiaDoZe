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
    public static Result<Cpf> Criar(string valor)
    {
        if (NormalizacaoService.TextoVazioOuNulo(valor))
            return Result<Cpf>.Failure("Cpf", "CPF_OBRIGATORIO");
        var textoLimpo = NormalizacaoService.LimparEDigitos(valor);
        if (textoLimpo.Length != 11)
            return Result<Cpf>.Failure("Cpf", "CPF_DIGITOS");
        if (!Validar(textoLimpo))
            return Result<Cpf>.Failure("Cpf", "CPF_INVALIDO");
        return Result<Cpf>.Success(new Cpf(textoLimpo));
    }
    private static bool Validar(string cpf)
    {
        if (cpf.Length != 11) return false;
        // invalid if all digits are the same
        if (cpf.Distinct().Count() == 1) return false;

        int[] numbers = cpf.Select(c => c - '0').ToArray();

        // first verifier digit
        int sum = 0;
        for (int i = 0; i < 9; i++) sum += numbers[i] * (10 - i);
        int remainder = sum % 11;
        int firstDigit = remainder < 2 ? 0 : 11 - remainder;
        if (numbers[9] != firstDigit) return false;

        // second verifier digit
        sum = 0;
        for (int i = 0; i < 10; i++) sum += numbers[i] * (11 - i);
        remainder = sum % 11;
        int secondDigit = remainder < 2 ? 0 : 11 - remainder;
        if (numbers[10] != secondDigit) return false;

        return true;
    }
    public override string ToString() => Valor;

}