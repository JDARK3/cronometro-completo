using CronometroRegressivo.Core.Managers;
using CronometroRegressivo.Core.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CronometroRegressivo.UI.Views
{
    // Formulário de configuração do temporizador
    // Herda da classe Form do Windows Forms
    public partial class ConfigForm : Form
    {
        // Propriedade pública que expõe o temporizador selecionado (pode ser nulo)
        // O setter privado garante o encapsulamento - só pode ser modificado dentro desta classe
        public TimerModel? SelectedTimer { get; private set; }

        // Controles da interface declarados como campos readonly (somente leitura)
        // Isso garante que não serão reatribuídos após a inicialização
        private readonly ListBox _listBox = new ListBox();
        private readonly TextBox _txtName = new TextBox();
        private readonly NumericUpDown _numHours = new NumericUpDown();
        private readonly NumericUpDown _numMinutes = new NumericUpDown();
        private readonly NumericUpDown _numSeconds = new NumericUpDown();
        private readonly CheckBox _chkSound = new CheckBox();
        private readonly Button _btnSave = new Button();
        private readonly Button _btnCancel = new Button();

        // Construtor que recebe opcionalmente um temporizador para edição
        // Isso demonstra injeção de dependência - o formulário pode receber suas dependências
        public ConfigForm(TimerModel? timerToEdit = null)
        {
            InitializeComponent(); // Inicialização padrão do Windows Forms
            InitializeUI();        // Configura a interface do usuário
            LoadTimerList();       // Carrega a lista de temporizadores

            // Se foi fornecido um temporizador para edição, carrega seus dados
            if (timerToEdit != null)
            {
                LoadTimerData(timerToEdit);
            }
        }

        // Método para configurar as propriedades básicas do formulário
        private void InitializeComponent()
        {
            this.Text = "Gerenciar Temporizador";
            this.ClientSize = new Size(450, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog; // Diálogo fixo (não redimensionável)
            this.MaximizeBox = false; // Desabilita o botão de maximizar
            this.StartPosition = FormStartPosition.CenterParent; // Centraliza em relação ao form pai
        }

        // Configura todos os controles da interface do usuário
        private void InitializeUI()
        {
            // ListBox para seleção de temporizadores
            _listBox.Location = new Point(20, 20);
            _listBox.Size = new Size(410, 150);
            _listBox.DisplayMember = "Name"; // Mostra a propriedade Name dos itens
            
            // Evento que é disparado quando seleciona um item na lista
            // Usa pattern matching para verificar se o item é um TimerModel
            _listBox.SelectedIndexChanged += (s, e) =>
            {
                if (_listBox.SelectedItem is TimerModel timer)
                {
                    LoadTimerData(timer);
                }
            };

            // Configuração do campo de texto para o nome do temporizador
            _txtName.PlaceholderText = "Nome do Temporizador";
            _txtName.Location = new Point(20, 180);
            _txtName.Size = new Size(410, 25);

            // Configuração dos campos numéricos para horas (0-23)
            _numHours.Minimum = 0;
            _numHours.Maximum = 23;
            _numHours.Location = new Point(20, 220);
            _numHours.Width = 80;

            // Configuração dos campos numéricos para minutos (0-59)
            _numMinutes.Minimum = 0;
            _numMinutes.Maximum = 59;
            _numMinutes.Location = new Point(120, 220);
            _numMinutes.Width = 80;

            // Configuração dos campos numéricos para segundos (0-59)
            _numSeconds.Minimum = 0;
            _numSeconds.Maximum = 59;
            _numSeconds.Location = new Point(220, 220);
            _numSeconds.Width = 80;

            // Checkbox para ativar/desativar som de notificação
            _chkSound.Text = "Notificação Sonora";
            _chkSound.Location = new Point(20, 260);
            _chkSound.Width = 150;

            // Botão Salvar - usa um método separado como handler do evento
            _btnSave.Text = "Salvar";
            _btnSave.Location = new Point(250, 320);
            _btnSave.Click += BtnSave_Click;

            // Botão Cancelar - usa lambda expression para fechar o form
            _btnCancel.Text = "Cancelar";
            _btnCancel.Location = new Point(350, 320);
            _btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            // Adiciona todos os controles ao formulário
            this.Controls.AddRange(new Control[] {
                _listBox,
                _txtName,
                _numHours, _numMinutes, _numSeconds,
                _chkSound,
                _btnSave, _btnCancel
            });
        }

        // Carrega a lista de temporizadores disponíveis
        private void LoadTimerList()
        {
            // Obtém a lista de temporizadores do TimerManager
            // Demonstra o padrão de injeção de dependência indireta
            _listBox.DataSource = TimerManager.GetTimers();
        }

        // Carrega os dados de um temporizador nos controles do formulário
        private void LoadTimerData(TimerModel timer)
        {
            _txtName.Text = timer.Name;
            // Converte o tempo total em segundos para horas, minutos e segundos
            _numHours.Value = timer.TimeInSeconds / 3600; // Calcula as horas
            _numMinutes.Value = (timer.TimeInSeconds % 3600) / 60; // Calcula os minutos restantes
            _numSeconds.Value = timer.TimeInSeconds % 60; // Calcula os segundos restantes
            _chkSound.Checked = timer.HasSoundNotification;
        }

        // Manipulador do evento de clique no botão Salvar
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            // Validação simples - verifica se o nome não está vazio
            if (string.IsNullOrWhiteSpace(_txtName.Text))
            {
                MessageBox.Show("Digite um nome válido para o temporizador.");
                return;
            }

            // Cria um novo TimerModel com os dados do formulário
            SelectedTimer = new TimerModel
            {
                Name = _txtName.Text,
                // Converte horas, minutos e segundos para segundos totais
                TimeInSeconds = (int)_numHours.Value * 3600 + // Horas para segundos
                               (int)_numMinutes.Value * 60 +  // Minutos para segundos
                               (int)_numSeconds.Value,        // Segundos
                HasSoundNotification = _chkSound.Checked
            };

            // Define o resultado do diálogo como OK e fecha o formulário
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}