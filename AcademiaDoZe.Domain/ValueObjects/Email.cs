// Renata Amabile Basquerote
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Email
{
    public string Valor { get; }

    private Email(string valor)
    {
        Valor = valor;
    }

    public static Result<Email> Criar(string valor)
    {
        var emailNormalizado =
            NormalizadoService.LimparEspacos(valor);

        if (string.IsNullOrWhiteSpace(emailNormalizado))
            return Result<Email>.Failure(
                "Email",
                "EMAIL_OBRIGATORIO");

        if (!ValidarFormato(emailNormalizado))
            return Result<Email>.Failure(
                "Email",
                "EMAIL_FORMATO");

        return Result<Email>.Success(
            new Email(emailNormalizado));
    }

    private static bool ValidarFormato(string email)
    {
        var partes = email.Split('@');

        if (partes.Length != 2)
            return false;

        if (string.IsNullOrWhiteSpace(partes[0]))
            return false;

        var dominio = partes[1];

        if (string.IsNullOrWhiteSpace(dominio))
            return false;

        if (dominio.StartsWith('.') ||
            dominio.EndsWith('.'))
            return false;

        var partesDominio = dominio.Split('.');

        if (partesDominio.Length < 2)
            return false;

        if (partesDominio.Any(
            parte => string.IsNullOrWhiteSpace(parte)))
            return false;

        return true;
    }

    public override string ToString() => Valor;
}