using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using CronometroRegressivo.Core.Models;
using NAudio.Wave;

namespace CronometroRegressivo.UI.Controls
{
    public partial class TimerPanel : UserControl
    {
        public event EventHandler MenuClicked;
        public event EventHandler TimerCompleted;
        public event EventHandler OnTimerCompleted;
        public event EventHandler<bool> SoundToggled; // Novo evento para notificar mudança de som

        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private TimerModel currentTimerModel;
        private TimeSpan currentTime;
        private bool isRunning;
        private bool isCountdownMode;
        private bool countdownSoundPlayed = false;

        private Image playIcon, pauseIcon, resetIcon, menuIcon, backgroundImage;
        private Rectangle btnPlayBounds, btnPauseBounds, btnResetBounds, btnMenuBounds, btnSoundBounds;
        private Font timerFont;
        private Point mousePosition;
        private float soundVolume = 1.0f;
        private bool soundEnabled = true; // Controle de som ativado/desativado

        // Para reprodução de áudio com NAudio
        private WaveOutEvent waveOut;
        private AudioFileReader audioFileReader;

        // Caminho base para os arquivos de som
        private readonly string notificationPath = @"E:\ESTUDOS VISUAL COLD\APPS E SITES DESENVOLVIDOS\ESTUDO CRONÔMETRO DOTNET APP WINDOUS\CRONOMETRO APP\CronometroRegressivo\notification";

        public TimerPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            
            // Desativa sons do sistema para este controle
            SetStyle(ControlStyles.EnableNotifyMessage, false);
            
            InitializeComponent();
            LoadBackgroundImage();
            LoadIcons();
            InitializeTimer();
            SetupEvents();
            
            VerifySoundFiles();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 400);
            this.BackColor = Color.FromArgb(20, 13, 39);
            this.DoubleBuffered = true;
            
            // Desativa sons de clique
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            
            // Initialize font with fallback
            try
            {
                timerFont = new Font("Rampart One", 48, FontStyle.Bold);
            }
            catch
            {
                timerFont = new Font("Arial", 48, FontStyle.Bold);
            }
        }

        // Override para prevenir sons do sistema
        protected override void WndProc(ref Message m)
        {
            // Filtra mensagens que podem causar sons do sistema
            switch (m.Msg)
            {
                case 0x0020: // WM_SETCURSOR
                case 0x0021: // WM_MOUSEACTIVATE
                case 0x0201: // WM_LBUTTONDOWN
                case 0x0202: // WM_LBUTTONUP
                case 0x0203: // WM_LBUTTONDBLCLK
                case 0x0204: // WM_RBUTTONDOWN
                case 0x0205: // WM_RBUTTONUP
                case 0x0206: // WM_RBUTTONDBLCLK
                    // Processa normalmente mas não produz som
                    base.WndProc(ref m);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void SetupEvents()
        {
            this.MouseClick += TimerPanel_MouseClick;
            this.MouseMove += (s, e) => { mousePosition = e.Location; Invalidate(); };
        }

        private void TimerPanel_MouseClick(object sender, MouseEventArgs e)
        {
            // Previne o som padrão de clique
            if (btnMenuBounds.Contains(e.Location))
            {
                MenuClicked?.Invoke(this, EventArgs.Empty);
            }
            else if (btnPlayBounds.Contains(e.Location))
            {
                StartTimer();
            }
            else if (btnPauseBounds.Contains(e.Location))
            {
                PauseTimer();
            }
            else if (btnResetBounds.Contains(e.Location))
            {
                ResetTimer();
            }
            else if (btnSoundBounds.Contains(e.Location))
            {
                ToggleSound();
            }
        }

        private void ToggleSound()
        {
            soundEnabled = !soundEnabled;
            SoundToggled?.Invoke(this, soundEnabled);
            Invalidate();
        }

        public bool IsSoundEnabled => soundEnabled;

        private void LoadBackgroundImage()
        {
            string path = Path.Combine(Application.StartupPath, "Resources", "mapa.png");
            if (File.Exists(path))
            {
                backgroundImage = Image.FromFile(path);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            if (backgroundImage != null)
            {
                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = 0.25f;
                
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                e.Graphics.DrawImage(
                    backgroundImage,
                    new Rectangle(0, 0, Width, Height),
                    0, 0, backgroundImage.Width, backgroundImage.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }
        }

        private Image ApplyOpacity(Image image, float opacity)
        {
            Bitmap bmp = new Bitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                ColorMatrix matrix = new ColorMatrix { Matrix33 = opacity };
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                g.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
            return bmp;
        }

        private void LoadIcons()
        {
            string basePath = Path.Combine(Application.StartupPath, "Resources");
            playIcon = LoadIcon(Path.Combine(basePath, "play.png"));
            pauseIcon = LoadIcon(Path.Combine(basePath, "pause.png"));
            resetIcon = LoadIcon(Path.Combine(basePath, "reset.png"));
            menuIcon = LoadIcon(Path.Combine(basePath, "menu.png"));
        }

        private Image LoadIcon(string path)
        {
            if (!File.Exists(path)) return new Bitmap(60, 60);
            Image icon = Image.FromFile(path);
            Bitmap bmp = new Bitmap(60, 60);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawImage(icon, new Rectangle(0, 0, 60, 60));
            }
            return bmp;
        }

        private void InitializeTimer()
        {
            timer.Interval = 1000;
            timer.Tick += (s, e) => TimerTick();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            string timeText = currentTime.ToString(@"hh\:mm\:ss");
            SizeF textSize = g.MeasureString(timeText, timerFont);
            Point textLocation = new Point((Width - (int)textSize.Width) / 2, (Height - (int)textSize.Height) / 2 - 30);
            g.DrawString(timeText, timerFont, Brushes.White, textLocation);

            int yButtons = textLocation.Y + (int)textSize.Height + 80;
            btnPlayBounds = new Rectangle(textLocation.X + 30, yButtons, 30, 30);
            btnPauseBounds = new Rectangle(textLocation.X + 120, yButtons, 30, 30);
            btnResetBounds = new Rectangle(textLocation.X + 210, yButtons, 30, 30);
            btnMenuBounds = new Rectangle(20, 50, 30, 30);
            btnSoundBounds = new Rectangle(Width - 50, 50, 30, 30);

            DrawIconWhite(g, menuIcon, btnMenuBounds);
            DrawIconWhite(g, playIcon, btnPlayBounds);
            DrawIconWhite(g, pauseIcon, btnPauseBounds);
            DrawIconWhite(g, resetIcon, btnResetBounds);
            DrawSoundIcon(g, btnSoundBounds, soundEnabled);
        }

        private void DrawIconWhite(Graphics g, Image icon, Rectangle bounds)
        {
            if (icon == null) return;

            ColorMatrix colorMatrix = new ColorMatrix(new float[][]
            {
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {1, 1, 1, 0, 1}
            });

            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);

            g.DrawImage(icon, bounds, 0, 0, icon.Width, icon.Height, GraphicsUnit.Pixel, attributes);
        }

        private void DrawSoundIcon(Graphics g, Rectangle bounds, bool enabled)
        {
            using (var font = new Font("Segoe UI", 14))
            using (var brush = new SolidBrush(enabled ? Color.White : Color.Gray))
            {
                string iconText = enabled ? "🔔" : "🔕";
                g.DrawString(iconText, font, brush, bounds);
            }
        }

        public void SetTimer(string name, TimeSpan time)
        {
            currentTimerModel = new TimerModel { Name = name, TimeInSeconds = (int)time.TotalSeconds };
            LoadTimer(currentTimerModel);
        }

        public void LoadTimer(TimerModel timerModel)
        {
            if (timerModel == null) return;
            currentTimerModel = timerModel;
            currentTime = TimeSpan.FromSeconds(timerModel.TimeInSeconds);
            isCountdownMode = true;
            countdownSoundPlayed = false;
            soundEnabled = timerModel.HasSoundNotification; // Sincroniza com as configurações do timer
            UpdateDisplay();
            PauseTimer();
        }

        private void TimerTick()
{
    if (!isRunning) return;
    
    if (isCountdownMode && currentTimerModel != null)
    {
        if (currentTime > TimeSpan.Zero)
        {
            currentTime = currentTime.Add(TimeSpan.FromSeconds(-1));
            UpdateDisplay();
            
            if (currentTime.TotalSeconds == 5 && !countdownSoundPlayed && soundEnabled)
            {
                PlayCountdownSound();
                countdownSoundPlayed = true;
            }
        }
        
        if (currentTime == TimeSpan.Zero)
        {
            // Para o timer primeiro
            PauseTimer();
            
            // Toca som personalizado se configurado e som habilitado
            if (currentTimerModel.HasSoundNotification && soundEnabled)
            {
                PlayTimerEndSound();
            }
            
            // Dispara eventos sem som do sistema
            InvokeEventsSilently();
            
            // CORREÇÃO: Reiniciar automaticamente se o loop estiver ativado
            if (currentTimerModel.ShouldRepeat)
            {
                currentTime = TimeSpan.FromSeconds(currentTimerModel.TimeInSeconds);
                countdownSoundPlayed = false;
                UpdateDisplay();
                
                if (soundEnabled) PlayRestartSound();
                
                // REINICIAR O TIMER AUTOMATICAMENTE - Esta é a correção principal
                StartTimer();
            }
        }
    }
    else
    {
        currentTime = currentTime.Add(TimeSpan.FromSeconds(1));
        UpdateDisplay();
    }
}

        // Método para invocar eventos sem causar sons do sistema
        private void InvokeEventsSilently()
        {
            // Usa BeginInvoke para evitar bloqueio e sons do sistema
            BeginInvoke(new Action(() =>
            {
                TimerCompleted?.Invoke(this, EventArgs.Empty);
                OnTimerCompleted?.Invoke(this, EventArgs.Empty);
            }));
        }

        private void UpdateDisplay()
        {
            Invalidate();
        }

        // Método para reproduzir arquivos MP3 usando NAudio
        private void PlayMp3Sound(string fileName)
        {
            if (!soundEnabled) return; // Não reproduz som se estiver desativado
            
            try
            {
                string soundPath = Path.Combine(notificationPath, fileName);
                if (File.Exists(soundPath))
                {
                    StopMp3Sound();
                    
                    audioFileReader = new AudioFileReader(soundPath);
                    waveOut = new WaveOutEvent();
                    waveOut.Volume = soundVolume;
                    waveOut.Init(audioFileReader);
                    waveOut.Play();
                    
                    waveOut.PlaybackStopped += (s, e) => 
                    {
                        waveOut?.Dispose();
                        audioFileReader?.Dispose();
                        waveOut = null;
                        audioFileReader = null;
                    };
                }
                else
                {
                    Console.WriteLine($"Arquivo de som não encontrado: {fileName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao reproduzir som {fileName}: {ex.Message}");
            }
        }

        // Para parar o som MP3 atual
        private void StopMp3Sound()
        {
            try
            {
                waveOut?.Stop();
                waveOut?.Dispose();
                audioFileReader?.Dispose();
                waveOut = null;
                audioFileReader = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao parar som: {ex.Message}");
            }
        }

        private void PlayStartSound()
        {
            PlayMp3Sound("futuristic-noises-236386.mp3");
        }

        private void PlayTimerEndSound()
        {
            PlayMp3Sound("timer-end-sound.mp3");
        }

        private void PlayCountdownSound()
        {
            PlayMp3Sound("nada-dering-bom-waktu-367220.mp3");
        }

        private void PlayRestartSound()
        {
            PlayMp3Sound("restart-sound.mp3");
        }

        public float SoundVolume
        {
            get => soundVolume;
            set
            {
                soundVolume = Math.Max(0, Math.Min(1, value));
                if (waveOut != null)
                {
                    waveOut.Volume = soundVolume;
                }
            }
        }

        public void StartTimer()
        {
            if (!isRunning)
            {
                StopMp3Sound();
                
                if (currentTime == TimeSpan.Zero && currentTimerModel != null)
                {
                    isCountdownMode = false;
                }
                
                countdownSoundPlayed = false;
                if (soundEnabled) PlayStartSound();
                timer.Start();
                isRunning = true;
            }
        }

        private void PauseTimer()
        {
            if (isRunning)
            {
                timer.Stop();
                isRunning = false;
                StopMp3Sound();
            }
        }

        private void ResetTimer()
        {
            PauseTimer();
            currentTime = TimeSpan.Zero;
            isCountdownMode = false;
            countdownSoundPlayed = false;
            UpdateDisplay();
        }

        public void VerifySoundFiles()
        {
            string[] files = {
                "futuristic-noises-236386.mp3",
                "nada-dering-bom-waktu-367220.mp3",
                "timer-end-sound.mp3",
                "restart-sound.mp3"
            };
            
            Console.WriteLine("=== VERIFICAÇÃO DE ARQUIVOS DE SOM ===");
            Console.WriteLine($"Pasta de notificação: {notificationPath}");
            Console.WriteLine($"Pasta existe: {Directory.Exists(notificationPath)}");
            
            if (Directory.Exists(notificationPath))
            {
                foreach (var file in files)
                {
                    string path = Path.Combine(notificationPath, file);
                    bool exists = File.Exists(path);
                    Console.WriteLine($"{file} -> {(exists ? "✓ ENCONTRADO" : "✗ NÃO ENCONTRADO")}");
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopMp3Sound();
                timer?.Dispose();
                playIcon?.Dispose();
                pauseIcon?.Dispose();
                resetIcon?.Dispose();
                menuIcon?.Dispose();
                backgroundImage?.Dispose();
                timerFont?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}