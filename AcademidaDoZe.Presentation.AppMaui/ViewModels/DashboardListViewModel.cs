using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class DashboardListViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;
    private readonly IMatriculaService _matriculaService;

    private int _totalLogradouros;
    public int TotalLogradouros
    {
        get => _totalLogradouros;
        set => SetProperty(ref _totalLogradouros, value);
    }

    private int _totalAlunos;
    public int TotalAlunos
    {
        get => _totalAlunos;
        set => SetProperty(ref _totalAlunos, value);
    }

    private int _totalColaboradores;
    public int TotalColaboradores
    {
        get => _totalColaboradores;
        set => SetProperty(ref _totalColaboradores, value);
    }

    private int _totalMatriculas;
    public int TotalMatriculas
    {
        get => _totalMatriculas;
        set => SetProperty(ref _totalMatriculas, value);
    }

    public DashboardListViewModel(
        ILogradouroService logradouroService,
        IAlunoService alunoService,
        IColaboradorService colaboradorService,
        IMatriculaService matriculaService)
    {
        _logradouroService = logradouroService;
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        _matriculaService = matriculaService;

        Title = "Dashboard";
        IsBusy = false;
    }

    [RelayCommand]
    private async Task LoadDashboardDataAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            using var cts =
                new CancellationTokenSource(
                    TimeSpan.FromSeconds(10));

            Debug.WriteLine("========================================");
            Debug.WriteLine("INICIANDO CARREGAMENTO DO DASHBOARD");
            Debug.WriteLine("========================================");

            var logradouros =
                await _logradouroService.ObterTodosAsync(cts.Token);

            Debug.WriteLine(
                $"Logradouros encontrados: {logradouros.Count()}");

            var alunos =
                await _alunoService.ObterTodosAsync(cts.Token);

            Debug.WriteLine(
                $"Alunos encontrados: {alunos.Count()}");

            var colaboradores =
                await _colaboradorService.ObterTodosAsync(cts.Token);

            Debug.WriteLine(
                $"Colaboradores encontrados: {colaboradores.Count()}");

            var matriculas =
                await _matriculaService.ObterTodasAsync(cts.Token);

            Debug.WriteLine(
                $"Matrículas encontradas: {matriculas.Count()}");

            TotalLogradouros = logradouros.Count();
            TotalAlunos = alunos.Count();
            TotalColaboradores = colaboradores.Count();
            TotalMatriculas = matriculas.Count();

            Debug.WriteLine("========================================");
            Debug.WriteLine("DASHBOARD CARREGADO");
            Debug.WriteLine("========================================");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                "Tempo Esgotado",
                "A consulta ao banco de dados demorou mais que o esperado.",
                "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("========================================");
            Debug.WriteLine("ERRO NO DASHBOARD");
            Debug.WriteLine(ex.ToString());
            Debug.WriteLine("========================================");

            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Erro ao carregar dados:\n{ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToLogradourosAsync()
    {
        await Shell.Current.GoToAsync("//logradouros");
    }

    [RelayCommand]
    private async Task NavigateToAlunosAsync()
    {
        await Shell.Current.GoToAsync("//alunos");
    }

    [RelayCommand]
    private async Task NavigateToColaboradoresAsync()
    {
        await Shell.Current.GoToAsync("//colaboradores");
    }

    [RelayCommand]
    private async Task NavigateToMatriculasAsync()
    {
        await Shell.Current.GoToAsync("//matriculas");
    }
}