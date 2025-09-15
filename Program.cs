using CronometroRegressivo.Utils;
using System;
using System.Windows.Forms;
using CronometroRegressivo.UI.Views;

namespace CronometroRegressivo
{
    // Classe principal que inicia a aplicação Windows Forms
    static class Program
    {
        // Atributo STAThread é obrigatório para aplicações Windows Forms
        // Indica que o modelo de threading é Single-Threaded Apartment (STA)
        [STAThread]
        static void Main()
        {
            // Configuração padrão do Windows Forms para melhor renderização visual
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Inicializa o tratamento global de erros não capturados
            ErrorHandler.Initialize();
            
            // Limpa logs antigos (mantém apenas os últimos 7 dias por padrão)
            FileLogger.CleanOldLogs();

            try
            {
                // Registra no log que a aplicação está iniciando
                FileLogger.Log(FileLogger.LogLevel.Info, "Aplicação iniciada");
                
                // Cria e executa o formulário principal (MainForm)
                // Application.Run inicia o loop de mensagens do Windows Forms
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                // Captura qualquer exceção não tratada e:
                // 1. Registra no log com nível de erro
                // 2. Exibe uma mensagem amigável ao usuário
                FileLogger.Log(FileLogger.LogLevel.Error, "Falha na inicialização", ex.ToString());
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Sempre registra o encerramento da aplicação no log
                FileLogger.Log(FileLogger.LogLevel.Info, "Aplicação encerrada");
            }
        }
    }
}