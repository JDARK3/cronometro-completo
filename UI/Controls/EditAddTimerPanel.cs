using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CronometroRegressivo.Core.Managers;
using CronometroRegressivo.Core.Models;

namespace CronometroRegressivo.UI.Controls
{
    public class RoundedButton : Button
    {
        public int CornerRadius { get; set; } = 15;

        public RoundedButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.BackColor = Color.FromArgb(0, 120, 215);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.Size = new Size(90, 35);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            
            using (GraphicsPath path = GetRoundedRectPath(ClientRectangle, CornerRadius))
            using (SolidBrush brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillPath(brush, path);
                
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                
                e.Graphics.DrawString(Text, Font, new SolidBrush(ForeColor), 
                    ClientRectangle, sf);
            }
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            
            this.Region = new Region(path);
            return path;
        }
    }

    public class EditAddTimerPanel : UserControl
    {
        public event EventHandler? BackClicked;
        public event EventHandler<TimerModel>? TimerSaved;

        private TimerModel? _timer;
        private readonly TextBox txtName;
        private readonly CustomNumericUpDown numHours;
        private readonly CustomNumericUpDown numMinutes;
        private readonly CustomNumericUpDown numSeconds;
        private readonly RoundedButton btnSave;
        private readonly RoundedButton btnCancel;
        private Image? _backgroundImage;
        private Panel timeContainer;

        public class CustomNumericUpDown : UserControl
        {
            private int _value = 0;
            private int _min = 0;
            private int _max = 99;

            private Label lblValue;
            private PictureBox btnUp;
            private PictureBox btnDown;

            public int Value
            {
                get => _value;
                set
                {
                    if (value < _min) value = _min;
                    if (value > _max) value = _max;
                    _value = value;
                    UpdateLabel();
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            public event EventHandler? ValueChanged;

            public CustomNumericUpDown()
            {
                this.Size = new Size(60, 100);
                this.BackColor = Color.Transparent;

                btnUp = new PictureBox
                {
                    Size = new Size(30, 25),
                    Location = new Point(15, 0),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = LoadWhiteArrow("arrow-up.png"),
                    Cursor = Cursors.Hand,
                    BackColor = Color.Transparent
                };
                btnUp.Click += (s, e) => 
                {
                    Value++;
                    this.Focus(); // Manter o foco no controle pai após clicar na seta
                };

                lblValue = new Label
                {
                    Size = new Size(60, 50),
                    Location = new Point(0, 25),
                    Font = new Font("Segoe UI", 24, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Text = "00",
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    BorderStyle = BorderStyle.None
                };

                btnDown = new PictureBox
                {
                    Size = new Size(30, 25),
                    Location = new Point(15, 75),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = LoadWhiteArrow("arrow-down.png"),
                    Cursor = Cursors.Hand,
                    BackColor = Color.Transparent
                };
                btnDown.Click += (s, e) => 
                {
                    Value--;
                    this.Focus(); // Manter o foco no controle pai após clicar na seta
                };

                this.Controls.Add(btnUp);
                this.Controls.Add(lblValue);
                this.Controls.Add(btnDown);

                // Configurar para receber eventos de teclado
                this.SetStyle(ControlStyles.Selectable, true);
                this.TabStop = true;
                
                // Adicionar event handlers para teclado
                this.KeyDown += CustomNumericUpDown_KeyDown;
                this.PreviewKeyDown += CustomNumericUpDown_PreviewKeyDown;
                
                // Configurar os eventos de clique nas setas para focar o controle pai
                btnUp.MouseDown += (s, e) => this.Focus();
                btnDown.MouseDown += (s, e) => this.Focus();
                
                // Remover a borda de foco padrão
                this.SetStyle(ControlStyles.UserPaint, true);
                this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                this.SetStyle(ControlStyles.DoubleBuffer, true);
                this.SetStyle(ControlStyles.ResizeRedraw, true);
                this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            }

            private void CustomNumericUpDown_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
            {
                // Permitir que as teclas de seta sejam processadas
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                {
                    e.IsInputKey = true;
                }
            }

            private void CustomNumericUpDown_KeyDown(object sender, KeyEventArgs e)
            {
                HandleKeyDown(e);
            }

            // Método público para manipular teclas
            public bool HandleKey(Keys keyData)
            {
                switch (keyData)
                {
                    case Keys.Up:
                        Value++;
                        return true;
                    case Keys.Down:
                        Value--;
                        return true;
                    default:
                        return false;
                }
            }

            // Método para manipular KeyEventArgs
            public void HandleKeyDown(KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                {
                    HandleKey(e.KeyCode);
                    e.Handled = true;
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                
                // REMOVIDO: Código que desenhava a borda de foco
                // Não queremos mostrar nenhuma borda quando o controle está com foco
            }

            protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
            {
                // Capturar teclas de seta mesmo quando o controle não está com foco
                if (keyData == Keys.Up || keyData == Keys.Down)
                {
                    HandleKey(keyData);
                    return true;
                }
                return base.ProcessCmdKey(ref msg, keyData);
            }

            private void UpdateLabel()
            {
                lblValue.Text = _value.ToString("D2");
            }

            private Image LoadWhiteArrow(string fileName)
            {
                try
                {
                    string path = Path.Combine(Application.StartupPath, "Resources", fileName);
                    if (File.Exists(path))
                    {
                        using (var original = Image.FromFile(path))
                        {
                            var whiteArrow = new Bitmap(original.Width, original.Height);

                            using (Graphics g = Graphics.FromImage(whiteArrow))
                            using (ImageAttributes attributes = new ImageAttributes())
                            {
                                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                                {
                                    new float[] {0, 0, 0, 0, 0},
                                    new float[] {0, 0, 0, 0, 0},
                                    new float[] {0, 0, 0, 0, 0},
                                    new float[] {0, 0, 0, 1, 0},
                                    new float[] {1, 1, 1, 0, 1}
                                });

                                attributes.SetColorMatrix(colorMatrix);

                                g.DrawImage(
                                    original,
                                    new Rectangle(0, 0, whiteArrow.Width, whiteArrow.Height),
                                    0, 0, original.Width, original.Height,
                                    GraphicsUnit.Pixel,
                                    attributes);
                            }
                            return whiteArrow;
                        }
                    }
                }
                catch { }

                var bmp = new Bitmap(24, 24);
                using (Graphics g = Graphics.FromImage(bmp))
                using (Font font = new Font("Segoe UI", 12))
                using (Brush brush = new SolidBrush(Color.White))
                {
                    g.Clear(Color.Transparent);
                    string arrow = fileName.Contains("up") ? "↑" : "↓";
                    g.DrawString(arrow, font, brush, new PointF(5, 5));
                }
                return bmp;
            }
            
            // Remover a borda de foco padrão do Windows
            protected override void OnPaintBackground(PaintEventArgs e)
            {
                base.OnPaintBackground(e);
                
                // Não fazer nada - isso previne a borda de foco padrão
            }
        }

        public EditAddTimerPanel(TimerModel? timer = null)
        {
            _timer = timer;

            try
            {
                string path = Path.Combine(Application.StartupPath, "Resources", "mapa.png");
                if (File.Exists(path))
                {
                    _backgroundImage = Image.FromFile(path);
                }
            }
            catch { }

            txtName = new TextBox();
            numHours = new CustomNumericUpDown();
            numMinutes = new CustomNumericUpDown();
            numSeconds = new CustomNumericUpDown();
            btnSave = new RoundedButton();
            btnCancel = new RoundedButton();

            InitializeComponents();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            if (_backgroundImage != null)
            {
                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = 0.25f;
                
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                e.Graphics.DrawImage(
                    _backgroundImage,
                    new Rectangle(0, 0, Width, Height),
                    0, 0, _backgroundImage.Width, _backgroundImage.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }
        }

        private void InitializeComponents()
        {
            this.BackColor = Color.FromArgb(20, 13, 39);
            this.Size = new Size(600, 800);
            this.DoubleBuffered = true;

            int centerX = this.Width / 2;
            int currentY = 70;

            var lblTitle = new Label
            {
                Text = _timer == null ? "Adicionar novo temporizador" : "Editar temporizador",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(400, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(centerX - 200, currentY),
                BackColor = Color.Transparent
            };
            currentY += 50;

            timeContainer = new Panel
            {
                Size = new Size(180, 100),
                Location = new Point(centerX - 90, currentY),
                BackColor = Color.Transparent
            };

            numHours.Location = new Point(0, 0);
            numMinutes.Location = new Point(60, 0);
            numSeconds.Location = new Point(120, 0);

            if (_timer != null)
            {
                numHours.Value = _timer.TimeInSeconds / 3600;
                numMinutes.Value = (_timer.TimeInSeconds % 3600) / 60;
                numSeconds.Value = _timer.TimeInSeconds % 60;
            }

            timeContainer.Controls.AddRange(new Control[] { numHours, numMinutes, numSeconds });
            currentY += 110;

            var lblTimerLabel = new Label
            {
                Text = "Cronômetro",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(100, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(centerX - 50, currentY),
                BackColor = Color.Transparent
            };
            currentY += 30;

            txtName.Text = _timer?.Name ?? "";
            txtName.Size = new Size(300, 30);
            txtName.Font = new Font("Segoe UI", 12);
            txtName.BackColor = Color.FromArgb(40, 33, 59);
            txtName.ForeColor = Color.White;
            txtName.BorderStyle = BorderStyle.FixedSingle;
            txtName.Location = new Point(centerX - 150, currentY);
            txtName.PlaceholderText = "Nome do temporizador";
            txtName.TextAlign = HorizontalAlignment.Center;
            currentY += 80;

            btnSave.Text = "Salvar";
            btnSave.BackColor = Color.FromArgb(0, 120, 215);
            btnSave.Location = new Point(centerX - 120, currentY);
            btnSave.CornerRadius = 10;
            btnSave.Click += (s, e) => SaveTimer();
            btnSave.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 90, 180);
            btnSave.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 60, 120);

            btnCancel.Text = "Cancelar";
            btnCancel.BackColor = Color.FromArgb(100, 100, 100);
            btnCancel.Location = new Point(centerX + 30, currentY);
            btnCancel.CornerRadius = 10;
            btnCancel.Click += (s, e) => BackClicked?.Invoke(this, EventArgs.Empty);
            btnCancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 80, 80);
            btnCancel.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 60, 60);

            Controls.AddRange(new Control[]
            {
                lblTitle,
                timeContainer,
                lblTimerLabel,
                txtName,
                btnSave,
                btnCancel
            });

            // Configurar tab order e capacidade de receber foco
            numHours.TabStop = true;
            numMinutes.TabStop = true;
            numSeconds.TabStop = true;

            // Configurar a ordem de tabulação
            numHours.TabIndex = 1;
            numMinutes.TabIndex = 2;
            numSeconds.TabIndex = 3;
            txtName.TabIndex = 4;
            btnSave.TabIndex = 5;
            btnCancel.TabIndex = 6;

            // Adicionar evento KeyDown para o painel principal para capturar teclas
            this.KeyDown += EditAddTimerPanel_KeyDown;
            this.PreviewKeyDown += EditAddTimerPanel_PreviewKeyDown;
        }

        private void EditAddTimerPanel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            // Permitir que as teclas de seta sejam processadas pelo KeyDown
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || 
                e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                e.IsInputKey = true;
            }
        }

        private void EditAddTimerPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                // Encontrar o controle com foco
                Control focusedControl = this.ActiveControl;
                
                // Se o foco está em um CustomNumericUpDown, processar a tecla
                if (focusedControl is CustomNumericUpDown numericControl)
                {
                    numericControl.HandleKeyDown(e);
                    return;
                }
                
                // Se não, passar o foco para o primeiro CustomNumericUpDown e processar a tecla
                if (numHours.CanFocus)
                {
                    numHours.Focus();
                    numHours.HandleKey(e.KeyCode);
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                // Navegar entre os controles com as teclas esquerda/direita
                Control focusedControl = this.ActiveControl;
                
                if (focusedControl is CustomNumericUpDown)
                {
                    // Determinar qual controle está focado atualmente
                    int currentIndex = -1;
                    if (focusedControl == numHours) currentIndex = 0;
                    else if (focusedControl == numMinutes) currentIndex = 1;
                    else if (focusedControl == numSeconds) currentIndex = 2;
                    
                    // Calcular o próximo controle com base na tecla pressionada
                    int nextIndex = currentIndex;
                    if (e.KeyCode == Keys.Right) nextIndex = (currentIndex + 1) % 3;
                    else if (e.KeyCode == Keys.Left) nextIndex = (currentIndex + 2) % 3;
                    
                    // Mover o foco para o próximo controle
                    CustomNumericUpDown nextControl = null;
                    if (nextIndex == 0) nextControl = numHours;
                    else if (nextIndex == 1) nextControl = numMinutes;
                    else if (nextIndex == 2) nextControl = numSeconds;
                    
                    if (nextControl != null && nextControl.CanFocus)
                    {
                        nextControl.Focus();
                        e.Handled = true;
                    }
                }
                else
                {
                    // Se o foco não está em um controle numérico, focar no primeiro
                    if (numHours.CanFocus)
                    {
                        numHours.Focus();
                        e.Handled = true;
                    }
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Se alguma seta for pressionada, processar a tecla
            if (keyData == Keys.Up || keyData == Keys.Down || 
                keyData == Keys.Left || keyData == Keys.Right)
            {
                // Verificar se o foco está em um dos controles numéricos
                Control focusedControl = this.ActiveControl;
                
                if (focusedControl is CustomNumericUpDown numericControl)
                {
                    // Processar teclas para cima/baixo
                    if (keyData == Keys.Up || keyData == Keys.Down)
                    {
                        return numericControl.HandleKey(keyData);
                    }
                    // Processar teclas para esquerda/direita (navegação)
                    else if (keyData == Keys.Left || keyData == Keys.Right)
                    {
                        // Esta lógica já é tratada no KeyDown, então retornar true
                        return true;
                    }
                }
                else
                {
                    // Se o foco não está em um controle numérico, focar no primeiro e processar a tecla
                    if (numHours.CanFocus && (keyData == Keys.Up || keyData == Keys.Down))
                    {
                        numHours.Focus();
                        return numHours.HandleKey(keyData);
                    }
                }
            }
            
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void SaveTimer()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Por favor, insira um nome para o temporizador.");
                return;
            }

            int totalSeconds = numHours.Value * 3600 + numMinutes.Value * 60 + numSeconds.Value;

            if (totalSeconds <= 0)
            {
                MessageBox.Show("O tempo deve ser maior que zero.");
                return;
            }

            if (_timer == null && TimerManager.TimerNameExists(txtName.Text))
            {
                MessageBox.Show("Já existe um temporizador com esse nome. Escolha outro nome.");
                return;
            }

            if (_timer == null)
                _timer = new TimerModel();

            _timer.Name = txtName.Text;
            _timer.TimeInSeconds = totalSeconds;
            _timer.InitialSeconds = totalSeconds;  // ATUALIZAR InitialSeconds também!
            
            // Mantém as configurações originais de notificação e repetição
            // Se for um novo timer, usa valores padrão
            if (_timer.Id == 0)
            {
                _timer.HasSoundNotification = true;
                _timer.ShouldRepeat = false;
                _timer.InitialSeconds = totalSeconds;
            }

            TimerManager.SaveTimer(_timer);
            TimerSaved?.Invoke(this, _timer);
            BackClicked?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _backgroundImage?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}