using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using CronometroRegressivo.Core.Managers;
using CronometroRegressivo.Core.Models;
using System.Linq;
using System.Media;

namespace CronometroRegressivo.UI.Controls
{
    public class TimerListPanel : UserControl
    {
        // ============ CONSTANTES DE POSICIONAMENTO ============
        private const int TITLE_TOP = 50;
        private const int ITEMS_START_TOP = 100;
        private const int ITEM_SPACING = 10;
        private const int ITEM_WIDTH = 450;
        private const int ITEM_HEIGHT = 20;
        private const int BUTTON_SIZE = 25;
        private const int BUTTON_MARGIN = 35;
        private const int ICON_SIZE = 18;
        
        private const int TOP_MARGIN = 70;
        private const int BOTTOM_MARGIN = 30;

        private List<TimerModel> timers = new List<TimerModel>();
        private Rectangle btnBackBounds, btnAddBounds, btnLoopBounds;
        private Point mousePosition;
        private Image backgroundImage;
        private System.Windows.Forms.Timer sequenceTimer;
        private List<TimerModel> sequenceTimers;
        private int currentSequenceIndex;
        private bool isSequenceRunning;
        private int scrollOffset = 0;
        private int maxScrollOffset = 0;
        private bool isScrolling = false;
        private Point scrollStartPoint;
        private int scrollStartOffset;

        // Áreas clicáveis dos itens
        private List<Rectangle> itemBounds = new List<Rectangle>();
        private List<Rectangle> itemEditBounds = new List<Rectangle>();
        private List<Rectangle> itemLoopBounds = new List<Rectangle>();
        private List<Rectangle> itemNotificationBounds = new List<Rectangle>();

        public event EventHandler BackClicked;
        public event EventHandler AddClicked;
        public event EventHandler<TimerModel> EditTimerClicked;
        public event EventHandler<TimerModel> TimerSelected;
        public event EventHandler<TimerModel> LoopToggled;
        public event EventHandler<TimerModel> NotificationToggled;
        public event EventHandler SequenceStarted;
        public event EventHandler SequenceFinished;

        // Novo evento para solicitar início automático do timer
        public event EventHandler StartTimerRequested;

        public TimerListPanel()
        {
            InitializeComponent();
            LoadBackgroundImage();
            InitializeSequenceTimer();
            
            this.Resize += (s, e) => CalculateLayout();
            this.MouseMove += (s, e) => { mousePosition = e.Location; Invalidate(); };
            
            // Eventos para rolagem personalizada
            this.MouseWheel += TimerListPanel_MouseWheel;
            this.MouseDown += TimerListPanel_MouseDown;
            this.MouseMove += TimerListPanel_MouseMove;
            this.MouseUp += TimerListPanel_MouseUp;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(20, 13, 39);
            this.Dock = DockStyle.Fill;
            
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                         ControlStyles.AllPaintingInWmPaint | 
                         ControlStyles.UserPaint | 
                         ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();
            
            this.ResumeLayout(false);
        }

        private void TimerListPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!IsOverSideButtons(e.Location))
            {
                int newScrollOffset = scrollOffset - (e.Delta / 3);
                newScrollOffset = Math.Max(0, Math.Min(maxScrollOffset, newScrollOffset));
                
                if (scrollOffset != newScrollOffset)
                {
                    scrollOffset = newScrollOffset;
                    Invalidate();
                }
            }
        }

        private void TimerListPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ProcessItemClick(e.Location);
            }
            
            if (e.Button == MouseButtons.Left && !IsOverSideButtons(e.Location))
            {
                isScrolling = true;
                scrollStartPoint = e.Location;
                scrollStartOffset = scrollOffset;
                this.Cursor = Cursors.Hand;
            }
        }

        private void TimerListPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isScrolling)
            {
                if (Math.Abs(scrollStartPoint.Y - e.Location.Y) < 3)
                    return;
                    
                int deltaY = scrollStartPoint.Y - e.Location.Y;
                int newScrollOffset = scrollStartOffset + deltaY;
                
                newScrollOffset = Math.Max(0, Math.Min(maxScrollOffset, newScrollOffset));
                
                if (scrollOffset != newScrollOffset)
                {
                    scrollOffset = newScrollOffset;
                    Invalidate();
                }
            }
            
            UpdateCursor(e.Location);
        }

        private void TimerListPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isScrolling = false;
                UpdateCursor(e.Location);
            }
        }

        private void ProcessItemClick(Point location)
        {
            if (isScrolling) return;
            
            if (btnBackBounds.Contains(location))
            {
                BackClicked?.Invoke(this, EventArgs.Empty);
                return;
            }
            else if (btnAddBounds.Contains(location))
            {
                AddClicked?.Invoke(this, EventArgs.Empty);
                return;
            }
            else if (btnLoopBounds.Contains(location))
            {
                ToggleSequence();
                return;
            }

            for (int i = 0; i < itemBounds.Count; i++)
            {
                if (itemBounds[i].Contains(location))
                {
                    if (itemEditBounds[i].Contains(location))
                    {
                        EditTimerClicked?.Invoke(this, timers[i]);
                    }
                    else if (itemLoopBounds[i].Contains(location))
                    {
                        ToggleItemLoop(i);
                    }
                    else if (itemNotificationBounds[i].Contains(location))
                    {
                        ToggleItemNotification(i);
                    }
                    else
                    {
                        TimerSelected?.Invoke(this, timers[i]);
                    }
                    return;
                }
            }
        }

        private void ToggleItemLoop(int index)
        {
            timers[index].ShouldRepeat = !timers[index].ShouldRepeat;
            TimerManager.SaveTimer(timers[index]);
            LoopToggled?.Invoke(this, timers[index]);
            Invalidate();
        }

        private void ToggleItemNotification(int index)
        {
            timers[index].HasSoundNotification = !timers[index].HasSoundNotification;
            TimerManager.SaveTimer(timers[index]);
            NotificationToggled?.Invoke(this, timers[index]);
            Invalidate();
        }

        private bool IsOverSideButtons(Point location)
        {
            return btnBackBounds.Contains(location) || 
                   btnAddBounds.Contains(location) || 
                   btnLoopBounds.Contains(location);
        }

        private void UpdateCursor(Point location)
        {
            if (isScrolling)
            {
                this.Cursor = Cursors.Hand;
            }
            else if (IsOverSideButtons(location) || IsOverAnyItemButton(location))
            {
                this.Cursor = Cursors.Hand;
            }
            else
            {
                this.Cursor = Cursors.Default;
            }
        }

        private bool IsOverAnyItemButton(Point location)
        {
            foreach (var bounds in itemEditBounds) if (bounds.Contains(location)) return true;
            foreach (var bounds in itemLoopBounds) if (bounds.Contains(location)) return true;
            foreach (var bounds in itemNotificationBounds) if (bounds.Contains(location)) return true;
            return false;
        }

        private void InitializeSequenceTimer()
        {
            sequenceTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            sequenceTimer.Tick += ProcessSequence;
        }

        private void LoadBackgroundImage()
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, "Resources", "mapa.png");
                if (File.Exists(path))
                {
                    backgroundImage = Image.FromFile(path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar imagem de fundo: {ex.Message}");
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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (var font = new Font("Segoe UI", 18, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                var textSize = g.MeasureString("Selecionar Temporizador", font);
                g.DrawString("Selecionar Temporizador", font, brush, 
                    (Width - textSize.Width) / 2, TITLE_TOP);
            }

            int yPos = 50;
            btnBackBounds = new Rectangle(20, yPos, BUTTON_SIZE, BUTTON_SIZE);
            btnAddBounds = new Rectangle(20, yPos + BUTTON_MARGIN, BUTTON_SIZE, BUTTON_SIZE);
            btnLoopBounds = new Rectangle(20, yPos + BUTTON_MARGIN * 2, BUTTON_SIZE, BUTTON_SIZE);

            DrawIcon(g, "voltar.png", btnBackBounds);
            DrawIcon(g, "add.png", btnAddBounds);
            DrawLoopIcon(g, btnLoopBounds, isSequenceRunning);

            DrawVirtualItems(g);
        }

        private void DrawVirtualItems(Graphics g)
        {
            itemBounds.Clear();
            itemEditBounds.Clear();
            itemLoopBounds.Clear();
            itemNotificationBounds.Clear();

            int centerX = (Width - ITEM_WIDTH) / 2;
            int yPos = ITEMS_START_TOP - scrollOffset;

            int visibleTop = TITLE_TOP + TOP_MARGIN;
            int visibleBottom = Height - BOTTOM_MARGIN;

            for (int i = 0; i < timers.Count; i++)
            {
                var timer = timers[i];
                
                bool isCompletelyAbove = (yPos + ITEM_HEIGHT) < visibleTop;
                bool isCompletelyBelow = yPos > visibleBottom;
                bool isPartiallyBelow = (yPos + ITEM_HEIGHT) > visibleBottom;
                
                if (isCompletelyAbove || isCompletelyBelow || isPartiallyBelow)
                {
                    yPos += ITEM_HEIGHT + ITEM_SPACING;
                    continue;
                }

                var itemRect = new Rectangle(centerX, yPos, ITEM_WIDTH, ITEM_HEIGHT);
                itemBounds.Add(itemRect);

                using (var path = GetRoundedRectPath(itemRect, 15))
                using (var brush = new SolidBrush(Color.FromArgb(50, 255, 255, 255)))
                {
                    g.FillPath(brush, path);
                }

                using (var sf = new StringFormat())
                using (var nameFont = new Font("Segoe UI", 12))
                using (var timeFont = new Font("Segoe UI", 12))
                using (var textBrush = new SolidBrush(Color.White))
                using (var timeBrush = new SolidBrush(Color.LightGray))
                {
                    sf.Alignment = StringAlignment.Near;
                    sf.LineAlignment = StringAlignment.Center;

                    int textLeftMargin = 20;
                    int timeRightMargin = 150;
                    int timeWidth = 80;
                    int spacingBetweenTextAndTime = 5;
                    
                    int textAreaWidth = ITEM_WIDTH - timeRightMargin - timeWidth - spacingBetweenTextAndTime;
                    RectangleF textRect = new RectangleF(
                        centerX + textLeftMargin, 
                        yPos, 
                        textAreaWidth,
                        ITEM_HEIGHT
                    );

                    g.DrawString(timer.Name, nameFont, textBrush, textRect, sf);

                    sf.Alignment = StringAlignment.Far;
                    RectangleF timeRect = new RectangleF(
                        centerX + textLeftMargin + textAreaWidth + spacingBetweenTextAndTime,
                        yPos, 
                        timeWidth, 
                        ITEM_HEIGHT
                    );
                    
                    g.DrawString(
                        TimeSpan.FromSeconds(timer.TimeInSeconds).ToString(@"hh\:mm\:ss"),
                        timeFont,
                        timeBrush,
                        timeRect,
                        sf
                    );
                }

                int btnY = yPos + (ITEM_HEIGHT - ICON_SIZE) / 2;
                int iconSpacing = 20;
                int rightMargin = 20;
                
                var notifBounds = new Rectangle(
                    centerX + ITEM_WIDTH - rightMargin - ICON_SIZE, 
                    btnY,
                    ICON_SIZE, 
                    ICON_SIZE
                );
                itemNotificationBounds.Add(notifBounds);
                DrawNotificationIcon(g, notifBounds, timer.HasSoundNotification);
                
                var loopBounds = new Rectangle(
                    centerX + ITEM_WIDTH - rightMargin - ICON_SIZE - iconSpacing, 
                    btnY, 
                    ICON_SIZE, 
                    ICON_SIZE
                );
                itemLoopBounds.Add(loopBounds);
                DrawLoopIcon(g, loopBounds, timer.ShouldRepeat);
                
                var editBounds = new Rectangle(
                    centerX + ITEM_WIDTH - rightMargin - ICON_SIZE - iconSpacing * 2, 
                    btnY, 
                    ICON_SIZE, 
                    ICON_SIZE
                );
                itemEditBounds.Add(editBounds);
                DrawIcon(g, "edit.png", editBounds);

                yPos += ITEM_HEIGHT + ITEM_SPACING;
            }
        }

        private void DrawIcon(Graphics g, string iconName, Rectangle bounds)
        {
            Rectangle fixedBounds = new Rectangle(bounds.X, bounds.Y, ICON_SIZE, ICON_SIZE);
            
            try
            {
                string path = Path.Combine(Application.StartupPath, "Resources", iconName);
                if (File.Exists(path))
                {
                    using (var icon = Image.FromFile(path))
                    {
                        using (var resizedIcon = new Bitmap(ICON_SIZE, ICON_SIZE))
                        using (var resizedGraphics = Graphics.FromImage(resizedIcon))
                        {
                            resizedGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            resizedGraphics.DrawImage(icon, 0, 0, ICON_SIZE, ICON_SIZE);
                            
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

                            g.DrawImage(
                                resizedIcon, 
                                fixedBounds, 
                                0, 0, ICON_SIZE, ICON_SIZE,
                                GraphicsUnit.Pixel, 
                                attributes
                            );
                        }
                        return;
                    }
                }
            }
            catch { }

            using (var font = new Font("Segoe UI", 10))
            using (var brush = new SolidBrush(Color.White))
            {
                string text = iconName switch
                {
                    string s when s.Contains("edit") => "✏",
                    string s when s.Contains("voltar") => "←",
                    string s when s.Contains("add") => "+",
                    string s when s.Contains("loop") => "⟲",
                    _ => "?"
                };
                g.DrawString(text, font, brush, fixedBounds);
            }
        }

        private void DrawLoopIcon(Graphics g, Rectangle bounds, bool isActive)
        {
            Rectangle fixedBounds = new Rectangle(bounds.X, bounds.Y, ICON_SIZE, ICON_SIZE);
            Color iconColor = isActive ? Color.FromArgb(0, 200, 0) : Color.White;

            try
            {
                string path = Path.Combine(Application.StartupPath, "Resources", "loop.png");
                if (File.Exists(path))
                {
                    using (var icon = Image.FromFile(path))
                    {
                        using (var resizedIcon = new Bitmap(ICON_SIZE, ICON_SIZE))
                        using (var resizedGraphics = Graphics.FromImage(resizedIcon))
                        {
                            resizedGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            resizedGraphics.DrawImage(icon, 0, 0, ICON_SIZE, ICON_SIZE);
                            
                            float[][] colorMatrixElements = {
                                new float[] {0, 0, 0, 0, 0},
                                new float[] {0, 0, 0, 0, 0},
                                new float[] {0, 0, 0, 0, 0},
                                new float[] {0, 0, 0, 1, 0},
                                new float[] {
                                    iconColor.R / 255f, 
                                    iconColor.G / 255f, 
                                    iconColor.B / 255f, 
                                    0, 1
                                }
                            };

                            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
                            ImageAttributes attributes = new ImageAttributes();
                            attributes.SetColorMatrix(colorMatrix);

                            g.DrawImage(
                                resizedIcon, 
                                fixedBounds, 
                                0, 0, ICON_SIZE, ICON_SIZE,
                                GraphicsUnit.Pixel, 
                                attributes
                            );
                        }
                        return;
                    }
                }
            }
            catch { }

            using (var brush = new SolidBrush(iconColor))
            {
                g.DrawString("↻", new Font("Segoe UI", 12), brush, fixedBounds);
            }
        }

        private void DrawNotificationIcon(Graphics g, Rectangle bounds, bool isActive)
        {
            Rectangle fixedBounds = new Rectangle(bounds.X, bounds.Y, ICON_SIZE, ICON_SIZE);
            
            try
            {
                string iconName = isActive ? "notifications-active.png" : "notifications-off.png";
                string path = Path.Combine(Application.StartupPath, "Resources", iconName);
                
                if (File.Exists(path))
                {
                    using (var icon = Image.FromFile(path))
                    {
                        using (var resizedIcon = new Bitmap(ICON_SIZE, ICON_SIZE))
                        using (var resizedGraphics = Graphics.FromImage(resizedIcon))
                        {
                            resizedGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            resizedGraphics.DrawImage(icon, 0, 0, ICON_SIZE, ICON_SIZE);
                            
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

                            g.DrawImage(
                                resizedIcon, 
                                fixedBounds, 
                                0, 0, ICON_SIZE, ICON_SIZE,
                                GraphicsUnit.Pixel, 
                                attributes
                            );
                        }
                        return;
                    }
                }
            }
            catch { }

            using (var font = new Font("Segoe UI", isActive ? 12 : 10))
            using (var brush = new SolidBrush(Color.White))
            {
                string text = isActive ? "🔔" : "🔕";
                g.DrawString(text, font, brush, fixedBounds);
            }
        }

        private GraphicsPath GetRoundedRectPath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
            path.AddArc(bounds.Right - radius, bounds.Y, radius, radius, 270, 90);
            path.AddArc(bounds.Right - radius, bounds.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void ToggleSequence()
        {
            if (isSequenceRunning)
            {
                StopSequence();
            }
            else
            {
                StartSequence();
            }
        }

        private void StartSequence()
        {
            sequenceTimers = new List<TimerModel>(timers);
            if (sequenceTimers.Count == 0)
            {
                MessageBox.Show("Nenhum temporizador disponível para a sequência.");
                return;
            }

            isSequenceRunning = true;
            currentSequenceIndex = 0;
            SequenceStarted?.Invoke(this, EventArgs.Empty);
            
            // Toca som de início da sequência
            PlaySequenceStartSound();
            
            StartNextTimerInSequence();
            
            Invalidate();
        }

        private void StartNextTimerInSequence()
        {
            if (currentSequenceIndex < sequenceTimers.Count)
            {
                var timer = sequenceTimers[currentSequenceIndex];
                TimerSelected?.Invoke(this, timer);
                
                // Toca som de início para o próximo timer na sequência
                PlayNextTimerSound();
                
                StartTimerRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                currentSequenceIndex = 0;
                if (sequenceTimers.Count > 0)
                {
                    var timer = sequenceTimers[currentSequenceIndex];
                    TimerSelected?.Invoke(this, timer);
                    
                    // Toca som de início para o primeiro timer da sequência
                    PlayNextTimerSound();
                    
                    StartTimerRequested?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void PlaySequenceStartSound()
        {
            try
            {
                string soundPath = Path.Combine(Application.StartupPath, "Resources", "sequence-start.wav");
                if (File.Exists(soundPath))
                {
                    using (var soundPlayer = new SoundPlayer(soundPath))
                    {
                        soundPlayer.Play();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao reproduzir som de início de sequência: {ex.Message}");
            }
        }

        private void PlayNextTimerSound()
        {
            try
            {
                string soundPath = Path.Combine(Application.StartupPath, "Resources", "next-timer.wav");
                if (File.Exists(soundPath))
                {
                    using (var soundPlayer = new SoundPlayer(soundPath))
                    {
                        soundPlayer.Play();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao reproduzir som de próximo timer: {ex.Message}");
            }
        }

        public void NotifyTimerCompleted()
        {
            if (isSequenceRunning)
            {
                sequenceTimer.Stop();
                sequenceTimer.Interval = 2000;
                sequenceTimer.Tick -= ProcessSequence;
                sequenceTimer.Tick += MoveToNextTimer;
                sequenceTimer.Start();
            }
        }

        private void MoveToNextTimer(object sender, EventArgs e)
        {
            sequenceTimer.Stop();
            sequenceTimer.Interval = 1000;
            sequenceTimer.Tick -= MoveToNextTimer;
            sequenceTimer.Tick += ProcessSequence;
            
            currentSequenceIndex++;
            
            // Toca som de transição para o próximo timer
            PlayNextTimerSound();
            
            StartNextTimerInSequence();
            
            if (currentSequenceIndex >= sequenceTimers.Count)
            {
                currentSequenceIndex = 0;
            }
        }

        private void ProcessSequence(object sender, EventArgs e)
        {
            // Verificação contínua da sequência
        }

        private void StopSequence()
        {
            sequenceTimer.Stop();
            isSequenceRunning = false;
            SequenceFinished?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        public void RefreshList()
        {
            var currentTimerIds = timers.Select(t => t.Id).ToList();
            var updatedTimers = TimerManager.GetTimers();
            
            var preservedOrder = new List<TimerModel>();
            
            foreach (var existingTimer in timers)
            {
                var updatedTimer = updatedTimers.FirstOrDefault(t => t.Id == existingTimer.Id);
                if (updatedTimer != null)
                {
                    preservedOrder.Add(updatedTimer);
                }
            }
            
            var newTimers = updatedTimers.Where(t => !currentTimerIds.Contains(t.Id));
            preservedOrder.AddRange(newTimers);
            
            LoadTimers(preservedOrder);
        }

        private void LoadTimers(List<TimerModel> newTimers)
        {
            timers = newTimers;
            CalculateLayout();
            Invalidate();
        }

        private void CalculateLayout()
        {
            int totalContentHeight = ITEMS_START_TOP + (timers.Count * (ITEM_HEIGHT + ITEM_SPACING));
            
            int visibleAreaHeight = Height - (TITLE_TOP + TOP_MARGIN + BOTTOM_MARGIN);
            maxScrollOffset = Math.Max(0, totalContentHeight - visibleAreaHeight);
            
            scrollOffset = Math.Min(scrollOffset, maxScrollOffset);
        }

        public void StopAllTimers()
        {
            StopSequence();
        }

        public void AddTimer(TimerModel timer)
        {
            timers.Add(timer);
            CalculateLayout();
            
            scrollOffset = maxScrollOffset;
            Invalidate();
        }

        public void RemoveTimer(TimerModel timer)
        {
            timers.RemoveAll(t => t.Id == timer.Id);
            CalculateLayout();
            Invalidate();
        }

        public void UpdateTimer(TimerModel timer)
        {
            var index = timers.FindIndex(t => t.Id == timer.Id);
            if (index >= 0)
            {
                timers[index] = timer;
                Invalidate();
            }
        }
    }
}