using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PDF_Engine.WinForms
{
    public partial class PdfPageCard : UserControl
    {
        public PictureBox PageThumbnail { get; private set; }
        public string SourcePdfPath { get; private set; }        
        private Button btnRotateLeft;
        private Button btnRotateRight;
        private Button btnDelete;
        
        // NEW: The Scissors Tool
        private Button btnCut;
        public bool IsCutActive { get; private set; } = false;
        private bool isLastCard = false;
        private float currentScale = 1.0f;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int OriginalPageIndex { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentRotation { get; set; } = 0;

        public event EventHandler? RotateLeftClicked;
        public event EventHandler? RotateRightClicked;
        public event EventHandler? DeleteClicked;

        public PdfPageCard(string sourcePath, int pageIndex, Image thumbnail)
        {
            SourcePdfPath = sourcePath;
            OriginalPageIndex = pageIndex;
            InitializeCard();
            SetupComponents(thumbnail);
        }

        private void InitializeCard()
        {
            this.BackColor = Color.FromArgb(45, 45, 48); 
            this.Margin = new Padding(10); 
            this.Cursor = Cursors.Hand; 
        }

        private void SetupComponents(Image thumbnail)
        {
            PageThumbnail = new PictureBox
            {
                Image = thumbnail,
                SizeMode = PictureBoxSizeMode.Zoom, 
                BackColor = Color.White
            };

            btnDelete = CreateCardButton("🗑", Color.IndianRed);
            btnDelete.Click += (s, e) => DeleteClicked?.Invoke(this, EventArgs.Empty);

            btnRotateRight = CreateCardButton("↪", Color.LightGray);
            btnRotateRight.Click += (s, e) => 
            {
                CurrentRotation = (CurrentRotation + 90) % 360;
                PageThumbnail.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                PageThumbnail.Refresh();
                RotateRightClicked?.Invoke(this, EventArgs.Empty); 
            };

            btnRotateLeft = CreateCardButton("↩", Color.LightGray);
            btnRotateLeft.Click += (s, e) => 
            {
                CurrentRotation = (CurrentRotation - 90) % 360;
                PageThumbnail.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                PageThumbnail.Refresh();
                RotateLeftClicked?.Invoke(this, EventArgs.Empty);
            };

            // NEW: Setup the Cut Button
            btnCut = new Button
            {
                Text = "✂",
                ForeColor = Color.Gray,
                BackColor = Color.FromArgb(35, 35, 38), // Slightly darker to look like a gap
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCut.FlatAppearance.BorderSize = 0;
            btnCut.Click += BtnCut_Click;

            this.Controls.Add(PageThumbnail);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnRotateRight);
            this.Controls.Add(btnRotateLeft);
            this.Controls.Add(btnCut);

            ApplyScale(1.0f);
        }

        private Button CreateCardButton(string text, Color foreColor)
        {
            return new Button
            {
                Text = text,
                ForeColor = foreColor,
                BackColor = Color.FromArgb(60, 60, 65),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Default
            };
        }

        // ==========================================
        // CUT MARKER LOGIC
        // ==========================================
        private void BtnCut_Click(object? sender, EventArgs e)
        {
            IsCutActive = !IsCutActive;
            UpdateCutVisuals();
        }

        private void UpdateCutVisuals()
        {
            if (IsCutActive)
            {
                btnCut.BackColor = Color.IndianRed;
                btnCut.ForeColor = Color.White;
                btnCut.Font = new Font("Segoe UI", Math.Max(12f, 16f * currentScale), FontStyle.Bold);
            }
            else
            {
                btnCut.BackColor = Color.FromArgb(35, 35, 38);
                btnCut.ForeColor = Color.Gray;
                btnCut.Font = new Font("Segoe UI", Math.Max(10f, 14f * currentScale), FontStyle.Regular);
            }
        }

        public void SetAsLastCard(bool isLast)
        {
            if (isLastCard != isLast)
            {
                isLastCard = isLast;
                btnCut.Visible = !isLast;
                if (isLast) IsCutActive = false; // Failsafe: Cannot cut after the last page
                UpdateCutVisuals();
                ApplyScale(currentScale); // Recalculate physical width to hide/show the margin
            }
        }

        // ==========================================
        // DYNAMIC SCALING ENGINE
        // ==========================================
        public void ApplyScale(float scale)
        {
            currentScale = scale;
            
            // Core measurements
            int pageWidth = (int)(200 * scale);
            int cutWidth = (int)(40 * scale);
            int cardHeight = (int)(300 * scale);
            int padding = (int)(10 * scale);
            
            // If it is the last card, we completely cut off the right-side margin!
            this.Size = new Size(pageWidth + (isLastCard ? 0 : cutWidth), cardHeight);
            
            int buttonSize = (int)(25 * Math.Max(scale, 0.7f)); 
            int buttonY = cardHeight - buttonSize - padding;
            
            PageThumbnail.Location = new Point(padding, padding);
            PageThumbnail.Size = new Size(pageWidth - (padding * 2), buttonY - (padding * 2));
            
            btnDelete.Size = new Size(buttonSize, buttonSize);
            btnDelete.Location = new Point(pageWidth - padding - buttonSize, buttonY);
            
            btnRotateRight.Size = new Size(buttonSize, buttonSize);
            btnRotateRight.Location = new Point(btnDelete.Left - (int)(5 * scale) - buttonSize, buttonY);
            
            btnRotateLeft.Size = new Size(buttonSize, buttonSize);
            btnRotateLeft.Location = new Point(btnRotateRight.Left - (int)(5 * scale) - buttonSize, buttonY);

            // Position the tall Cut Button in the new right-hand margin
            btnCut.Size = new Size((int)(30 * scale), cardHeight - (padding * 2));
            btnCut.Location = new Point(pageWidth + (int)(5 * scale), padding);
            
            UpdateCutVisuals(); // Ensure fonts scale properly

            float fontSize = Math.Max(6f, 8f * scale);
            Font btnFont = new Font("Segoe UI", fontSize);
            btnDelete.Font = btnFont;
            btnRotateRight.Font = btnFont;
            btnRotateLeft.Font = btnFont;
        }
    }
}