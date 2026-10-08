using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AlunoListPage : ContentPage
{
    private readonly AlunoListViewModel _viewModel;

    public AlunoListPage(AlunoListViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.CarregarCommand.ExecuteAsync(null);
    }

    private async void OnEditButtonClicked(
        object sender,
        EventArgs e)
    {
        if (sender is not Button button ||
            button.BindingContext is not AlunoDto aluno)
        {
            return;
        }

        await _viewModel.EditarCommand.ExecuteAsync(aluno);
    }

    private async void OnDeleteButtonClicked(
        object sender,
        EventArgs e)
    {
        if (sender is not Button button ||
            button.BindingContext is not AlunoDto aluno)
        {
            return;
        }

        await _viewModel.ExcluirCommand.ExecuteAsync(aluno);
    }
}