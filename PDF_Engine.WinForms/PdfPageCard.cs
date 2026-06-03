using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PDF_Engine.WinForms
{
    public partial class PdfPageCard : UserControl
    {
        public PictureBox PageThumbnail { get; private set; }
        
        // Notice: The lblPageNumber is completely gone!
        private Button btnRotateLeft;
        private Button btnRotateRight;
        private Button btnDelete;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int OriginalPageIndex { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentRotation { get; set; } = 0;

        public event EventHandler? RotateLeftClicked;
        public event EventHandler? RotateRightClicked;
        public event EventHandler? DeleteClicked;

        public PdfPageCard(int pageIndex, Image thumbnail)
        {
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

            this.Controls.Add(PageThumbnail);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnRotateRight);
            this.Controls.Add(btnRotateLeft);

            // Automatically snap to 100% size when created
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
        // DYNAMIC SCALING ENGINE
        // ==========================================
        public void ApplyScale(float scale)
        {
            // Base A4 ratio math
            int cardWidth = (int)(200 * scale);
            int cardHeight = (int)(300 * scale);
            this.Size = new Size(cardWidth, cardHeight);
            
            int padding = (int)(10 * scale);
            
            // Limit how small the buttons can get so they are always clickable
            int buttonSize = (int)(25 * Math.Max(scale, 0.7f)); 
            int buttonY = cardHeight - buttonSize - padding;
            
            // Image takes up all space above the buttons
            PageThumbnail.Location = new Point(padding, padding);
            PageThumbnail.Size = new Size(cardWidth - (padding * 2), buttonY - (padding * 2));
            
            // Align buttons from Right to Left
            btnDelete.Size = new Size(buttonSize, buttonSize);
            btnDelete.Location = new Point(cardWidth - padding - buttonSize, buttonY);
            
            btnRotateRight.Size = new Size(buttonSize, buttonSize);
            btnRotateRight.Location = new Point(btnDelete.Left - (int)(5 * scale) - buttonSize, buttonY);
            
            btnRotateLeft.Size = new Size(buttonSize, buttonSize);
            btnRotateLeft.Location = new Point(btnRotateRight.Left - (int)(5 * scale) - buttonSize, buttonY);

            // Scale the icon font sizes so they fit perfectly
            float fontSize = Math.Max(6f, 8f * scale);
            Font btnFont = new Font("Segoe UI", fontSize);
            btnDelete.Font = btnFont;
            btnRotateRight.Font = btnFont;
            btnRotateLeft.Font = btnFont;
        }
    }
}