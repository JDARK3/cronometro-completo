using System;
using System.Drawing;
using System.Windows.Forms;
using CronometroRegressivo.Core.Managers;
using CronometroRegressivo.Core.Models;
using CronometroRegressivo.UI.Themes;

namespace CronometroRegressivoApp.Views
{
    public partial class AddTimerView : UserControl
    {
        public event EventHandler? BackClicked;
        
        private readonly Button btnBack = new Button();
        private readonly Button btnSave = new Button();
        private readonly TextBox txtName = new TextBox();
        private readonly NumericUpDown numHours = new NumericUpDown();
        private readonly NumericUpDown numMinutes = new NumericUpDown();
        private readonly NumericUpDown numSeconds = new NumericUpDown();

        public AddTimerView()
        {
            InitializeComponents();
            SetupAddTimerUI();
        }

        private void InitializeComponents()
        {
            this.BackColor = AppTheme.BackgroundColor;
            this.Size = new Size(545, 295);
        }

        private void SetupAddTimerUI()
        {
            // ... (código existente mantido) ...
        }

        private void SaveTimer()
        {
            if (!string.IsNullOrWhiteSpace(txtName.Text))
            {
                int totalSeconds = (int)numHours.Value * 3600 + 
                                 (int)numMinutes.Value * 60 + 
                                 (int)numSeconds.Value;
                
                var timer = new TimerModel {
                    Name = txtName.Text,
                    TimeInSeconds = totalSeconds,
                    HasSoundNotification = true
                };
                
                TimerManager.SaveTimer(timer);
                BackClicked?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}