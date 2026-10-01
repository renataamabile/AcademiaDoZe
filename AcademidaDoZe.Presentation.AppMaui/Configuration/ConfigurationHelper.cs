using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

public static class ConfigurationHelper
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Banco utilizado pela aplicação
        var databaseType = AppDatabaseType.Sqlite;

        string connectionString;

        if (databaseType == AppDatabaseType.Sqlite)
        {
            // Banco SQLite utilizado pelo Windows Machine
            var userProfile = Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile);

            var dbPath = Path.Combine(
                userProfile,
                "AcademiaDoZe",
                "db_academia_do_ze.db");

            // Verificação para evitar erro silencioso
            if (!File.Exists(dbPath))
            {
                throw new FileNotFoundException(
                    $"Banco SQLite não encontrado em: {dbPath}");
            }

            connectionString =
                $"Data Source={dbPath};Default Timeout=30;";
        }
        else
        {
            const string dbServer = "10.30.21.16";
            const string dbDatabase = "db_academia_do_ze";
            const string dbUser = "root";
            const string dbPassword = "abcBolinhas12345";

            string dbComplemento = string.Empty;

            if (databaseType == AppDatabaseType.SqlServer)
            {
                dbComplemento =
                    "TrustServerCertificate=True;" +
                    "Encrypt=True;" +
                    "Connect Timeout=5;" +
                    "Connection Timeout=5;";
            }
            else if (databaseType == AppDatabaseType.MySql)
            {
                dbComplemento =
                    "Connection Timeout=5;" +
                    "Default Command Timeout=30;";
            }

            connectionString =
                $"Server={dbServer};" +
                $"Database={dbDatabase};" +
                $"User Id={dbUser};" +
                $"Password={dbPassword};" +
                dbComplemento;
        }

        services.AddSingleton(new RepositoryConfig
        {
            ConnectionString = connectionString,
            DatabaseType = databaseType.ToInfrastructure()
        });

        services.AddApplicationServices();
    }
}