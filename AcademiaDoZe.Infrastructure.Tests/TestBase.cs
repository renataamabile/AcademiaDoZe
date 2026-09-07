// Renata Amabile Basquerote
using AcademiaDoZe.Infrastructure.Data;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]

namespace AcademiaDoZe.Infrastructure.Tests;

public abstract class TestBase
{
    // Alterne o SGBD alvo dos testes trocando apenas a constante abaixo:
    private const DatabaseType SelectedDatabaseType = DatabaseType.MySql;

    protected string ConnectionString { get; }
    protected DatabaseType DatabaseType { get; }

    protected TestBase()
    {
        DatabaseType = SelectedDatabaseType;

        // Ajuste a ConnectionString com caminhos e credenciais válidas
        ConnectionString = DatabaseType switch
        {
            DatabaseType.SqlServer =>
                 "Server=localhost,1433;Database=db_academia_do_ze;User Id=sa;Password=abcBolinhas12345;TrustServerCertificate=True;Encrypt=False;",

            DatabaseType.MySql =>
                 "Server=localhost;Port=3307;Database=db_academia_do_ze;User Id=root;Password=abcBolinhas12345;",

            DatabaseType.Sqlite =>
                 $"Data Source={Path.Combine(AppContext.BaseDirectory, "db_academia_do_ze.db")};Cache=Shared;",

            _ => throw new ArgumentOutOfRangeException(
                nameof(DatabaseType),
                DatabaseType,
                "SGBD não suportado para testes.")
        };
    }

    #region Geradores de dados aleatórios

    private static int _counter = 10000;

    protected static string GerarCep() =>
        (80000000 +
         ((int)(DateTime.UtcNow.Ticks % 8000000)) +
         Interlocked.Increment(ref _counter))
        .ToString("D8")[..8];

    protected static string GerarCpf()
    {
        // Gera 9 dígitos praticamente únicos
        var bytes = Guid.NewGuid().ToByteArray();
        var numero = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 900000000 + 100000000;

        var cpfBase = numero.ToString("D9");

        // Primeiro dígito verificador
        var soma = 0;

        for (int i = 0; i < 9; i++)
            soma += (cpfBase[i] - '0') * (10 - i);

        var resto = soma % 11;
        var digito1 = resto < 2 ? 0 : 11 - resto;

        // Segundo dígito verificador
        soma = 0;

        for (int i = 0; i < 9; i++)
            soma += (cpfBase[i] - '0') * (11 - i);

        soma += digito1 * 2;

        resto = soma % 11;
        var digito2 = resto < 2 ? 0 : 11 - resto;

        return $"{cpfBase}{digito1}{digito2}";
    }

    protected static string GerarTelefone()
    {
        var numero = Interlocked.Increment(ref _counter);
        // Gera número celular com DDD 41 + 9 + 8 dígitos (ex.: 419XXXXXXXX)
        var sufixo = (10000000 + (numero % 90000000)).ToString("D8");
        return $"41{9}{sufixo}";
    }

    protected static string GerarEmail()
    {
        return $"teste_{Guid.NewGuid():N}@exemplo.com";
    }

    #endregion
}
