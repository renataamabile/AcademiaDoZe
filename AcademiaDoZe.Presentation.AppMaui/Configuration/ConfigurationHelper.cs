using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

public static class ConfigurationHelper
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var (connectionString, databaseType) =
            ObterConfiguracaoAtual();

        var repoConfig = new RepositoryConfig
        {
            ConnectionString = connectionString,
            DatabaseType = databaseType.ToInfrastructure()
        };

        // Configura a fábrica de repositórios com a string de conexão
        // e o tipo de banco.
        services.AddSingleton(repoConfig);

        // Atualiza o RepositoryConfig quando as preferências
        // do banco forem alteradas.
        WeakReferenceMessenger.Default.Register<
            RepositoryConfig,
            BancoPreferencesUpdatedMessage>(
                repoConfig,
                (r, m) =>
                {
                    var (novaConnStr, novoDbType) =
                        ObterConfiguracaoAtual();

                    r.ConnectionString = novaConnStr;
                    r.DatabaseType = novoDbType.ToInfrastructure();
                });

        // Configura os serviços da camada de aplicação.
        services.AddApplicationServices();
    }


    /// <summary>
    /// Obtém a Connection String e o AppDatabaseType ativos
    /// a partir das Preferences do usuário.
    /// Cada tipo de banco utiliza somente suas próprias configurações.
    /// </summary>
    public static (
        string ConnectionString,
        AppDatabaseType DatabaseType)
        ObterConfiguracaoAtual()
    {
        var databaseTypeStr =
            Preferences.Get(
                "DatabaseType",
                AppDatabaseType.Sqlite.ToString());

        if (!Enum.TryParse<AppDatabaseType>(
            databaseTypeStr,
            out var databaseType))
        {
            databaseType =
                AppDatabaseType.Sqlite;
        }


        // =========================================================
        // SQLITE
        // =========================================================

        if (databaseType == AppDatabaseType.Sqlite)
        {
            var defaultDbPath =
                DeviceInfo.Platform == DevicePlatform.WinUI
                    ? @"C:\DEV\AcademiaDoZe\db_academia_do_ze.db"
                    : Path.Combine(
                        FileSystem.AppDataDirectory,
                        "db_academia_do_ze.db");


            var dbPath =
                Preferences.Get(
                    "Sqlite_Caminho",
                    defaultDbPath);


            if (string.IsNullOrWhiteSpace(dbPath))
            {
                dbPath = defaultDbPath;
            }


            // SQLite possui seu próprio complemento.
            // Não utiliza o complemento do MySQL ou SQL Server.
            var complemento =
                Preferences.Get(
                    "Sqlite_Complemento",
                    "Default Timeout=5;");


            // Se uma versão antiga do aplicativo tiver salvo
            // parâmetros do MySQL no SQLite, corrige automaticamente.
            if (string.IsNullOrWhiteSpace(complemento) ||
                complemento.Contains(
                    "Port=",
                    StringComparison.OrdinalIgnoreCase) ||
                complemento.Contains(
                    "SslMode=",
                    StringComparison.OrdinalIgnoreCase) ||
                complemento.Contains(
                    "AllowPublicKeyRetrieval",
                    StringComparison.OrdinalIgnoreCase))
            {
                complemento =
                    "Default Timeout=5;";

                Preferences.Set(
                    "Sqlite_Complemento",
                    complemento);
            }


            return (
                $"Data Source={dbPath};{complemento}",
                AppDatabaseType.Sqlite);
        }


        // =========================================================
        // MYSQL
        // =========================================================

        if (databaseType == AppDatabaseType.MySql)
        {
            var dbServer =
                Preferences.Get(
                    "MySql_Servidor",
                    "192.168.1.2");


            var dbDatabase =
                Preferences.Get(
                    "MySql_Banco",
                    "db_academia_do_ze");


            var dbUser =
                Preferences.Get(
                    "MySql_Usuario",
                    "root");


            var dbPassword =
                Preferences.Get(
                    "MySql_Senha",
                    "abcBolinhas12345");


            var dbComplemento =
                Preferences.Get(
                    "MySql_Complemento",
                    "Port=3306;" +
                    "SslMode=Disabled;" +
                    "AllowPublicKeyRetrieval=True;" +
                    "Connection Timeout=10;" +
                    "Default Command Timeout=30;");


            return (
                $"Server={dbServer};" +
                $"Database={dbDatabase};" +
                $"User Id={dbUser};" +
                $"Password={dbPassword};" +
                dbComplemento,
                AppDatabaseType.MySql);
        }


        // =========================================================
        // SQL SERVER
        // =========================================================

        var sqlServer =
            Preferences.Get(
                "SqlServer_Servidor",
                "172.24.32.1");


        var sqlDatabase =
            Preferences.Get(
                "SqlServer_Banco",
                "db_academia_do_ze");


        var sqlUser =
            Preferences.Get(
                "SqlServer_Usuario",
                "sa");


        var sqlPassword =
            Preferences.Get(
                "SqlServer_Senha",
                "abcBolinhas12345");


        var sqlComplemento =
            Preferences.Get(
                "SqlServer_Complemento",
                "TrustServerCertificate=True;" +
                "Encrypt=True;" +
                "Connect Timeout=5;" +
                "Connection Timeout=5;");


        return (
            $"Server={sqlServer};" +
            $"Database={sqlDatabase};" +
            $"User Id={sqlUser};" +
            $"Password={sqlPassword};" +
            sqlComplemento,
            AppDatabaseType.SqlServer);
    }
}