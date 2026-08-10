// Renata Amabile Basquerote
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities;

public class Aluno : Pessoa
{
    public bool Instrutor { get; }

    private Aluno(
        int id,
        string nome,
        Cpf cpf,
        DateOnly dataNascimento,
        Telefone telefone,
        Email email,
        Endereco endereco,
        Senha senha,
        Arquivo foto,
        bool instrutor)
        : base(
            id,
            nome,
            cpf,
            dataNascimento,
            telefone,
            email,
            endereco,
            senha,
            foto)
    {
        Instrutor = instrutor;
    }

    public static Result<Aluno> Criar(
        int id,
        string nome,
        string cpf,
        DateOnly dataNascimento,
        string telefone,
        string email,
        Logradouro endereco,
        string numero,
        string complemento,
        string senha,
        Arquivo foto,
        bool instrutor)
    {
        var notifications = new List<Notification>();

        if (NormalizadoService.TextoVazioOuNulo(nome))
        {
            notifications.Add(
                new Notification(
                    "Nome",
                    "NOME_OBRIGATORIO"));
        }
        else
        {
            nome =
                NormalizadoService.LimparEspacos(nome);
        }

        if (dataNascimento == default)
        {
            notifications.Add(
                new Notification(
                    "DataNascimento",
                    "DATA_NASCIMENTO_OBRIGATORIO"));
        }
        else if (
            dataNascimento >
            DateOnly.FromDateTime(
                DateTime.Today.AddYears(-12)))
        {
            notifications.Add(
                new Notification(
                    "DataNascimento",
                    "DATA_NASCIMENTO_MINIMA_INVALIDA"));
        }

        var cpfResult = Cpf.Criar(cpf);

        if (cpfResult.IsFailure)
            notifications.AddRange(
                cpfResult.Notifications);

        var telefoneResult =
            Telefone.Criar(telefone);

        if (telefoneResult.IsFailure)
            notifications.AddRange(
                telefoneResult.Notifications);

        var emailResult =
            Email.Criar(email);

        if (emailResult.IsFailure)
            notifications.AddRange(
                emailResult.Notifications);

        var senhaResult =
            Senha.Criar(senha);

        if (senhaResult.IsFailure)
            notifications.AddRange(
                senhaResult.Notifications);

        var enderecoResult =
            Endereco.Criar(
                endereco,
                numero,
                complemento);

        if (enderecoResult.IsFailure)
            notifications.AddRange(
                enderecoResult.Notifications);

        if (foto is null)
        {
            notifications.Add(
                new Notification(
                    "Foto",
                    "FOTO_OBRIGATORIA"));
        }

        if (notifications.Count > 0)
            return Result<Aluno>.Failure(
                notifications);

        return Result<Aluno>.Success(
            new Aluno(
                id,
                nome,
                cpfResult.Value!,
                dataNascimento,
                telefoneResult.Value!,
                emailResult.Value!,
                enderecoResult.Value!,
                senhaResult.Value!,
                foto,
                instrutor));
    }
}