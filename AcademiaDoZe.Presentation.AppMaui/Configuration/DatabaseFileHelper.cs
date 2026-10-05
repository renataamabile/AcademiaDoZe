using Microsoft.Maui.Storage;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

public static class DatabaseFileHelper
{
    private const string DatabaseName = "db_academia_do_ze.db";

    public static async Task EnsureDatabaseFileInAppDataAsync()
    {
        var destinationPath = Path.Combine(
            FileSystem.AppDataDirectory,
            DatabaseName);

        // Se já existe, não copia novamente.
        if (File.Exists(destinationPath))
            return;

        // O banco fica dentro de Resources/Raw
        // e é empacotado junto com o aplicativo.
        await using var sourceStream =
            await FileSystem.OpenAppPackageFileAsync(DatabaseName);

        await using var destinationStream =
            File.Create(destinationPath);

        await sourceStream.CopyToAsync(destinationStream);
    }
}