using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using CronometroRegressivo.Core.Models;
using CronometroRegressivo.UI.Controls;
using CronometroRegressivo.UI.Themes;

namespace CronometroRegressivo.UI.Views
{
    public partial class MainForm : Form
    {
        private TimerPanel timerPanel;
        private TimerListPanel timerListPanel;
        private EditAddTimerPanel editAddTimerPanel;
        private CustomTitleBar titleBar;
        private bool isClosing = false;

        public MainForm()
        {
            // Configuração inicial do formulário
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(600, 400);
            this.MinimumSize = new Size(600, 20);
            this.BackColor = AppTheme.BackgroundColor;
            this.ForeColor = Color.FromArgb(220, 220, 220);
            this.DoubleBuffered = true;

            // Adiciona a barra de título personalizada
            titleBar = new CustomTitleBar(this);
            this.Controls.Add(titleBar);
            titleBar.BringToFront();

            // Inicializa os componentes
            InitializeUI();
            ShowTimerPanel();
        }

        private void InitializeUI()
        {
            timerPanel = new TimerPanel() 
            { 
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };

            timerListPanel = new TimerListPanel() 
            { 
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };

            editAddTimerPanel = new EditAddTimerPanel(new TimerModel()) 
            { 
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };

            // Configura os eventos
            timerPanel.MenuClicked += (s, e) => ShowTimerListPanel();
            timerPanel.TimerCompleted += (s, e) => HandleTimerCompletion();
            timerPanel.OnTimerCompleted += (s, e) => timerListPanel.NotifyTimerCompleted();
            
            timerListPanel.BackClicked += (s, e) => ShowTimerPanel();
            timerListPanel.TimerSelected += (s, timer) => ShowTimerPanelWithTimer(timer);
            timerListPanel.StartTimerRequested += (s, e) => timerPanel.StartTimer();
            
            timerListPanel.AddClicked += (s, e) => ShowEditAddTimerPanel(new TimerModel());
            timerListPanel.EditTimerClicked += (s, timer) => ShowEditAddTimerPanel(timer);
            
            editAddTimerPanel.BackClicked += (s, e) => ShowTimerListPanel();
            editAddTimerPanel.TimerSaved += (s, timer) => 
            {
                timerListPanel.RefreshList();
                ShowTimerListPanel();
            };

            // Adiciona os controles ao formulário
            this.Controls.Add(timerPanel);
            this.Controls.Add(timerListPanel);
            this.Controls.Add(editAddTimerPanel);

            // Garante que a barra de título fique sempre no topo
            titleBar.BringToFront();
        }

        private void HandleTimerCompletion()
        {
            // Aqui você pode adicionar lógica adicional quando o timer completar
            // Por exemplo: mostrar notificação, tocar som adicional, etc.
            Console.WriteLine("Timer completado!");
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (var brush = new SolidBrush(AppTheme.BackgroundColor))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }

            using (var pen = new Pen(Color.FromArgb(80, 80, 80), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        private void ShowTimerPanel()
        {
            timerListPanel.Hide();
            editAddTimerPanel.Hide();
            timerPanel.Show();
            timerPanel.BringToFront();
            titleBar.BringToFront();
        }

        private void ShowTimerListPanel()
        {
            timerPanel.Hide();
            editAddTimerPanel.Hide();
            timerListPanel.Show();
            timerListPanel.BringToFront();
            timerListPanel.RefreshList();
            titleBar.BringToFront();
        }

        private void ShowEditAddTimerPanel(TimerModel timer)
        {
            timerPanel.Hide();
            timerListPanel.Hide();
            
            if (editAddTimerPanel != null)
            {
                this.Controls.Remove(editAddTimerPanel);
                editAddTimerPanel.Dispose();
            }

            editAddTimerPanel = new EditAddTimerPanel(timer) 
            { 
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };

            editAddTimerPanel.BackClicked += (s, e) => ShowTimerListPanel();
            editAddTimerPanel.TimerSaved += (s, t) => 
            {
                timerListPanel.RefreshList();
                ShowTimerListPanel();
            };

            this.Controls.Add(editAddTimerPanel);
            editAddTimerPanel.Show();
            editAddTimerPanel.BringToFront();
            titleBar.BringToFront();
        }

        private void ShowTimerPanelWithTimer(TimerModel timer)
        {
            timerPanel.LoadTimer(timer);
            ShowTimerPanel();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isClosing) return;

            e.Cancel = true;
            isClosing = true;

            timerPanel?.Dispose();
            timerListPanel?.Dispose();
            editAddTimerPanel?.Dispose();
            titleBar?.Dispose();

            BeginInvoke(new Action(() =>
            {
                Close();
                Application.Exit();
            }));
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }
    }

    public class CustomTitleBar : Panel
    {
        private readonly MainForm _parentForm;
        private readonly Button _btnMinimize;
        private readonly Button _btnMaximize;
        private readonly Button _btnClose;

        public CustomTitleBar(MainForm parentForm)
        {
            _parentForm = parentForm;
            this.BackColor = AppTheme.TitleBarColor;
            this.Height = 40;
            this.Dock = DockStyle.Top;
            
            // Botão minimizar
            _btnMinimize = new Button
            {
                Text = "─",
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Dock = DockStyle.Right,
                Width = 46,
                Height = 40,
                TabStop = false
            };
            _btnMinimize.FlatAppearance.BorderSize = 0;
            _btnMinimize.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 80, 80);
            _btnMinimize.Click += (s, e) => _parentForm.WindowState = FormWindowState.Minimized;
            
            // Botão maximizar/restaurar
            _btnMaximize = new Button
            {
                Text = "□",
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Dock = DockStyle.Right,
                Width = 46,
                Height = 40,
                TabStop = false
            };
            _btnMaximize.FlatAppearance.BorderSize = 0;
            _btnMaximize.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 80, 80);
            _btnMaximize.Click += (s, e) => 
            {
                _parentForm.WindowState = _parentForm.WindowState == FormWindowState.Maximized 
                    ? FormWindowState.Normal 
                    : FormWindowState.Maximized;
                UpdateMaximizeButton();
            };
            
            // Botão fechar
            _btnClose = new Button
            {
                Text = "×",
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Dock = DockStyle.Right,
                Width = 46,
                Height = 40,
                TabStop = false
            };
            _btnClose.FlatAppearance.BorderSize = 0;
            _btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 17, 35);
            _btnClose.Click += (s, e) => _parentForm.Close();
            
            // Adiciona os controles
            this.Controls.Add(_btnClose);
            this.Controls.Add(_btnMaximize);
            this.Controls.Add(_btnMinimize);
            
            // Evento para arrastar a janela
            this.MouseDown += TitleBar_MouseDown;
            
            // Atualiza o texto do botão maximizar
            UpdateMaximizeButton();
        }

        private void UpdateMaximizeButton()
        {
            _btnMaximize.Text = _parentForm.WindowState == FormWindowState.Maximized ? "❐" : "□";
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(_parentForm.Handle, NativeMethods.WM_NCLBUTTONDOWN, NativeMethods.HT_CAPTION, 0);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Desenha o título do formulário
            using (var brush = new SolidBrush(Color.White))
            {
                e.Graphics.DrawString(_parentForm.Text, new Font("Segoe UI", 10), brush, new PointF(10, 12));
            }
        }
    }

    internal static class NativeMethods
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
    }
}