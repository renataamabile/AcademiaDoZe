// Renata Amabile Basquerote

using AcademiaDoZe.Infrastructure.Data;
using System.Data.Common;

[assembly: CollectionBehavior(
    CollectionBehavior.CollectionPerAssembly,
    DisableTestParallelization = true)]

namespace AcademiaDoZe.Infrastructure.Tests;

public abstract class TestBase : IDisposable
{
    // ============================================================
    // CONFIGURAÇÃO DO BANCO
    // ============================================================

    // Os testes utilizarão SQLite.
    // É o mesmo banco utilizado pelo Windows Machine.
    private const DatabaseType SelectedDatabaseType =
        DatabaseType.Sqlite;

    protected string ConnectionString { get; }

    protected DatabaseType DatabaseType { get; }


    // ============================================================
    // CONTROLE DOS DADOS CRIADOS PELOS TESTES
    // ============================================================

    // Alunos serão identificados pelo CPF.
    private readonly List<string> _cpfsAlunosCriados = new();

    // Matrículas serão identificadas pelo ID.
    private readonly List<int> _idsMatriculasCriadas = new();

    // Colaboradores serão identificados pelo ID.
    private readonly List<int> _idsColaboradoresCriados = new();

    // Logradouros serão identificados pelo ID.
    private readonly List<int> _idsLogradourosCriados = new();


    // ============================================================
    // CONSTRUTOR
    // ============================================================

    protected TestBase()
    {
        DatabaseType = SelectedDatabaseType;

        ConnectionString = DatabaseType switch
        {
            DatabaseType.SqlServer =>
                "Server=localhost,1433;" +
                "Database=db_academia_do_ze;" +
                "User Id=sa;" +
                "Password=abcBolinhas12345;" +
                "TrustServerCertificate=True;" +
                "Encrypt=False;",

            DatabaseType.MySql =>
                "Server=localhost;" +
                "Port=3307;" +
                "Database=db_academia_do_ze;" +
                "User Id=root;" +
                "Password=abcBolinhas12345;",

            DatabaseType.Sqlite =>
                ObterConnectionStringSqlite(),

            _ => throw new ArgumentOutOfRangeException(
                nameof(DatabaseType),
                DatabaseType,
                "SGBD não suportado para testes.")
        };
    }


    // ============================================================
    // SQLITE
    // ============================================================

    private static string ObterConnectionStringSqlite()
    {
        var userProfile =
            Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile);

        var dbPath =
            Path.Combine(
                userProfile,
                "AcademiaDoZe",
                "db_academia_do_ze.db");

        if (!File.Exists(dbPath))
        {
            throw new FileNotFoundException(
                "O banco SQLite utilizado pelos testes não foi " +
                $"encontrado em:\n{dbPath}");
        }

        return
            $"Data Source={dbPath};" +
            "Cache=Shared;" +
            "Default Timeout=30;";
    }


    // ============================================================
    // REGISTRO DOS DADOS CRIADOS
    // ============================================================

    protected void RegistrarAlunoCriado(string cpf)
    {
        if (!string.IsNullOrWhiteSpace(cpf) &&
            !_cpfsAlunosCriados.Contains(cpf))
        {
            _cpfsAlunosCriados.Add(cpf);
        }
    }


    protected void RegistrarMatriculaCriada(int idMatricula)
    {
        if (idMatricula > 0 &&
            !_idsMatriculasCriadas.Contains(idMatricula))
        {
            _idsMatriculasCriadas.Add(idMatricula);
        }
    }


    protected void RegistrarColaboradorCriado(int idColaborador)
    {
        if (idColaborador > 0 &&
            !_idsColaboradoresCriados.Contains(idColaborador))
        {
            _idsColaboradoresCriados.Add(idColaborador);
        }
    }


    protected void RegistrarLogradouroCriado(int idLogradouro)
    {
        if (idLogradouro > 0 &&
            !_idsLogradourosCriados.Contains(idLogradouro))
        {
            _idsLogradourosCriados.Add(idLogradouro);
        }
    }


    // ============================================================
    // DISPOSE
    // ============================================================

    public void Dispose()
    {
        LimparDadosCriados();
    }


    // ============================================================
    // LIMPEZA DOS DADOS CRIADOS PELO TESTE
    // ============================================================

    private void LimparDadosCriados()
    {
        if (_idsMatriculasCriadas.Count == 0 &&
            _cpfsAlunosCriados.Count == 0 &&
            _idsColaboradoresCriados.Count == 0 &&
            _idsLogradourosCriados.Count == 0)
        {
            return;
        }

        try
        {
            // IMPORTANTE:
            // A ordem abaixo respeita as possíveis
            // dependências entre as tabelas.

            using var connection =
                DbProvider.CreateConnection(
                    ConnectionString,
                    DatabaseType);

            connection.Open();


            // ====================================================
            // 1. MATRÍCULAS
            // ====================================================

            foreach (var idMatricula in _idsMatriculasCriadas)
            {
                ExecutarDelete(
                    connection,
                    "DELETE FROM tb_matricula " +
                    "WHERE id_matricula = @id;",
                    "@id",
                    idMatricula);
            }


            // ====================================================
            // 2. ALUNOS
            // ====================================================

            foreach (var cpf in _cpfsAlunosCriados)
            {
                ExecutarDelete(
                    connection,
                    "DELETE FROM tb_aluno " +
                    "WHERE cpf = @cpf;",
                    "@cpf",
                    cpf);
            }


            // ====================================================
            // 3. COLABORADORES
            // ====================================================

            foreach (var idColaborador in _idsColaboradoresCriados)
            {
                ExecutarDelete(
                    connection,
                    "DELETE FROM tb_colaborador " +
                    "WHERE id_colaborador = @id;",
                    "@id",
                    idColaborador);
            }


            // ====================================================
            // 4. LOGRADOUROS
            // ====================================================

            foreach (var idLogradouro in _idsLogradourosCriados)
            {
                ExecutarDelete(
                    connection,
                    "DELETE FROM tb_logradouro " +
                    "WHERE id_logradouro = @id;",
                    "@id",
                    idLogradouro);
            }
        }
        catch (Exception ex)
        {
            // A limpeza não deve transformar um teste aprovado
            // em teste com falha.
            //
            // Porém, mostramos o problema no Output para que
            // seja possível identificar caso algum registro
            // permaneça no banco.

            System.Diagnostics.Debug.WriteLine(
                $"ERRO AO LIMPAR DADOS DO TESTE: {ex.Message}");
        }
        finally
        {
            // Limpa as listas mesmo após a tentativa de exclusão.
            _idsMatriculasCriadas.Clear();
            _cpfsAlunosCriados.Clear();
            _idsColaboradoresCriados.Clear();
            _idsLogradourosCriados.Clear();
        }
    }


    // ============================================================
    // EXECUÇÃO DOS DELETEs
    // ============================================================

    private static void ExecutarDelete(
        DbConnection connection,
        string sql,
        string parameterName,
        object parameterValue)
    {
        try
        {
            // ATENÇÃO:
            // A ordem correta é:
            // CreateCommand(SQL, CONNECTION)

            using var command =
                DbProvider.CreateCommand(
                    sql,
                    connection);

            var parameter =
                command.CreateParameter();

            parameter.ParameterName =
                parameterName;

            parameter.Value =
                parameterValue;

            command.Parameters.Add(parameter);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"ERRO AO EXECUTAR DELETE: {ex.Message}");
        }
    }


    // ============================================================
    // GERADORES DE DADOS ALEATÓRIOS
    // ============================================================

    private static int _counter = 10000;


    // ============================================================
    // CEP
    // ============================================================

    protected static string GerarCep()
    {
        var numero =
            80000000 +
            ((int)(DateTime.UtcNow.Ticks % 8000000)) +
            Interlocked.Increment(ref _counter);

        return numero
            .ToString("D8")[..8];
    }


    // ============================================================
    // CPF
    // ============================================================

    protected static string GerarCpf()
    {
        var numero =
            Interlocked.Increment(ref _counter);

        // Gera os 9 primeiros dígitos.
        var baseCpf =
            (100000000 + numero)
            .ToString("D9");

        // Primeiro dígito verificador.
        int soma = 0;

        for (int i = 0; i < 9; i++)
        {
            soma +=
                int.Parse(baseCpf[i].ToString()) *
                (10 - i);
        }

        int digito1 =
            (soma * 10) % 11;

        if (digito1 == 10)
            digito1 = 0;


        // Segundo dígito verificador.
        soma = 0;

        for (int i = 0; i < 9; i++)
        {
            soma +=
                int.Parse(baseCpf[i].ToString()) *
                (11 - i);
        }

        soma +=
            digito1 * 2;

        int digito2 =
            (soma * 10) % 11;

        if (digito2 == 10)
            digito2 = 0;

        return
            $"{baseCpf}{digito1}{digito2}";
    }


    // ============================================================
    // TELEFONE
    // ============================================================

    protected static string GerarTelefone()
    {
        var numero =
            Interlocked.Increment(
                ref _counter);

        var sufixo =
            (10000000 +
             (numero % 90000000))
            .ToString("D8");

        return
            $"419{sufixo}";
    }


    // ============================================================
    // E-MAIL
    // ============================================================

    protected static string GerarEmail()
    {
        return
            $"teste_{Guid.NewGuid():N}@exemplo.com";
    }
}