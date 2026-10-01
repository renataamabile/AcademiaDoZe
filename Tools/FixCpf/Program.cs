using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.Sqlite;

static class Program
{
    static int Main(string[] args)
    {
        // Suporta modo list-only com --list, --dry-run ou --list-only
        bool listOnly = args.Any(a => a.Equals("--list", StringComparison.OrdinalIgnoreCase)
            || a.Equals("--dry-run", StringComparison.OrdinalIgnoreCase)
            || a.Equals("--list-only", StringComparison.OrdinalIgnoreCase));

        var dbPathArg = args.FirstOrDefault(a => !a.StartsWith("-")) ?? string.Empty;
        var dbPath = !string.IsNullOrWhiteSpace(dbPathArg) ? dbPathArg : @"C:\Users\renat\academia_do_ze.db";
        if (!System.IO.File.Exists(dbPath))
        {
            Console.WriteLine($"Arquivo não encontrado: {dbPath}");
            return 1;
        }

        var backup = dbPath + ".backup." + DateTime.Now.ToString("yyyyMMddHHmmss");
        System.IO.File.Copy(dbPath, backup);
        Console.WriteLine($"Backup criado: {backup}");

        var connStr = $"Data Source={dbPath};";
        using var conn = new SqliteConnection(connStr);
        conn.Open();

        var existingCpfs = new HashSet<string>(StringComparer.Ordinal);
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT cpf FROM tb_aluno UNION SELECT cpf FROM tb_colaborador";
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                var cpf = rdr.IsDBNull(0) ? string.Empty : rdr.GetString(0) ?? string.Empty;
                cpf = OnlyDigits(cpf);
                if (!string.IsNullOrEmpty(cpf)) existingCpfs.Add(cpf);
            }
        }

        var invalidAlunos = GetInvalidRows(conn, "tb_aluno", "id_aluno");
        var invalidColabs = GetInvalidRows(conn, "tb_colaborador", "id_colaborador");

        Console.WriteLine($"Alunos com CPF inválido: {invalidAlunos.Count}");
        Console.WriteLine($"Colaboradores com CPF inválido: {invalidColabs.Count}");

        if (invalidAlunos.Count == 0 && invalidColabs.Count == 0)
        {
            Console.WriteLine("Nenhum CPF inválido encontrado.");
            return 0;
        }

        if (listOnly)
        {
            Console.WriteLine("Listagem de registros com CPF inválido (modo somente leitura):");
            foreach (var r in invalidAlunos)
                Console.WriteLine($"Aluno id={r.id} cpf='{r.cpf}'");
            foreach (var r in invalidColabs)
                Console.WriteLine($"Colaborador id={r.id} cpf='{r.cpf}'");
            Console.WriteLine("Nenhuma alteração foi aplicada (modo list-only).");
            conn.Close();
            return 0;
        }

        // Gerar e aplicar novos CPFs válidos
        int updated = 0;
        foreach (var row in invalidAlunos)
        {
            var newCpf = GenerateUniqueCpf(existingCpfs);
            if (UpdateCpf(conn, "tb_aluno", "cpf", "id_aluno", row.id, newCpf))
            {
                existingCpfs.Add(newCpf);
                updated++;
                Console.WriteLine($"Aluno {row.id}: CPF atualizado -> {newCpf}");
            }
        }
        foreach (var row in invalidColabs)
        {
            var newCpf = GenerateUniqueCpf(existingCpfs);
            if (UpdateCpf(conn, "tb_colaborador", "cpf", "id_colaborador", row.id, newCpf))
            {
                existingCpfs.Add(newCpf);
                updated++;
                Console.WriteLine($"Colaborador {row.id}: CPF atualizado -> {newCpf}");
            }
        }

        Console.WriteLine($"Atualizações realizadas: {updated}");
        conn.Close();
        return 0;
    }

    static List<(int id, string cpf)> GetInvalidRows(SqliteConnection conn, string table, string idColumn)
    {
        var list = new List<(int, string)>();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {idColumn}, cpf FROM {table};";
        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            var id = rdr.GetInt32(0);
            var cpf = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1) ?? string.Empty;
            var digits = OnlyDigits(cpf);
            if (!IsValidCpf(digits)) list.Add((id, cpf));
        }
        return list;
    }

    static bool UpdateCpf(SqliteConnection conn, string table, string cpfColumn, string idColumn, int id, string newCpf)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"UPDATE {table} SET {cpfColumn} = @cpf WHERE {idColumn} = @id;";
        cmd.Parameters.AddWithValue("@cpf", newCpf);
        cmd.Parameters.AddWithValue("@id", id);
        var rows = cmd.ExecuteNonQuery();
        return rows > 0;
    }

    static string OnlyDigits(string s) => new string((s ?? string.Empty).Where(char.IsDigit).ToArray());

    static string GenerateUniqueCpf(HashSet<string> existing)
    {
        string cpf;
        do
        {
            cpf = GenerateCpf();
        } while (existing.Contains(cpf));
        return cpf;
    }

    static string GenerateCpf()
    {
        var rnd = new Random();
        int[] nums = new int[9];
        for (int i = 0; i < 9; i++) nums[i] = rnd.Next(0, 10);
        int d1 = CalcDigit(nums, 10);
        var arr = nums.Concat(new[] { d1 }).ToArray();
        int d2 = CalcDigit(arr, 11);
        return string.Concat(nums.Select(n => n.ToString())) + d1.ToString() + d2.ToString();
    }

    static int CalcDigit(IEnumerable<int> digits, int weightStart)
    {
        int soma = 0; int i = 0;
        foreach (var d in digits)
        {
            soma += d * (weightStart - i);
            i++;
        }
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    static bool IsValidCpf(string digits)
    {
        if (string.IsNullOrEmpty(digits) || digits.Length != 11) return false;
        // Reject sequences like 00000000000
        if (digits.Distinct().Count() == 1) return false;
        var nums = digits.Select(c => c - '0').ToArray();
        var d1 = CalcDigit(nums.Take(9), 10);
        var d2 = CalcDigit(nums.Take(10), 11);
        return nums[9] == d1 && nums[10] == d2;
    }
}
