// ErrorHandler.cs (para referência)
using System;
using System.Windows.Forms;

namespace CronometroRegressivo.Utils
{
    // ETAPA 3 - SISTEMA DE TRATAMENTO DE ERROS GLOBAL
    // Capturar exceções não tratadas para evitar crashes
    public static class ErrorHandler
    {
        // INICIALIZAÇÃO - Configurar handlers globais de exceção
        public static void Initialize()
        {
            // EXCEÇÕES DE UI - Erros na thread de interface
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;

            // EXCEÇÕES GERAIS - Erros em outras threads
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        // HANDLER DE ERROS DE UI - Exceções na thread principal
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception, "UI Thread");
        }

        // HANDLER DE ERROS GERAIS - Exceções em threads secundárias
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                HandleException(ex, "Background Thread");
            }
        }

        // TRATAMENTO CENTRALIZADO - Processar todas as exceções
        private static void HandleException(Exception ex, string source)
        {
            try
            {
                // REGISTRO DE ERRO - Log detalhado da exceção
                FileLogger.Log(FileLogger.LogLevel.Error, 
                    $"Exceção não tratada em {source}", 
                    ex.ToString());

                // NOTIFICAÇÃO AO USUÁRIO - Dialog amigável (apenas na UI thread)
                if (source == "UI Thread")
                {
                    MessageBox.Show(
                        $"Ocorreu um erro inesperado:\n{ex.Message}\n\nOs detalhes foram registrados para análise.",
                        "Erro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch
            {
                // FALHA NO TRATAMENTO - Último recurso para evitar loop infinito
            }
        }
    }
}