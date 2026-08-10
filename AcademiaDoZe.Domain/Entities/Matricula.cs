// Renata Amabile Basquerote
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities;

public class Matricula : Entity
{
    public Aluno AlunoMatricula { get; }
    public MatriculaPlano PlanoMatricula { get; }
    public DateOnly DataInicio { get; }
    public DateOnly DataFim { get; }
    public string Objetivo { get; }
    public MatriculaRestricoes RestricoesMedicas { get; }
    public string ObservacoesRestricoes { get; }
    public Arquivo? LaudoMedico { get; }

    private Matricula(
        int id,
        Aluno alunoMatricula,
        MatriculaPlano planoMatricula,
        DateOnly dataInicio,
        DateOnly dataFim,
        string objetivo,
        MatriculaRestricoes restricoesMedicas,
        string observacoesRestricoes,
        Arquivo? laudoMedico)
        : base(id)
    {
        AlunoMatricula = alunoMatricula;
        PlanoMatricula = planoMatricula;
        DataInicio = dataInicio;
        DataFim = dataFim;
        Objetivo = objetivo;
        RestricoesMedicas = restricoesMedicas;
        ObservacoesRestricoes = observacoesRestricoes;
        LaudoMedico = laudoMedico;
    }

    public static Result<Matricula> Criar(
        int id,
        Aluno alunoMatricula,
        MatriculaPlano planoMatricula,
        DateOnly dataInicio,
        DateOnly dataFim,
        string objetivo,
        MatriculaRestricoes restricoesMedicas,
        string observacoesRestricoes = "",
        Arquivo? laudoMedico = null)
    {
        var notifications = new List<Notification>();

        if (alunoMatricula is null)
        {
            notifications.Add(
                new Notification(
                    "AlunoMatricula",
                    "ALUNO_OBRIGATORIO"));
        }

        if (!Enum.IsDefined(planoMatricula))
        {
            notifications.Add(
                new Notification(
                    "PlanoMatricula",
                    "PLANO_INVALIDO"));
        }

        if (dataInicio == default)
        {
            notifications.Add(
                new Notification(
                    "DataInicio",
                    "DATA_INICIO_OBRIGATORIA"));
        }

        if (dataFim == default)
        {
            notifications.Add(
                new Notification(
                    "DataFim",
                    "DATA_FIM_OBRIGATORIA"));
        }

        if (
            dataInicio != default &&
            dataFim != default &&
            dataFim < dataInicio)
        {
            notifications.Add(
                new Notification(
                    "DataFim",
                    "DATA_FIM_MENOR_DATA_INICIO"));
        }

        if (NormalizadoService.TextoVazioOuNulo(objetivo))
        {
            notifications.Add(
                new Notification(
                    "Objetivo",
                    "OBJETIVO_OBRIGATORIO"));
        }
        else
        {
            objetivo =
                NormalizadoService.LimparEspacos(
                    objetivo);
        }

        if (!Enum.IsDefined(restricoesMedicas))
        {
            notifications.Add(
                new Notification(
                    "RestricoesMedicas",
                    "RESTRICOES_INVALIDAS"));
        }

        observacoesRestricoes =
            NormalizadoService.LimparEspacos(
                observacoesRestricoes);

        if (
            alunoMatricula is not null &&
            dataInicio != default)
        {
            var idade =
                CalcularIdade(
                    alunoMatricula.DataNascimento,
                    dataInicio);

            if (idade >= 12 &&
                idade <= 16 &&
                laudoMedico is null)
            {
                notifications.Add(
                    new Notification(
                        "LaudoMedico",
                        "LAUDO_OBRIGATORIO_MENOR"));
            }
        }

        if (
            Enum.IsDefined(restricoesMedicas) &&
            restricoesMedicas != MatriculaRestricoes.None &&
            laudoMedico is null)
        {
            notifications.Add(
                new Notification(
                    "LaudoMedico",
                    "LAUDO_OBRIGATORIO_RESTRICAO"));
        }

        if (notifications.Count > 0)
            return Result<Matricula>.Failure(
                notifications);

        return Result<Matricula>.Success(
            new Matricula(
                id,
                alunoMatricula!,
                planoMatricula,
                dataInicio,
                dataFim,
                objetivo,
                restricoesMedicas,
                observacoesRestricoes,
                laudoMedico));
    }

    private static int CalcularIdade(
        DateOnly nascimento,
        DateOnly dataReferencia)
    {
        var idade =
            dataReferencia.Year -
            nascimento.Year;

        if (dataReferencia <
            nascimento.AddYears(idade))
        {
            idade--;
        }

        return idade;
    }
}