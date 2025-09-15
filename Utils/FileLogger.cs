using System;
using System.IO;

namespace CronometroRegressivo.Utils
{
    /// <summary>
    /// Classe estática para registro de logs em arquivos.
    /// Implementa um sistema simples de log com níveis de severidade e rotação por data.
    /// </summary>
    public static class FileLogger
    {
        // Diretório base onde os logs serão armazenados (readonly para imutabilidade)
        private static readonly string _logDirectory = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, // Pega o diretório da aplicação
            "logs" // Subpasta para os logs
        );

        /// <summary>
        /// Níveis de severidade para os logs.
        /// </summary>
        public enum LogLevel 
        { 
            Info,     // Para mensagens informativas
            Warning,  // Para alertas não críticos
            Error     // Para erros e exceções
        }

        /// <summary>
        /// Registra uma mensagem no arquivo de log.
        /// </summary>
        /// <param name="level">Nível de severidade (Info, Warning, Error)</param>
        /// <param name="message">Mensagem principal do log</param>
        /// <param name="details">Detalhes adicionais (opcional)</param>
        public static void Log(LogLevel level, string message, string? details = null)
        {
            try
            {
                // Garante que o diretório de logs existe
                Directory.CreateDirectory(_logDirectory);
                
                // Cria um arquivo de log com o padrão app_YYYYMMDD.log
                string logFile = Path.Combine(_logDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");
                
                // Formata a mensagem de log com:
                // - Horário atual
                // - Nível do log (em maiúsculas)
                // - Mensagem principal
                // - Detalhes (se fornecidos)
                File.AppendAllText(logFile,
                    $"[{DateTime.Now:HH:mm:ss}] {level.ToString().ToUpper()}\n" +
                    $"Message: {message}\n" +
                    $"{(details != null ? $"Details:\n{details}\n" : "")}\n");
            }
            catch 
            { 
                // Falha silenciosa para evitar que erros no logging quebrem a aplicação
                // Em um sistema real, poderia registrar no Event Viewer ou enviar para um serviço externo
            }
        }

        /// <summary>
        /// Limpa logs antigos com base na quantidade de dias a manter.
        /// </summary>
        /// <param name="daysToKeep">Número de dias de logs a preservar (padrão: 7)</param>
        public static void CleanOldLogs(int daysToKeep = 7)
        {
            try
            {
                // Procura por todos os arquivos de log no diretório
                foreach (var file in Directory.GetFiles(_logDirectory, "app_*.log"))
                {
                    // Verifica se a data de criação do arquivo é mais antiga que daysToKeep
                    if (File.GetCreationTime(file) < DateTime.Now.AddDays(-daysToKeep))
                    {
                        File.Delete(file); // Remove o arquivo antigo
                    }
                }
            }
            catch 
            { 
                // Falha silenciosa para evitar problemas se não puder excluir os logs
            }
        }
    }
}