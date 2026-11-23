
// Renata Amabile Basquerote
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Domain.Entities;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AcessoViewModel : BaseViewModel
{
    private readonly IAcessoAlunoService _acessoAlunoService;
    private readonly IAcessoColaboradorService _acessoColaboradorService;

    private string _codigoPessoa = string.Empty;

    public string CodigoPessoa
    {
        get => _codigoPessoa;
        set => SetProperty(ref _codigoPessoa, value);
    }

    private string _resultadoAcesso = string.Empty;

    public string ResultadoAcesso
    {
        get => _resultadoAcesso;
        set => SetProperty(ref _resultadoAcesso, value);
    }

    private string _nomePessoa = string.Empty;

    public string NomePessoa
    {
        get => _nomePessoa;
        set => SetProperty(ref _nomePessoa, value);
    }

    private ImageSource? _fotoPessoa;

    public ImageSource? FotoPessoa
    {
        get => _fotoPessoa;
        set => SetProperty(ref _fotoPessoa, value);
    }

    private bool _exibirResultado;

    public bool ExibirResultado
    {
        get => _exibirResultado;
        set => SetProperty(ref _exibirResultado, value);
    }

    public ObservableCollection<AcessoHistoricoItem> HistoricoAcessos { get; } = new();

    private bool _modoAluno = true;

    public bool ModoAluno
    {
        get => _modoAluno;
        set
        {
            if (SetProperty(ref _modoAluno, value))
            {
                OnPropertyChanged(nameof(ModoColaborador));
                OnPropertyChanged(nameof(TituloModo));
                OnPropertyChanged(nameof(TextoBotao));
                OnPropertyChanged(nameof(CorAluno));
                OnPropertyChanged(nameof(CorColaborador));
                OnPropertyChanged(nameof(CorTextoAluno));
                OnPropertyChanged(nameof(CorTextoColaborador));
                OnPropertyChanged(nameof(SubtituloModo));

                CodigoPessoa = string.Empty;
                ResultadoAcesso = string.Empty;
                NomePessoa = string.Empty;
                FotoPessoa = null;
                ExibirResultado = false;
            }
        }
    }

    public bool ModoColaborador => !ModoAluno;

    public string TituloModo =>
        ModoAluno ? "Acesso de Aluno" : "Acesso de Colaborador";

    public string SubtituloModo =>
        ModoAluno
        ? "Digite o código do aluno para registrar o acesso."
        : "Digite o código do colaborador para registrar o acesso.";

    public string TextoBotao =>
        ModoAluno
        ? "REGISTRAR ACESSO DO ALUNO"
        : "REGISTRAR ACESSO DO COLABORADOR";

    public Color CorAluno =>
        ModoAluno ? Color.FromArgb("#A879F5") : Color.FromArgb("#30283A");

    public Color CorColaborador =>
        ModoAluno ? Color.FromArgb("#30283A") : Color.FromArgb("#A879F5");

    public Color CorTextoAluno =>
        ModoAluno ? Colors.White : Color.FromArgb("#D5C9E3");

    public Color CorTextoColaborador =>
        ModoAluno ? Color.FromArgb("#D5C9E3") : Colors.White;

    public AcessoViewModel(
    IAcessoAlunoService acessoAlunoService,
    IAcessoColaboradorService acessoColaboradorService)
    {
        _acessoAlunoService = acessoAlunoService;
        _acessoColaboradorService = acessoColaboradorService;

        Title = "Registro de Acessos";

        _ = AtualizarHistoricoAsync();
    }

    [RelayCommand]
    private void SelecionarAluno()
    {
        ModoAluno = true;
    }

    [RelayCommand]
    private void SelecionarColaborador()
    {
        ModoAluno = false;
    }

    [RelayCommand]
    private async Task RegistrarAcessoAsync()
    {
        if (!int.TryParse(CodigoPessoa, out var id) || id <= 0)
        {
            await Shell.Current.DisplayAlertAsync(
                "Atenção",
                "Informe um código válido.",
                "OK");

            return;
        }

        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ExibirResultado = false;
            FotoPessoa = null;
            NomePessoa = string.Empty;

            AcademiaDoZe.Application.Models.ResultadoAcesso resultado;

            if (ModoAluno)
            {
                resultado = await _acessoAlunoService
                    .RegistrarEntradaOuSaidaAsync(id);
            }
            else
            {
                resultado = await _acessoColaboradorService
                    .RegistrarEntradaOuSaidaAsync(id);
            }

            ResultadoAcesso = resultado.Mensagem;
            NomePessoa = resultado.NomePessoa;

            if (resultado.Foto != null && resultado.Foto.Length > 0)
            {
                var bytes = resultado.Foto;

                FotoPessoa = ImageSource.FromStream(
                    () => new MemoryStream(bytes));
            }
            else
            {
                FotoPessoa = null;
            }

            ExibirResultado = true;

            CodigoPessoa = string.Empty;

            await AtualizarHistoricoAsync();
        }
        catch (Exception ex)
        {
            ResultadoAcesso = ex.Message;
            NomePessoa = string.Empty;
            FotoPessoa = null;
            ExibirResultado = true;

            await Shell.Current.DisplayAlertAsync(
                "Não foi possível registrar",
                ex.Message,
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AtualizarHistoricoAsync()
    {
        try
        {
            var acessosAlunos = await _acessoAlunoService
                .ObterAcessosHojeAsync();

            var acessosColaboradores = await _acessoColaboradorService
                .ObterAcessosHojeAsync();

            var itens = new List<AcessoHistoricoItem>();

            var alunosOrdenados = acessosAlunos
                .OrderBy(a => a.DataHora)
                .ToList();

            var colaboradoresOrdenados = acessosColaboradores
                .OrderBy(a => a.DataHora)
                .ToList();

            var contagemAlunos = new Dictionary<int, int>();

            foreach (var acesso in alunosOrdenados)
            {
                contagemAlunos.TryGetValue(acesso.AlunoId, out var contador);

                itens.Add(new AcessoHistoricoItem
                {
                    TipoPessoa = "Aluno",
                    CodigoPessoa = acesso.AlunoId,
                    DataHora = acesso.DataHora,
                    Movimento = contador % 2 == 0 ? "Entrada" : "Saída"
                });

                contagemAlunos[acesso.AlunoId] = contador + 1;
            }

            var contagemColaboradores = new Dictionary<int, int>();

            foreach (var acesso in colaboradoresOrdenados)
            {
                contagemColaboradores.TryGetValue(
                    acesso.ColaboradorId,
                    out var contador);

                itens.Add(new AcessoHistoricoItem
                {
                    TipoPessoa = "Colaborador",
                    CodigoPessoa = acesso.ColaboradorId,
                    DataHora = acesso.DataHora,
                    Movimento = contador % 2 == 0 ? "Entrada" : "Saída"
                });

                contagemColaboradores[acesso.ColaboradorId] = contador + 1;
            }

            var historicoOrdenado = itens
                .OrderByDescending(i => i.DataHora)
                .ToList();

            HistoricoAcessos.Clear();

            foreach (var item in historicoOrdenado)
            {
                HistoricoAcessos.Add(item);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro ao carregar histórico",
                ex.Message,
                "OK");
        }
    }
}

public class AcessoHistoricoItem
{
    public string TipoPessoa { get; set; } = string.Empty;

    public int CodigoPessoa { get; set; }

    public DateTime DataHora { get; set; }

    public string Movimento { get; set; } = string.Empty;

    public string HorarioFormatado => DataHora.ToString("HH:mm");

    public string DataFormatada => DataHora.ToString("dd/MM/yyyy");
}
