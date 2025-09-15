using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using CronometroRegressivo.Core.Managers;
using CronometroRegressivo.Core.Models;
using CronometroRegressivo.UI.Themes;

namespace CronometroRegressivoApp.Views
{
    public partial class TimerSelectionView : UserControl
    {
        public event EventHandler? BackClicked;
        public event EventHandler? AddClicked;
        public event EventHandler? SavedClicked;
        public event EventHandler<string>? TimerSelected;
        
        private readonly ListBox listTimers = new ListBox();
        private readonly Button btnBack = new Button();
        private readonly Button btnAdd = new Button();
        private readonly Button btnSaved = new Button();

        public TimerSelectionView()
        {
            InitializeComponents();
            SetupSelectionUI();
        }

        public void LoadTimers()
        {
            listTimers.Items.Clear();
            var timers = TimerManager.GetTimers();
            
            foreach (var timer in timers)
            {
                string timerInfo = $"{timer.Name} - {TimeSpan.FromSeconds(timer.TimeInSeconds):hh\\:mm\\:ss}";
                if (timer.HasSoundNotification)
                {
                    timerInfo += " (🔔)";
                }
                listTimers.Items.Add(timerInfo);
            }
        }

        private void InitializeComponents()
        {
            this.BackColor = AppTheme.BackgroundColor;
            this.Size = new Size(545, 295);
        }

        private void SetupSelectionUI()
        {
            // Configura ListBox
            listTimers.BackColor = Color.FromArgb(30, 30, 40);
            listTimers.ForeColor = Color.White;
            listTimers.Font = new Font("Segoe UI", 12);
            listTimers.Size = new Size(400, 200);
            listTimers.Location = new Point(70, 40);
            listTimers.DoubleClick += (s, e) => 
            {
                if (listTimers.SelectedItem is string selectedItem)
                {
                    string timerName = selectedItem.Split('-')[0].Trim();
                    if (!string.IsNullOrEmpty(timerName))
                    {
                        TimerSelected?.Invoke(this, timerName);
                    }
                }
            };

            // Configura botões
            ConfigureButton(btnBack, "←", new Point(20, 20), "Voltar");
            ConfigureButton(btnAdd, "+", new Point(80, 20), "Adicionar");
            ConfigureButton(btnSaved, "★", new Point(140, 20), "Salvos");

            // Eventos dos botões
            btnBack.Click += (s, e) => BackClicked?.Invoke(this, EventArgs.Empty);
            btnAdd.Click += (s, e) => AddClicked?.Invoke(this, EventArgs.Empty);
            btnSaved.Click += (s, e) => SavedClicked?.Invoke(this, EventArgs.Empty);

            this.Controls.AddRange(new Control[] { listTimers, btnBack, btnAdd, btnSaved });
        }

        private void ConfigureButton(Button btn, string text, Point location, string tooltip)
        {
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 14);
            btn.Size = new Size(40, 40);
            btn.Location = location;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.Transparent;
            btn.ForeColor = AppTheme.TextColor;
            btn.Cursor = Cursors.Hand;
        }

        public void RefreshList()
        {
            LoadTimers();
        }
    }
}