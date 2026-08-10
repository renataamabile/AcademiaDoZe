// Renata Amabile Basquerote
using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoAluno : Entity
{
    public Aluno Aluno { get; }
    public DateTime DataHora { get; }

    private AcessoAluno(
        int id,
        Aluno aluno,
        DateTime dataHora)
        : base(id)
    {
        Aluno = aluno;
        DataHora = dataHora;
    }

    public static Result<AcessoAluno> Criar(
        int id,
        Aluno aluno,
        DateTime dataHora)
    {
        var notifications =
            new List<Notification>();

        if (aluno is null)
        {
            notifications.Add(
                new Notification(
                    "Aluno",
                    "ALUNO_OBRIGATORIO"));
        }

        if (dataHora == default)
        {
            notifications.Add(
                new Notification(
                    "DataHora",
                    "DATA_HORA_OBRIGATORIA"));
        }

        if (notifications.Count > 0)
            return Result<AcessoAluno>.Failure(
                notifications);

        return Result<AcessoAluno>.Success(
            new AcessoAluno(
                id,
                aluno!,
                dataHora));
    }
}