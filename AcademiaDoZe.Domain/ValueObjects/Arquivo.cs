// Renata Amabile Basquerote
using System;
using System.Collections.Generic;
using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.ValueObjects;

public sealed class Arquivo
{
    public byte[] Conteudo { get; }

    public Arquivo(byte[] conteudo)
    {
        Conteudo = conteudo ?? Array.Empty<byte>();
    }

    public static Result<Arquivo> Criar(byte[] conteudo)
    {
        var notifications = new List<Notification>();
        if (conteudo == null || conteudo.Length == 0)
            notifications.Add(new Notification("Conteudo", "ARQUIVO_VAZIO"));
        if (notifications.Count != 0)
            return Result<Arquivo>.Failure(notifications);
        return Result<Arquivo>.Success(new Arquivo(conteudo ?? Array.Empty<byte>()));
    }
}
