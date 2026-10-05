using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

public class DataSeeder
{
    private readonly ILogradouroService _logradouroService;
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;
    private readonly IMatriculaService _matriculaService;

    public DataSeeder(ILogradouroService logradouroService,
        IAlunoService alunoService,
        IColaboradorService colaboradorService,
        IMatriculaService matriculaService)
    {
        _logradouroService = logradouroService;
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        _matriculaService = matriculaService;
    }

    public async Task SeedAsync()
    {
        try
        {
            var logs = (await _logradouroService.ObterTodosAsync()).ToList();
            if (!logs.Any())
            {
                logs = new List<LogradouroDto>
                {
                    await _logradouroService.AdicionarAsync(new LogradouroDto { Cep = "80000001", Nome = "Rua das Flores", Bairro = "Bairro A", Cidade = "SQLite", Estado = "SC", Pais = "Brasil" }),
                    await _logradouroService.AdicionarAsync(new LogradouroDto { Cep = "80000002", Nome = "Rua do Sol", Bairro = "Bairro B", Cidade = "SQLite", Estado = "SC", Pais = "Brasil" }),
                    await _logradouroService.AdicionarAsync(new LogradouroDto { Cep = "80000003", Nome = "Avenida Central", Bairro = "Centro", Cidade = "SQLite", Estado = "SC", Pais = "Brasil" })
                };
            }

            // Refresh logradouros
            logs = (await _logradouroService.ObterTodosAsync()).ToList();

            // Seed alunos if none
            var alunos = (await _alunoService.ObterTodosAsync()).ToList();
            if (!alunos.Any())
            {
                var created = new List<AlunoDto>();
                int i = 1;
                foreach (var log in logs)
                {
                    var alunoDto = new AlunoDto
                    {
                        Nome = "Aluno " + i,
                        Cpf = GerarCpf(),
                        DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
                        Telefone = GerarTelefone(i),
                        Email = $"aluno{i}@exemplo.com",
                        Endereco = log,
                        Numero = "100",
                        Complemento = "",
                        Senha = "SenhaSQLite123",
                    };
                    var added = await _alunoService.AdicionarAsync(alunoDto);
                    created.Add(added);
                    i++;
                }
                alunos = created;
            }

            // Seed colaboradores if none
            var colaboradores = (await _colaboradorService.ObterTodosAsync()).ToList();
            if (!colaboradores.Any())
            {
                var createdCols = new List<ColaboradorDto>();
                int j = 1;
                foreach (var log in logs)
                {
                    var colDto = new ColaboradorDto
                    {
                        Nome = "Colaborador " + j,
                        Cpf = GerarCpf(),
                        DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                        Telefone = GerarTelefone(j),
                        Email = $"colab{j}@exemplo.com",
                        Endereco = log,
                        Numero = "10",
                        Complemento = "",
                        Senha = "SenhaSQLite123",
                        DataAdmissao = DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
                        Tipo = AcademiaDoZe.Application.Enums.AppColaboradorTipo.Atendente,
                        Vinculo = AcademiaDoZe.Application.Enums.AppColaboradorVinculo.CLT
                    };
                    var addedCol = await _colaboradorService.AdicionarAsync(colDto);
                    createdCols.Add(addedCol);
                    j++;
                }
                colaboradores = createdCols;
            }

            // Seed matriculas if none
            var matriculas = (await _matriculaService.ObterTodasAsync()).ToList();
            if (!matriculas.Any() && alunos.Any())
            {
                foreach (var aluno in alunos.Take(3))
                {
                    var matDto = new MatriculaDto
                    {
                        AlunoMatricula = aluno,
                        Plano = AcademiaDoZe.Application.Enums.AppMatriculaPlano.Mensal,
                        DataInicio = DateOnly.FromDateTime(DateTime.Today.AddDays(-10)),
                        DataFim = DateOnly.FromDateTime(DateTime.Today.AddMonths(11)),
                        Objetivo = "Bem-estar",
                        RestricoesMedicas = AcademiaDoZe.Application.Enums.AppMatriculaRestricoes.None
                    };
                    await _matriculaService.AdicionarAsync(matDto);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DataSeeder error: {ex.Message}");
        }
    }

    // Gera CPF válido (similar ao TestBase)
    private static string GerarCpf()
    {
        var bytes = Guid.NewGuid().ToByteArray();
        var numero = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 900000000 + 100000000;
        var cpfBase = numero.ToString("D9");
        int soma = 0;
        for (int i = 0; i < 9; i++) soma += (cpfBase[i] - '0') * (10 - i);
        var resto = soma % 11;
        var digito1 = resto < 2 ? 0 : 11 - resto;
        soma = 0;
        for (int i = 0; i < 9; i++) soma += (cpfBase[i] - '0') * (11 - i);
        soma += digito1 * 2;
        resto = soma % 11;
        var digito2 = resto < 2 ? 0 : 11 - resto;
        return cpfBase + digito1.ToString() + digito2.ToString();
    }

    private static string GerarTelefone(int seed = 0)
    {
        var numero = (10000000 + (Math.Abs(Environment.TickCount) % 90000000));
        return $"419{(seed % 9) + 1}{numero:D8}";
    }
}
