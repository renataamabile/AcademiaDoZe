using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ConfigPage : ContentPage
{
    public ConfigPage()
    {
        InitializeComponent();

        CarregarTema();
        CarregarBanco();
    }


    #region Tema

    private void CarregarTema()
    {
        TemaPicker.SelectedIndex =
            Preferences.Get("Tema", "system") switch
            {
                "light" => 0,
                "dark" => 1,
                _ => 2
            };
    }


    private async void OnSalvarTemaClicked(
        object? sender,
        EventArgs e)
    {
        string selectedTheme =
            TemaPicker.SelectedIndex switch
            {
                0 => "light",
                1 => "dark",
                _ => "system"
            };

        Preferences.Set(
            "Tema",
            selectedTheme);

        WeakReferenceMessenger.Default.Send(
            new TemaPreferencesUpdatedMessage(
                "TemaAlterado"));

        await DisplayAlertAsync(
            "Sucesso",
            "Tema salvo com sucesso!",
            "OK");

        await Shell.Current.GoToAsync(
            "//dashboard");
    }

    #endregion


    #region Banco de Dados

    private void CarregarBanco()
    {
        DatabaseTypePicker.Items.Clear();

        foreach (var tipo in Enum.GetValues<AppDatabaseType>())
        {
            DatabaseTypePicker.Items.Add(
                tipo.ToString());
        }

        var bancoAtual =
            Preferences.Get(
                "DatabaseType",
                AppDatabaseType.Sqlite.ToString());

        DatabaseTypePicker.SelectedItem =
            bancoAtual;

        AtualizarInterfacePorTipoBanco();
    }


    private void OnDatabaseTypeChanged(
        object? sender,
        EventArgs? e)
    {
        AtualizarInterfacePorTipoBanco();
    }


    private void AtualizarInterfacePorTipoBanco()
    {
        if (DatabaseTypePicker.SelectedItem
            is not string selectedTypeStr ||
            !Enum.TryParse<AppDatabaseType>(
                selectedTypeStr,
                out var selectedType))
        {
            return;
        }


        switch (selectedType)
        {
            // =====================================================
            // SQLITE
            // =====================================================

            case AppDatabaseType.Sqlite:

                SqliteInfoCard.IsVisible = true;

                SqliteContainer.IsVisible = true;

                ServidorBancoGrid.IsVisible = false;

                CredenciaisGrid.IsVisible = false;

                ComplementoLabel.Text =
                    "Complemento (ex: Default Timeout=5;)";

                ComplementoEntry.Placeholder =
                    "Default Timeout=5;";


                // No Windows usamos o banco da pasta do projeto.
                // No Android usamos o AppDataDirectory do aplicativo.
                var defaultSqlitePath =
                    DeviceInfo.Platform == DevicePlatform.WinUI
                        ? @"C:\DEV\AcademiaDoZe\db_academia_do_ze.db"
                        : Path.Combine(
                            FileSystem.AppDataDirectory,
                            "db_academia_do_ze.db");


                SqliteCaminhoEntry.Text =
                    Preferences.Get(
                        "Sqlite_Caminho",
                        Preferences.Get(
                            "SqliteCaminho",
                            defaultSqlitePath));


                // IMPORTANTE:
                // SQLite possui seu próprio complemento.
                // Nunca utiliza o complemento do MySQL ou SQL Server.
                var sqliteComplemento =
                    Preferences.Get(
                        "Sqlite_Complemento",
                        "Default Timeout=5;");


                // Corrige uma configuração antiga que possa ter
                // sido salva com parâmetros do MySQL.
                if (string.IsNullOrWhiteSpace(sqliteComplemento) ||
                    sqliteComplemento.Contains(
                        "Port=",
                        StringComparison.OrdinalIgnoreCase) ||
                    sqliteComplemento.Contains(
                        "SslMode=",
                        StringComparison.OrdinalIgnoreCase) ||
                    sqliteComplemento.Contains(
                        "AllowPublicKeyRetrieval",
                        StringComparison.OrdinalIgnoreCase))
                {
                    sqliteComplemento =
                        "Default Timeout=5;";
                }


                ComplementoEntry.Text =
                    sqliteComplemento;

                break;


            // =====================================================
            // SQL SERVER
            // =====================================================

            case AppDatabaseType.SqlServer:

                SqliteInfoCard.IsVisible = false;

                SqliteContainer.IsVisible = false;

                ServidorBancoGrid.IsVisible = true;

                CredenciaisGrid.IsVisible = true;


                ServidorEntry.Placeholder =
                    "Ex: 172.24.32.1 ou localhost";

                BancoEntry.Placeholder =
                    "Ex: db_academia_do_ze";

                UsuarioEntry.Placeholder =
                    "Ex: sa";


                ComplementoLabel.Text =
                    "Complemento (SSL / Timeout / Criptografia)";

                ComplementoEntry.Placeholder =
                    "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;Connection Timeout=5;";


                // SQL Server possui suas próprias configurações.
                ServidorEntry.Text =
                    Preferences.Get(
                        "SqlServer_Servidor",
                        "172.24.32.1");


                BancoEntry.Text =
                    Preferences.Get(
                        "SqlServer_Banco",
                        "db_academia_do_ze");


                UsuarioEntry.Text =
                    Preferences.Get(
                        "SqlServer_Usuario",
                        "sa");


                SenhaEntry.Text =
                    Preferences.Get(
                        "SqlServer_Senha",
                        "abcBolinhas12345");


                ComplementoEntry.Text =
                    Preferences.Get(
                        "SqlServer_Complemento",
                        "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;Connection Timeout=5;");

                break;


            // =====================================================
            // MYSQL
            // =====================================================

            case AppDatabaseType.MySql:

                SqliteInfoCard.IsVisible = false;

                SqliteContainer.IsVisible = false;

                ServidorBancoGrid.IsVisible = true;

                CredenciaisGrid.IsVisible = true;


                ServidorEntry.Placeholder =
                    "Ex: 192.168.1.2 ou localhost";

                BancoEntry.Placeholder =
                    "Ex: db_academia_do_ze";

                UsuarioEntry.Placeholder =
                    "Ex: root";


                ComplementoLabel.Text =
                    "Complemento (Porta / Timeout)";

                ComplementoEntry.Placeholder =
                    "Port=3306;SslMode=Disabled;AllowPublicKeyRetrieval=True;Connection Timeout=10;Default Command Timeout=30;";


                // MySQL possui suas próprias configurações.
                ServidorEntry.Text =
                    Preferences.Get(
                        "MySql_Servidor",
                        "192.168.1.2");


                BancoEntry.Text =
                    Preferences.Get(
                        "MySql_Banco",
                        "db_academia_do_ze");


                UsuarioEntry.Text =
                    Preferences.Get(
                        "MySql_Usuario",
                        "root");


                SenhaEntry.Text =
                    Preferences.Get(
                        "MySql_Senha",
                        "abcBolinhas12345");


                ComplementoEntry.Text =
                    Preferences.Get(
                        "MySql_Complemento",
                        "Port=3306;SslMode=Disabled;AllowPublicKeyRetrieval=True;Connection Timeout=10;Default Command Timeout=30;");

                break;
        }
    }


    private async void OnSalvarBdClicked(
        object? sender,
        EventArgs e)
    {
        if (DatabaseTypePicker.SelectedItem
            is not string selectedTypeStr ||
            !Enum.TryParse<AppDatabaseType>(
                selectedTypeStr,
                out var selectedType))
        {
            await DisplayAlertAsync(
                "Aviso",
                "Selecione um tipo de banco de dados válido.",
                "OK");

            return;
        }


        // =========================================================
        // SQLITE
        // =========================================================

        if (selectedType == AppDatabaseType.Sqlite)
        {
            if (string.IsNullOrWhiteSpace(
                SqliteCaminhoEntry.Text))
            {
                await DisplayAlertAsync(
                    "Validação",
                    "Informe o caminho do arquivo do banco SQLite.",
                    "OK");

                return;
            }


            var caminho =
                SqliteCaminhoEntry.Text.Trim();

            var complemento =
                ComplementoEntry.Text?.Trim();

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
            }


            // Configurações EXCLUSIVAS do SQLite.
            Preferences.Set(
                "Sqlite_Caminho",
                caminho);

            Preferences.Set(
                "Sqlite_Complemento",
                complemento);

            // Mantém a chave antiga SqliteCaminho
            // para compatibilidade com versões anteriores.
            Preferences.Set(
                "SqliteCaminho",
                caminho);
        }


        // =========================================================
        // MYSQL / SQL SERVER
        // =========================================================

        else
        {
            if (string.IsNullOrWhiteSpace(
                ServidorEntry.Text))
            {
                await DisplayAlertAsync(
                    "Validação",
                    "Informe o servidor do banco de dados.",
                    "OK");

                return;
            }


            if (string.IsNullOrWhiteSpace(
                BancoEntry.Text))
            {
                await DisplayAlertAsync(
                    "Validação",
                    "Informe o nome do banco de dados.",
                    "OK");

                return;
            }


            if (string.IsNullOrWhiteSpace(
                UsuarioEntry.Text))
            {
                await DisplayAlertAsync(
                    "Validação",
                    "Informe o usuário do banco de dados.",
                    "OK");

                return;
            }


            var servidor =
                ServidorEntry.Text.Trim();

            var banco =
                BancoEntry.Text.Trim();

            var usuario =
                UsuarioEntry.Text.Trim();

            var senha =
                SenhaEntry.Text ?? string.Empty;

            var complemento =
                ComplementoEntry.Text?.Trim()
                ?? string.Empty;


            var prefix =
                selectedType == AppDatabaseType.SqlServer
                    ? "SqlServer"
                    : "MySql";


            // Configurações EXCLUSIVAS do banco selecionado.
            Preferences.Set(
                $"{prefix}_Servidor",
                servidor);

            Preferences.Set(
                $"{prefix}_Banco",
                banco);

            Preferences.Set(
                $"{prefix}_Usuario",
                usuario);

            Preferences.Set(
                $"{prefix}_Senha",
                senha);

            Preferences.Set(
                $"{prefix}_Complemento",
                complemento);
        }


        // =========================================================
        // TIPO DE BANCO ATUAL
        // =========================================================

        Preferences.Set(
            "DatabaseType",
            selectedType.ToString());


        // =========================================================
        // AVISA A APLICAÇÃO QUE O BANCO MUDOU
        // =========================================================

        WeakReferenceMessenger.Default.Send(
            new BancoPreferencesUpdatedMessage(
                "BancoAlterado"));


        await DisplayAlertAsync(
            "Sucesso",
            $"Configurações do banco de dados ({selectedType}) salvas com sucesso!",
            "OK");


        await Shell.Current.GoToAsync(
            "//dashboard");
    }

    #endregion


    private async void OnCancelarClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            "//dashboard");
    }


    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        WeakReferenceMessenger.Default
            .UnregisterAll(this);
    }
}