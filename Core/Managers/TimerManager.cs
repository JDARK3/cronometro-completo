// TimerManager.cs (VERSÃO COM System.Data.SQLite.Core)
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using CronometroRegressivo.Core.Models;
using CronometroRegressivo.Utils;

namespace CronometroRegressivo.Core.Managers
{
    // Etapa 5: GERENCIADOR DE DADOS COM PERSISTÊNCIA EM SQLITE
    // Modificar implementação para usar banco de dados permanente
    public static class TimerManager
    {
        // CONFIGURAÇÃO DO BANCO - Definir caminho e conexão
        private static string databasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "CronometroRegressivo");
        
        private static string databaseFile = Path.Combine(databasePath, "timers.db");
        private static string connectionString = $"Data Source={databaseFile};Version=3;";

        // INICIALIZAÇÃO ESTÁTICA - Garantir que banco existe ao primeiro uso
        static TimerManager()
        {
            try
            {
                // PREPARAÇÃO DO BANCO - Criar estrutura se necessário
                InitializeDatabase();
                FileLogger.Log(FileLogger.LogLevel.Info, "TimerManager inicializado com SQLite");
            }
            catch (Exception ex)
            {
                // FALHA CRÍTICA - Registrar erro de inicialização
                FileLogger.Log(FileLogger.LogLevel.Error, "Falha na inicialização do TimerManager", ex.ToString());
                throw;
            }
        }

        // INICIALIZAÇÃO DO BANCO - Criar arquivo e tabelas se não existirem
        private static void InitializeDatabase()
        {
            // VERIFICAÇÃO DE DIRETÓRIO - Criar pasta se não existir
            if (!Directory.Exists(databasePath))
            {
                Directory.CreateDirectory(databasePath);
                FileLogger.Log(FileLogger.LogLevel.Info, "Diretório de dados criado");
            }

            // CRIAÇÃO DO BANCO - Verificar se já existe
            if (!File.Exists(databaseFile))
            {
                SQLiteConnection.CreateFile(databaseFile);
                FileLogger.Log(FileLogger.LogLevel.Info, "Arquivo de banco de dados criado");
                
                // ESTRUTURA INICIAL - Criar tabelas necessárias
                CreateTables();
                
                // DADOS INICIAIS - Popular com exemplos padrão
                InsertDefaultTimers();
            }
            else
            {
                FileLogger.Log(FileLogger.LogLevel.Info, "Banco de dados existente encontrado");
            }
        }

        // CRIAÇÃO DE TABELAS - Definir estrutura do banco
        private static void CreateTables()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                
                // SQL DE CRIAÇÃO - Definir schema completo
                string createTableSql = @"
                    CREATE TABLE Timers (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        TimeInSeconds INTEGER NOT NULL CHECK(TimeInSeconds > 0),
                        HasSoundNotification INTEGER NOT NULL DEFAULT 1 CHECK(HasSoundNotification IN (0, 1)),
                        ShouldRepeat INTEGER NOT NULL DEFAULT 0 CHECK(ShouldRepeat IN (0, 1)),
                        InitialSeconds INTEGER NOT NULL DEFAULT 0 CHECK(InitialSeconds >= 0),
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    );

                    CREATE INDEX IF NOT EXISTS idx_timers_name ON Timers(Name);
                    CREATE INDEX IF NOT EXISTS idx_timers_created ON Timers(CreatedAt);
                ";

                using (var command = new SQLiteCommand(createTableSql, connection))
                {
                    command.ExecuteNonQuery();
                    FileLogger.Log(FileLogger.LogLevel.Info, "Tabelas do banco criadas com sucesso");
                }
            }
        }

        // DADOS PADRÃO - Inserir temporizadores de exemplo
        private static void InsertDefaultTimers()
        {
            try
            {
                var defaultTimers = new List<TimerModel>
                {
                    new TimerModel { 
                        Name = "Intervalo Curto", 
                        TimeInSeconds = 300,
                        HasSoundNotification = true,
                        ShouldRepeat = false,
                        InitialSeconds = 300
                    },
                    new TimerModel { 
                        Name = "Intervalo Longo", 
                        TimeInSeconds = 900,
                        HasSoundNotification = true,
                        ShouldRepeat = false,
                        InitialSeconds = 900
                    },
                    new TimerModel { 
                        Name = "Pomodoro", 
                        TimeInSeconds = 1500,
                        HasSoundNotification = true,
                        ShouldRepeat = true,
                        InitialSeconds = 1500
                    }
                };

                foreach (var timer in defaultTimers)
                {
                    SaveTimerInternal(timer); // Método interno para inserção inicial
                }

                FileLogger.Log(FileLogger.LogLevel.Info, "Temporizadores padrão inseridos");
            }
            catch (Exception ex)
            {
                FileLogger.Log(FileLogger.LogLevel.Warning, "Falha ao inserir temporizadores padrão", ex.Message);
            }
        }

        // MÉTODO INTERNO - Para inserção inicial sem log extensivo
        private static void SaveTimerInternal(TimerModel timer)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string insertSql = @"
                    INSERT INTO Timers (Name, TimeInSeconds, HasSoundNotification, ShouldRepeat, InitialSeconds)
                    VALUES (@Name, @TimeInSeconds, @HasSoundNotification, @ShouldRepeat, @InitialSeconds);
                    SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@Name", timer.Name);
                    command.Parameters.AddWithValue("@TimeInSeconds", timer.TimeInSeconds);
                    command.Parameters.AddWithValue("@HasSoundNotification", timer.HasSoundNotification ? 1 : 0);
                    command.Parameters.AddWithValue("@ShouldRepeat", timer.ShouldRepeat ? 1 : 0);
                    command.Parameters.AddWithValue("@InitialSeconds", timer.TimeInSeconds);

                    timer.Id = Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        // OPERAÇÃO PRINCIPAL - Salvar/atualizar temporizador no banco
        public static void SaveTimer(TimerModel timer)
        {
            // VALIDAÇÃO DE ENTRADA - Verificar dados obrigatórios
            if (timer == null)
                throw new ArgumentNullException(nameof(timer));

            if (string.IsNullOrWhiteSpace(timer.Name))
                throw new ArgumentException("Nome do temporizador não pode ser vazio");

            if (timer.TimeInSeconds <= 0)
                throw new ArgumentException("Duração deve ser maior que zero");

            try
            {
                // CONEXÃO COM BANCO - Usar using para garantir liberação de recursos
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    if (timer.Id > 0)
                    {
                        // ATUALIZAÇÃO - Modificar temporizador existente
                        string updateSql = @"
                            UPDATE Timers 
                            SET Name = @Name, 
                                TimeInSeconds = @TimeInSeconds, 
                                HasSoundNotification = @HasSoundNotification,
                                ShouldRepeat = @ShouldRepeat,
                                InitialSeconds = @InitialSeconds,
                                UpdatedAt = CURRENT_TIMESTAMP
                            WHERE Id = @Id";

                        using (var command = new SQLiteCommand(updateSql, connection))
                        {
                            command.Parameters.AddWithValue("@Id", timer.Id);
                            command.Parameters.AddWithValue("@Name", timer.Name);
                            command.Parameters.AddWithValue("@TimeInSeconds", timer.TimeInSeconds);
                            command.Parameters.AddWithValue("@HasSoundNotification", timer.HasSoundNotification ? 1 : 0);
                            command.Parameters.AddWithValue("@ShouldRepeat", timer.ShouldRepeat ? 1 : 0);
                            command.Parameters.AddWithValue("@InitialSeconds", timer.InitialSeconds);
                            
                            int rowsAffected = command.ExecuteNonQuery();
                            
                            if (rowsAffected == 0)
                            {
                                throw new Exception("Temporizador não encontrado para atualização");
                            }
                            
                            FileLogger.Log(FileLogger.LogLevel.Info, $"Temporizador atualizado: {timer.Name} (ID: {timer.Id})");
                        }
                    }
                    else
                    {
                        // INSERÇÃO - Criar novo temporizador
                        string insertSql = @"
                            INSERT INTO Timers (Name, TimeInSeconds, HasSoundNotification, ShouldRepeat, InitialSeconds)
                            VALUES (@Name, @TimeInSeconds, @HasSoundNotification, @ShouldRepeat, @InitialSeconds);
                            SELECT last_insert_rowid();";

                        using (var command = new SQLiteCommand(insertSql, connection))
                        {
                            command.Parameters.AddWithValue("@Name", timer.Name);
                            command.Parameters.AddWithValue("@TimeInSeconds", timer.TimeInSeconds);
                            command.Parameters.AddWithValue("@HasSoundNotification", timer.HasSoundNotification ? 1 : 0);
                            command.Parameters.AddWithValue("@ShouldRepeat", timer.ShouldRepeat ? 1 : 0);
                            command.Parameters.AddWithValue("@InitialSeconds", timer.TimeInSeconds);

                            timer.Id = Convert.ToInt32(command.ExecuteScalar());
                            FileLogger.Log(FileLogger.LogLevel.Info, $"Novo temporizador criado: {timer.Name} (ID: {timer.Id})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // REGISTRO DE ERRO - Log detalhado da falha
                FileLogger.Log(FileLogger.LogLevel.Error, $"Erro ao salvar temporizador '{timer.Name}'", ex.ToString());
                throw new Exception($"Falha ao salvar temporizador: {ex.Message}", ex);
            }
        }

        // OPERAÇÃO DE LEITURA - Obter todos os temporizadores
        public static List<TimerModel> GetTimers()
        {
            var timers = new List<TimerModel>();

            try
            {
                // CONSULTA COMPLETA - Buscar todos os registros ordenados
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string selectSql = "SELECT * FROM Timers ORDER BY Name";

                    using (var command = new SQLiteCommand(selectSql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // MAPEAMENTO DE DADOS - Converter registro para objeto
                            timers.Add(new TimerModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                TimeInSeconds = Convert.ToInt32(reader["TimeInSeconds"]),
                                HasSoundNotification = Convert.ToInt32(reader["HasSoundNotification"]) == 1,
                                ShouldRepeat = Convert.ToInt32(reader["ShouldRepeat"]) == 1,
                                InitialSeconds = Convert.ToInt32(reader["InitialSeconds"])
                            });
                        }
                    }
                }

                FileLogger.Log(FileLogger.LogLevel.Info, $"Carregados {timers.Count} temporizadores do banco");
            }
            catch (Exception ex)
            {
                // FALHA NA CONSULTA - Registrar erro mas não interromper aplicação
                FileLogger.Log(FileLogger.LogLevel.Error, "Erro ao carregar temporizadores", ex.ToString());
                
                // Fallback: retornar lista vazia em caso de erro
                return new List<TimerModel>();
            }

            return timers;
        }

        // OPERAÇÃO DE LEITURA - Obter temporizador específico
        public static TimerModel? GetTimer(int id)
        {
            try
            {
                // CONSULTA ESPECÍFICA - Buscar por ID
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string selectSql = "SELECT * FROM Timers WHERE Id = @Id";

                    using (var command = new SQLiteCommand(selectSql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new TimerModel
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = reader["Name"].ToString(),
                                    TimeInSeconds = Convert.ToInt32(reader["TimeInSeconds"]),
                                    HasSoundNotification = Convert.ToInt32(reader["HasSoundNotification"]) == 1,
                                    ShouldRepeat = Convert.ToInt32(reader["ShouldRepeat"]) == 1,
                                    InitialSeconds = Convert.ToInt32(reader["InitialSeconds"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log(FileLogger.LogLevel.Error, $"Erro ao carregar temporizador {id}", ex.ToString());
            }

            return null;
        }

        // OPERAÇÃO DE EXCLUSÃO - Remover temporizador
        public static bool DeleteTimer(int id)
        {
            try
            {
                // EXCLUSÃO FÍSICA - Remover registro do banco
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string deleteSql = "DELETE FROM Timers WHERE Id = @Id";

                    using (var command = new SQLiteCommand(deleteSql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        int rowsAffected = command.ExecuteNonQuery();
                        
                        if (rowsAffected > 0)
                        {
                            FileLogger.Log(FileLogger.LogLevel.Info, $"Temporizador {id} excluído com sucesso");
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log(FileLogger.LogLevel.Error, $"Erro ao deletar temporizador {id}", ex.ToString());
            }

            return false;
        }

        // OPERAÇÃO DE VERIFICAÇÃO - Validar se nome já existe
        public static bool TimerNameExists(string name, int excludeId = 0)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string checkSql = "SELECT COUNT(*) FROM Timers WHERE Name = @Name AND Id != @ExcludeId";

                    using (var command = new SQLiteCommand(checkSql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", name.Trim());
                        command.Parameters.AddWithValue("@ExcludeId", excludeId);

                        long count = (long)command.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log(FileLogger.LogLevel.Error, $"Erro ao verificar nome do temporizador: {name}", ex.ToString());
                return false;
            }
        }

        // MÉTODO DE BACKUP - Criar cópia de segurança do banco
        public static void BackupDatabase(string backupPath = null)
        {
            try
            {
                if (backupPath == null)
                {
                    backupPath = Path.Combine(databasePath, $"backup_timers_{DateTime.Now:yyyyMMdd_HHmmss}.db");
                }

                if (File.Exists(databaseFile))
                {
                    File.Copy(databaseFile, backupPath, true);
                    FileLogger.Log(FileLogger.LogLevel.Info, $"Backup criado: {backupPath}");
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log(FileLogger.LogLevel.Error, "Falha ao criar backup", ex.ToString());
            }
        }
    }
}