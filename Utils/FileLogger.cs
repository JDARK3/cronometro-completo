// FileLogger.cs (para referência)
using System;
using System.IO;
using System.Linq;

namespace CronometroRegressivo.Utils
{
    // ETAPA 2 - SISTEMA DE REGISTRO DE LOGS
    // Implementar logging robusto para diagnóstico de problemas
    public static class FileLogger
    {
        // NÍVEIS DE LOG - Definir severidade das mensagens
        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error
        }

        // CONFIGURAÇÃO - Parâmetros do sistema de log
        private static readonly string LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CronometroRegressivo",
            "Logs");

        private static readonly int MaxLogFiles = 7;
        private static readonly int MaxLogAgeDays = 30;

        // REGISTRO DE MENSAGEM - Método principal de logging
        public static void Log(LogLevel level, string message, string details = null)
        {
            try
            {
                // PREPARAÇÃO DE DIRETÓRIO - Garantir que pasta de logs existe
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }

                // CRIAÇÃO DE ARQUIVO - Usar data no nome do arquivo
                string logFile = Path.Combine(LogDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt");
                string logMessage = FormatLogMessage(level, message, details);

                // ESCRITA SEGURA - Append com tratamento de exceções
                File.AppendAllText(logFile, logMessage + Environment.NewLine);
            }
            catch
            {
                // FALHA SILENCIOSA - Não quebrar aplicação se logging falhar
            }
        }

        // FORMATAÇÃO DE MENSAGEM - Estrutura padrão para logs
        private static string FormatLogMessage(LogLevel level, string message, string details)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string levelStr = level.ToString().ToUpper().PadRight(7);
            
            string logEntry = $"{timestamp} [{levelStr}] {message}";
            
            if (!string.IsNullOrEmpty(details))
            {
                logEntry += $"\n\tDetails: {details}";
            }
            
            return logEntry;
        }

        // LIMPEZA DE LOGS - Remover arquivos antigos automaticamente
        public static void CleanOldLogs()
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                    return;

                // LIMPEZA POR IDADE - Remover logs muito antigos
                var cutoffDate = DateTime.Now.AddDays(-MaxLogAgeDays);
                foreach (var file in Directory.GetFiles(LogDirectory, "log_*.txt"))
                {
                    try
                    {
                        if (File.GetCreationTime(file) < cutoffDate)
                        {
                            File.Delete(file);
                        }
                    }
                    catch
                    {
                        // Ignorar arquivos inacessíveis
                    }
                }

                // LIMITE DE ARQUIVOS - Manter apenas os mais recentes
                var logFiles = Directory.GetFiles(LogDirectory, "log_*.txt")
                    .OrderByDescending(f => f)
                    .ToList();

                while (logFiles.Count > MaxLogFiles)
                {
                    File.Delete(logFiles.Last());
                    logFiles.RemoveAt(logFiles.Count - 1);
                }
            }
            catch
            {
                // FALHA SILENCIOSA - Não interromper aplicação
            }
        }
    }
}