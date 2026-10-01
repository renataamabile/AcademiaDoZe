-- ============================================================
-- SCRIPT DE CRIAÇÃO DO BANCO DE DADOS: db_academia_do_ze (SQLite)
-- Baseado na camada de domínio AcademiaDoZe.Domain
-- ============================================================
-- Habilitar suporte a Chaves Estrangeiras no SQLite
PRAGMA foreign_keys = ON;
CREATE TABLE IF NOT EXISTS tb_logradouro (
id_logradouro INTEGER PRIMARY KEY AUTOINCREMENT,
cep TEXT NOT NULL UNIQUE,
nome TEXT NOT NULL,
bairro TEXT NOT NULL,
cidade TEXT NOT NULL,
estado TEXT NOT NULL,
pais TEXT NOT NULL DEFAULT 'Brasil'
);
CREATE INDEX IF NOT EXISTS ix_tb_logradouro_cep ON tb_logradouro(cep);
CREATE INDEX IF NOT EXISTS ix_tb_logradouro_cidade ON tb_logradouro(cidade);
CREATE TABLE IF NOT EXISTS tb_aluno (
id_aluno INTEGER PRIMARY KEY AUTOINCREMENT,
cpf TEXT NOT NULL UNIQUE,
nome TEXT NOT NULL,
nascimento TEXT NOT NULL, -- Data em formato ISO-8601 (YYYY-MM-DD)
telefone TEXT NOT NULL,
email TEXT NOT NULL,
logradouro_id INTEGER NOT NULL,
numero TEXT NOT NULL,
complemento TEXT NULL,
senha TEXT NOT NULL,
foto BLOB NULL,
FOREIGN KEY (logradouro_id) REFERENCES tb_logradouro(id_logradouro) ON DELETE RESTRICT ON UPDATE CASCADE
);
CREATE INDEX IF NOT EXISTS ix_tb_aluno_cpf ON tb_aluno(cpf);
CREATE TABLE IF NOT EXISTS tb_colaborador (
id_colaborador INTEGER PRIMARY KEY AUTOINCREMENT,
cpf TEXT NOT NULL UNIQUE,
nome TEXT NOT NULL,
nascimento TEXT NOT NULL, -- Data em formato ISO-8601 (YYYY-MM-DD)
telefone TEXT NOT NULL,
email TEXT NOT NULL,
logradouro_id INTEGER NOT NULL,
numero TEXT NOT NULL,
complemento TEXT NULL,
senha TEXT NOT NULL,
foto BLOB NULL,
admissao TEXT NOT NULL, -- Data em formato ISO-8601 (YYYY-MM-DD)
tipo INTEGER NOT NULL, -- Enum ColaboradorTipo (0=Administrador, 1=Atendente, 2=Instrutor)
vinculo INTEGER NOT NULL, -- Enum ColaboradorVinculo (0=CLT, 1=Estágio)
FOREIGN KEY (logradouro_id) REFERENCES tb_logradouro(id_logradouro) ON DELETE RESTRICT ON UPDATE CASCADE
);
CREATE INDEX IF NOT EXISTS ix_tb_colaborador_cpf ON tb_colaborador(cpf);
CREATE TABLE IF NOT EXISTS tb_matricula (
id_matricula INTEGER PRIMARY KEY AUTOINCREMENT,
aluno_id INTEGER NOT NULL,
plano INTEGER NOT NULL, -- Enum MatriculaPlano (0=Mensal, 1=Trimestral, 2=Semestral, 3=Anual)
data_inicio TEXT NOT NULL, -- Data em formato ISO-8601 (YYYY-MM-DD)
data_fim TEXT NOT NULL, -- Data em formato ISO-8601 (YYYY-MM-DD)
objetivo TEXT NOT NULL,
restricao_medica INTEGER NOT NULL DEFAULT 0, -- Enum [Flags] MatriculaRestricoes (Bitmask)
obs_restricao TEXT NULL,
laudo_medico BLOB NULL,
FOREIGN KEY (aluno_id) REFERENCES tb_aluno(id_aluno) ON DELETE CASCADE ON UPDATE CASCADE
);
CREATE INDEX IF NOT EXISTS ix_tb_matricula_aluno_id ON tb_matricula(aluno_id);
CREATE INDEX IF NOT EXISTS ix_tb_matricula_data_fim ON tb_matricula(data_fim);
CREATE TABLE IF NOT EXISTS tb_acesso (
id_acesso INTEGER PRIMARY KEY AUTOINCREMENT,
pessoa_tipo INTEGER NOT NULL, -- 0 = Aluno (AcessoAluno), 1 = Colaborador (AcessoColaborador)
pessoa_id INTEGER NOT NULL,
data_hora TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP) -- Data/Hora em formato ISO-8601 (YYYY-MM-DD HH:MM:SS)
);
CREATE INDEX IF NOT EXISTS ix_tb_acesso_pessoa ON tb_acesso(pessoa_tipo, pessoa_id);
CREATE INDEX IF NOT EXISTS ix_tb_acesso_data_hora ON tb_acesso(data_hora); 

-- =====================
-- Dados de exemplo (seed) para facilitar testes manuais
-- Insere apenas se as tabelas estiverem vazias
-- =====================

-- Logradouros de exemplo
INSERT INTO tb_logradouro (cep, nome, bairro, cidade, estado, pais)
SELECT '80000001','Rua das Flores','Bairro A','SQLite','SC','Brasil'
WHERE NOT EXISTS (SELECT 1 FROM tb_logradouro LIMIT 1);

INSERT INTO tb_logradouro (cep, nome, bairro, cidade, estado, pais)
SELECT '80000002','Rua do Sol','Bairro B','SQLite','SC','Brasil'
WHERE NOT EXISTS (SELECT 1 FROM tb_logradouro WHERE cep = '80000002');

INSERT INTO tb_logradouro (cep, nome, bairro, cidade, estado, pais)
SELECT '80000003','Avenida Central','Centro','SQLite','SC','Brasil'
WHERE NOT EXISTS (SELECT 1 FROM tb_logradouro WHERE cep = '80000003');

-- Alunos de exemplo (vinculados ao primeiro logradouro)
INSERT INTO tb_aluno (cpf, nome, nascimento, telefone, email, logradouro_id, numero, complemento, senha, foto)
SELECT '12345678909','Renata Amabile','1995-05-15','41999990001','renata@example.com', (SELECT id_logradouro FROM tb_logradouro WHERE cep='80000001' LIMIT 1), '200','Basquerote','SenhaSQLite123', NULL
WHERE NOT EXISTS (SELECT 1 FROM tb_aluno LIMIT 1);

INSERT INTO tb_aluno (cpf, nome, nascimento, telefone, email, logradouro_id, numero, complemento, senha, foto)
SELECT '12345678910','Aluno Dois','1990-01-10','41999990002','aluno2@example.com', (SELECT id_logradouro FROM tb_logradouro WHERE cep='80000002' LIMIT 1), '100','Complemento','SenhaSQLite123', NULL
WHERE NOT EXISTS (SELECT 1 FROM tb_aluno WHERE cpf = '12345678910');

INSERT INTO tb_aluno (cpf, nome, nascimento, telefone, email, logradouro_id, numero, complemento, senha, foto)
SELECT '12345678911','Aluno Tres','1988-07-20','41999990003','aluno3@example.com', (SELECT id_logradouro FROM tb_logradouro WHERE cep='80000003' LIMIT 1), '50','Complemento','SenhaSQLite123', NULL
WHERE NOT EXISTS (SELECT 1 FROM tb_aluno WHERE cpf = '12345678911');

-- Colaboradores de exemplo
INSERT INTO tb_colaborador (cpf, nome, nascimento, telefone, email, logradouro_id, numero, complemento, senha, foto, admissao, tipo, vinculo)
SELECT '98765432100','Colaborador Um','1985-03-05','41988880001','colab1@example.com', (SELECT id_logradouro FROM tb_logradouro WHERE cep='80000001' LIMIT 1), '10','Comp','SenhaSQLite123', NULL, '2020-01-01', 0, 0
WHERE NOT EXISTS (SELECT 1 FROM tb_colaborador LIMIT 1);

INSERT INTO tb_colaborador (cpf, nome, nascimento, telefone, email, logradouro_id, numero, complemento, senha, foto, admissao, tipo, vinculo)
SELECT '98765432101','Colaborador Dois','1992-11-11','41988880002','colab2@example.com', (SELECT id_logradouro FROM tb_logradouro WHERE cep='80000002' LIMIT 1), '20','Comp','SenhaSQLite123', NULL, '2021-06-15', 1, 0
WHERE NOT EXISTS (SELECT 1 FROM tb_colaborador WHERE cpf = '98765432101');

-- Matrículas de exemplo (vinculadas aos alunos inseridos)
INSERT INTO tb_matricula (aluno_id, plano, data_inicio, data_fim, objetivo, restricao_medica, obs_restricao, laudo_medico)
SELECT (SELECT id_aluno FROM tb_aluno WHERE cpf='12345678909' LIMIT 1), 0, date('now','-30 days'), date('now','+335 days'), 'Emagrecimento', 0, NULL, NULL
WHERE NOT EXISTS (SELECT 1 FROM tb_matricula LIMIT 1);

INSERT INTO tb_matricula (aluno_id, plano, data_inicio, data_fim, objetivo, restricao_medica, obs_restricao, laudo_medico)
SELECT (SELECT id_aluno FROM tb_aluno WHERE cpf='12345678910' LIMIT 1), 1, date('now','-60 days'), date('now','+30 days'), 'Hipertrofia', 0, NULL, NULL
WHERE NOT EXISTS (SELECT 1 FROM tb_matricula WHERE aluno_id = (SELECT id_aluno FROM tb_aluno WHERE cpf='12345678910' LIMIT 1));

-- Acessos de exemplo
INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora)
SELECT 0, (SELECT id_aluno FROM tb_aluno WHERE cpf='12345678909' LIMIT 1), datetime('now')
WHERE NOT EXISTS (SELECT 1 FROM tb_acesso LIMIT 1);
