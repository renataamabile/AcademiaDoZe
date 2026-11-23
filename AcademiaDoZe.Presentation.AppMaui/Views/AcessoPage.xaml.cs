using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AcessoPage : ContentPage
{
    public AcessoPage(AcessoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
