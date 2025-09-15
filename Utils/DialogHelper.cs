using System.Windows.Forms;

namespace CronometroRegressivo.Utils
{
    // Classe utilitária estática para exibição de diálogos
    // Segue o princípio DRY (Don't Repeat Yourself) - centraliza a criação de diálogos
    public static class DialogHelper
    {
        // Método para exibir uma mensagem de erro personalizada
        // Parâmetros opcionais demonstram flexibilidade na API
        public static void ShowError(string title, string message, string? details = null)
        {
            // Cria e configura o formulário de erro
            // Uso do 'using' garante liberação adequada de recursos (padrão Dispose)
            using var form = new Form
            {
                Text = title,
                Size = new Size(500, 300),
                FormBorderStyle = FormBorderStyle.FixedDialog, // Diálogo não redimensionável
                StartPosition = FormStartPosition.CenterScreen // Centraliza na tela
            };

            // Label para a mensagem principal
            var label = new Label
            {
                Text = message,
                Dock = DockStyle.Top, // Fixa no topo do formulário
                Padding = new Padding(10), // Espaçamento interno
                AutoSize = true // Ajusta tamanho automaticamente
            };

            // TextBox para detalhes do erro
            var textBox = new TextBox
            {
                Text = details ?? "Nenhum detalhe adicional disponível", // Operador de coalescência nula
                Multiline = true, // Permite múltiplas linhas
                ReadOnly = true, // Somente leitura
                ScrollBars = ScrollBars.Vertical, // Barra de rolagem vertical
                Dock = DockStyle.Fill, // Preenche o espaço restante
                BackColor = SystemColors.Window // Cor padrão de fundo
            };

            // Botão de confirmação
            var button = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK, // Fecha o diálogo com resultado OK
                Dock = DockStyle.Bottom // Fixa na base do formulário
            };

            // Adiciona todos os controles ao formulário
            form.Controls.AddRange(new Control[] { label, textBox, button });
            
            // Exibe o diálogo de forma modal
            form.ShowDialog();
        }

        // Método para exibir diálogo de confirmação (Sim/Não)
        // Parâmetro opcional com valor padrão demonstra boa prática de API design
        public static bool Confirm(string message, string title = "Confirmação")
        {
            // Usa MessageBox nativo do Windows Forms
            // Retorna true se a resposta for 'Sim'
            return MessageBox.Show(
                message,
                title,
                MessageBoxButtons.YesNo, // Botões Sim/Não
                MessageBoxIcon.Question // Ícone de interrogação
            ) == DialogResult.Yes;
        }
    }
}