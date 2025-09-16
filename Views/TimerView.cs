using System;
using System.Drawing;
using System.Windows.Forms;
using CronometroRegressivo.UI.Themes;

namespace CronometroRegressivoApp.Views
{
    public partial class TimerView : UserControl
    {
        public event EventHandler? MenuClicked;

        private readonly Button btnPlay = new Button();
        private readonly Button btnPause = new Button();
        private readonly Button btnReset = new Button();
        private readonly Button btnMenu = new Button();
        private readonly Label lblTimer = new Label();
        private readonly Label lblTimerName = new Label();
        private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private TimeSpan currentTime;
        private bool isRunning;
        private bool isCountdown = false;

        public TimerView()
        {
            InitializeComponents();
            SetupTimerUI();
            InitializeTimer();
        }

        private void InitializeComponents()
        {
            currentTime = TimeSpan.Zero;
            isRunning = false;

            this.BackColor = AppTheme.BackgroundColor;
            this.Size = new Size(545, 295);
        }

        private void InitializeTimer()
        {
            timer.Interval = 1000;
            timer.Tick += (s, e) => Timer_Tick(s!, e);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (isRunning)
            {
                if (isCountdown && currentTime > TimeSpan.Zero)
                {
                    currentTime = currentTime.Add(TimeSpan.FromSeconds(-1));
                    UpdateTimerDisplay();

                    if (currentTime == TimeSpan.Zero)
                    {
                        TimerCompleted();
                    }
                }
                else if (!isCountdown)
                {
                    currentTime = currentTime.Add(TimeSpan.FromSeconds(1));
                    UpdateTimerDisplay();
                }
            }
        }

        private void TimerCompleted()
        {
            PauseTimer();
            // Som personalizado será tocado pelo TimerPanel, não há necessidade de som aqui
        }

        private void UpdateTimerDisplay()
        {
            lblTimer.Text = currentTime.ToString(@"hh\:mm\:ss");
        }

        public void SetTimer(string name, TimeSpan time)
        {
            currentTime = time;
            isCountdown = true;
            lblTimerName.Text = name;
            UpdateTimerDisplay();
            PauseTimer();
        }

        private void SetupTimerUI()
        {
            // Configuração dos botões
            ConfigureButton(btnPlay, "▶", new Point(180, 220), "Iniciar");
            ConfigureButton(btnPause, "⏸", new Point(310, 220), "Pausar");
            ConfigureButton(btnReset, "↻", new Point(440, 220), "Resetar");
            ConfigureButton(btnMenu, "≡", new Point(20, 20), "Menu");

            // Label do Nome do Timer
            lblTimerName.Text = "";
            lblTimerName.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTimerName.ForeColor = AppTheme.TextColor;
            lblTimerName.Location = new Point(100, 70);
            lblTimerName.Size = new Size(400, 30);
            lblTimerName.TextAlign = ContentAlignment.MiddleCenter;

            // Label do Timer
            lblTimer.Text = "00:00:00";
            lblTimer.Font = AppTheme.TimerFont;
            lblTimer.ForeColor = AppTheme.TextColor;
            lblTimer.Location = new Point(100, 100);
            lblTimer.Size = new Size(400, 76);
            lblTimer.TextAlign = ContentAlignment.MiddleCenter;

            // Configura estados iniciais
            btnPause.Enabled = false;

            // Adiciona controles
            this.Controls.AddRange(new Control[] { lblTimerName, lblTimer, btnPlay, btnPause, btnReset, btnMenu });

            // Configura eventos
            btnMenu.Click += (s, e) => MenuClicked?.Invoke(this, EventArgs.Empty);
            btnPlay.Click += (s, e) => StartTimer();
            btnPause.Click += (s, e) => PauseTimer();
            btnReset.Click += (s, e) => ResetTimer();
        }

        private void StartTimer()
        {
            if (!isRunning)
            {
                if (isCountdown && currentTime == TimeSpan.Zero)
                    return;

                timer.Start();
                isRunning = true;
                btnPlay.Enabled = false;
                btnPause.Enabled = true;
            }
        }

        private void PauseTimer()
        {
            if (isRunning)
            {
                timer.Stop();
                isRunning = false;
                btnPlay.Enabled = true;
                btnPause.Enabled = false;
            }
        }

        private void ResetTimer()
        {
            timer.Stop();
            currentTime = isCountdown ? currentTime : TimeSpan.Zero;
            isRunning = false;
            UpdateTimerDisplay();
            btnPlay.Enabled = true;
            btnPause.Enabled = false;

            if (!isCountdown)
                lblTimerName.Text = "";
        }

        private void ConfigureButton(Button btn, string text, Point location, string tooltip)
        {
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 14);
            btn.Size = new Size(60, 60);
            btn.Location = location;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.Transparent;
            btn.ForeColor = AppTheme.TextColor;
            btn.Cursor = Cursors.Hand;
        }
    }
}