using System;
using System.IO;
using System.Windows.Forms;

namespace CronometroRegressivo.Utils
{
    /// <summary>
    /// Classe estática para tratamento centralizado de exceções.
    /// Implementa captura de exceções não tratadas e registro em log.
    /// </summary>
    public static class ErrorHandler
    {
        // Caminho do arquivo de log (readonly para imutabilidade após inicialização)
        private static readonly string _logPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory ?? Directory.GetCurrentDirectory(),
            "logs",
            "error_log.txt"
        );

        /// <summary>
        /// Inicializa os handlers de exceção global.
        /// </summary>
        public static void Initialize()
        {
            // Registra handlers para exceções não tratadas:
            // 1. Exceções em threads não gerenciadas (UnhandledException)
            // 2. Exceções na thread principal da UI (ThreadException)
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
            Application.ThreadException += HandleThreadException;

            // Garante que o diretório de logs existe (não implementado aqui, mas seria uma boa prática)
            // Exemplo: Directory.CreateDirectory(Path.GetDirectoryName(_logPath));
        }

        /// <summary>
        /// Manipula exceções não tratadas em threads não gerenciadas.
        /// </summary>
        /// <param name="sender">Objeto que disparou a exceção.</param>
        /// <param name="e">Argumentos contendo a exceção.</param>
        private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            LogError(ex); // Registra o erro em log
            ShowErrorDialog(ex, isFatal: e.IsTerminating); // Exibe diálogo de erro
        }

        /// <summary>
        /// Manipula exceções não tratadas na thread principal da UI.
        /// </summary>
        /// <param name="sender">Objeto que disparou a exceção.</param>
        /// <param name="e">Argumentos contendo a exceção.</param>
        private static void HandleThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            LogError(e.Exception); // Registra o erro em log
            ShowErrorDialog(e.Exception, isFatal: false); // Exibe diálogo de erro (não fatal)
        }

        /// <summary>
        /// Registra a exceção em um arquivo de log.
        /// </summary>
        /// <param name="ex">Exceção a ser registrada.</param>
        private static void LogError(Exception ex)
        {
            try
            {
                // Formato do log:
                // [Data/Hora]
                // Tipo: NomeDaExceção
                // Mensagem: Descrição do erro
                // Stack Trace: Detalhes do erro
                File.AppendAllText(_logPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]\n" +
                    $"Type: {ex.GetType().Name}\n" +
                    $"Message: {ex.Message}\n" +
                    $"Stack Trace:\n{ex.StackTrace}\n\n");
            }
            catch
            {
                // Falha silenciosa para evitar loops de erro no log.
                // Em produção, poderia ser registrado em um fallback (ex: Event Viewer).
            }
        }

        /// <summary>
        /// Exibe um diálogo de erro para o usuário.
        /// </summary>
        /// <param name="ex">Exceção ocorrida.</param>
        /// <param name="isFatal">Indica se o erro é fatal (aplicação será encerrada).</param>
        private static void ShowErrorDialog(Exception ex, bool isFatal)
        {
            // Usa a classe DialogHelper para exibir o erro de forma padronizada
            DialogHelper.ShowError(
                title: isFatal ? "Erro Fatal" : "Erro",
                message: $"{(isFatal ? "A aplicação será encerrada.\n\n" : "")}" +
                         $"Erro: {ex.GetType().Name}\n" +
                         $"Detalhes: {ex.Message}",
                details: ex.StackTrace
            );

            // Se for um erro fatal, encerra a aplicação com código de erro 1
            if (isFatal)
            {
                Environment.Exit(1);
            }
        }
    }
}