using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AlunoListViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;

    public ObservableCollection<string> FilterTypes { get; } =
    [
        "Nome",
        "CPF",
        "Id",
        "Email"
    ];

    private ObservableCollection<AlunoDto> _alunos = [];

    public ObservableCollection<AlunoDto> Alunos
    {
        get => _alunos;
        set => SetProperty(ref _alunos, value);
    }

    private AlunoDto? _selectedAluno;

    public AlunoDto? SelectedAluno
    {
        get => _selectedAluno;
        set => SetProperty(ref _selectedAluno, value);
    }

    private string _searchText = string.Empty;

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedFilterType = "Nome";

    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    public AlunoListViewModel(IAlunoService alunoService)
    {
        _alunoService = alunoService;
        Title = "Alunos";
    }

    [RelayCommand]
    private async Task CarregarAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var alunos = await _alunoService.ObterTodosAsync();

            Alunos.Clear();

            foreach (var aluno in alunos)
                Alunos.Add(aluno);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                $"Não foi possível carregar os alunos.\n\n{ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task BuscarAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            IEnumerable<AlunoDto> resultado;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                resultado = await _alunoService.ObterTodosAsync();
            }
            else
            {
                resultado = SelectedFilterType switch
                {
                    "Nome" =>
                        await _alunoService.ObterPorNomeAsync(SearchText),

                    "CPF" =>
                        await BuscarPorCpfAsync(SearchText),

                    "Email" =>
                        await BuscarPorEmailAsync(SearchText),

                    "Id" =>
                        await BuscarPorIdAsync(SearchText),

                    _ =>
                        await _alunoService.ObterTodosAsync()
                };
            }

            Alunos.Clear();

            foreach (var aluno in resultado)
                Alunos.Add(aluno);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                $"Não foi possível realizar a busca.\n\n{ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<IEnumerable<AlunoDto>> BuscarPorCpfAsync(string cpf)
    {
        var aluno = await _alunoService.ObterPorCpfAsync(cpf);

        return aluno is null
            ? []
            : [aluno];
    }

    private async Task<IEnumerable<AlunoDto>> BuscarPorEmailAsync(string email)
    {
        var aluno = await _alunoService.ObterPorEmailAsync(email);

        return aluno is null
            ? []
            : [aluno];
    }

    private async Task<IEnumerable<AlunoDto>> BuscarPorIdAsync(string idTexto)
    {
        if (!int.TryParse(idTexto, out var id))
            return [];

        var aluno = await _alunoService.ObterPorIdAsync(id);

        return aluno is null
            ? []
            : [aluno];
    }

    [RelayCommand]
    private async Task NovoAsync()
    {
        await Shell.Current.GoToAsync("aluno");
    }

    [RelayCommand]
    private async Task EditarAsync(AlunoDto? aluno)
    {
        if (aluno is null)
            return;

        await Shell.Current.GoToAsync(
            "aluno",
            new Dictionary<string, object>
            {
                ["Aluno"] = aluno
            });
    }

    [RelayCommand]
    private async Task ExcluirAsync(AlunoDto? aluno)
    {
        if (aluno is null)
            return;

        bool confirmar = await Shell.Current.DisplayAlert(
            "Excluir aluno",
            $"Deseja realmente excluir o aluno {aluno.Nome}?",
            "Sim",
            "Não");

        if (!confirmar)
            return;

        try
        {
            IsBusy = true;

            var removido = await _alunoService.RemoverAsync(aluno.Id);

            if (removido)
            {
                Alunos.Remove(aluno);

                if (SelectedAluno == aluno)
                    SelectedAluno = null;
            }
            else
            {
                await Shell.Current.DisplayAlert(
                    "Aviso",
                    "O aluno não foi encontrado.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                $"Não foi possível excluir o aluno.\n\n{ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AtualizarAsync()
    {
        await CarregarAsync();
    }
}