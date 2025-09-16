// Program.cs (APENAS ADICIONAR LOG DE INICIALIZAÇÃO)
using CronometroRegressivo.Utils;
using System;
using System.Windows.Forms;
using CronometroRegressivo.UI.Views;
// REMOVER using CronometroRegressivo.Core.Data; - Não é mais necessário

namespace CronometroRegressivo
{
    // ETAPA 1 - PONTO DE ENTRADA DA APLICAÇÃO
    static class Program
    {
     
        static void Main()
        {
            // CONFIGURAÇÃO INICIAL - Primeiras linhas a serem escritas
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // SISTEMA DE ERROS - Implementar early para capturar falhas
            ErrorHandler.Initialize();
            
            // MANUTENÇÃO - Configurar limpeza automática de logs
            FileLogger.CleanOldLogs();

            try
            {
                // BANCO DE DADOS - Inicialização automática via TimerManager
                // O construtor estático do TimerManager já cuida disso
                FileLogger.Log(FileLogger.LogLevel.Info, "Aplicação iniciada - Banco será inicializado automaticamente");
                
                // LOOP PRINCIPAL - Iniciar a interface do usuário
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                // TRATAMENTO DE FALHA - Capturar erros de inicialização
                FileLogger.Log(FileLogger.LogLevel.Error, "Falha na inicialização", ex.ToString());
                MessageBox.Show($"Erro ao iniciar aplicação: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // LOG FINAL - Garantir registro do encerramento
                FileLogger.Log(FileLogger.LogLevel.Info, "Aplicação encerrada");
            }
        }
    }
}