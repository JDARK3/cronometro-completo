using CronometroRegressivo.UI.Controls;

namespace CronometroRegressivo.UI.Views
{
    // Formulário principal que exibe o temporizador
    // Herda da classe Form do Windows Forms
    public partial class TimerForm : Form
    {
        // Painel do temporizador (controle customizado)
        // Declarado como readonly pois não será alterado após a inicialização
        private readonly TimerPanel _timerPanel = new TimerPanel();

        // Construtor da classe
        public TimerForm()
        {
            // Observação: InitializeComponent() foi removido pois não está sendo usado o Designer
            // Isso é comum quando a UI é construída totalmente via código

            // Adiciona o painel do temporizador ao formulário
            this.Controls.Add(_timerPanel);

            // Configura o dock para preencher todo o espaço disponível
            _timerPanel.Dock = DockStyle.Fill;

            // Assina o evento MenuClicked usando uma expressão lambda
            // Demonstra o padrão observer para lidar com eventos
            _timerPanel.MenuClicked += (s, e) => OpenConfigForm();
        }

        // Método para abrir o formulário de configuração
        private void OpenConfigForm()
        {
            // Usa o bloco 'using' para garantir que o recurso será liberado
            // Demonstra o padrão Dispose para gerenciamento de recursos
            using var configForm = new ConfigForm();

            // Mostra o formulário de configuração como diálogo modal
            // Verifica se o resultado foi OK e se há um temporizador selecionado
            if (configForm.ShowDialog() == DialogResult.OK && configForm.SelectedTimer != null)
            {
                // Atualiza o painel do temporizador com os novos valores
                // Demonstra comunicação entre formulários
                _timerPanel.SetTimer(
                    configForm.SelectedTimer.Name,
                    configForm.SelectedTimer.GetTimeSpan()
                );
            }
        }
        
    }
}