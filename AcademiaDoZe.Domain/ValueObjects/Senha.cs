// Renata Amabile Basquerote
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Senha
{
    public string ChaveAcesso { get; }

    private Senha(string senha)
    {
        ChaveAcesso = senha;
    }

    public static Result<Senha> Criar(string senha)
    {
        var notifications = new List<Notification>();

        if (NormalizadoService.TextoVazioOuNulo(senha))
        {
            notifications.Add(
                new Notification("Senha", "SENHA_OBRIGATORIA")
            );
        }
        else
        {
            senha = NormalizadoService.LimparEspacos(senha);
        }

        if (notifications.Count != 0)
            return Result<Senha>.Failure(notifications);

        return Result<Senha>.Success(new Senha(senha));
    }
}