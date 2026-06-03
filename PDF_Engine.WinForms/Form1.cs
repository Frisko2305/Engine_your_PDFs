using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PdfiumViewer;

namespace PDF_Engine.WinForms
{
    public partial class Form1 : Form
    {
        // 1. Initial State Buttons
        private Button btnUploadInitial;
        private Button btnCloseApp;
        
        // 2. The Main Layout Grids
        private TableLayoutPanel mainLayout;
        private TableLayoutPanel topToolbar;
        private FlowLayoutPanel workspaceGrid;

        // 3. Workspace Controls
        private Button btnClearAll;
        private Button btnAddPdf;
        private Button btnZoomIn;
        private Button btnZoomOut;
        private Button btnSaveExport;

        // State tracking
        private string currentPdfPath = string.Empty;
        private List<PdfPageCard> pageCards = new List<PdfPageCard>();
        private float globalScale = 1.0f; 

        public Form1()
        {
            InitializeComponent();
            SetupMainWindow();
            ShowInitialState();
            this.Load += Form1_Load;
        }

        private void SetupMainWindow()
        {
            this.Text = "PDF Engine - Professional Workspace";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.WindowState = FormWindowState.Maximized; 
        }

        private void ShowInitialState()
        {
            // ====================================================================
            // 1. THE MAIN TABLE LAYOUT (3 Rows: Top Bar, Body, Bottom Bar)
            // ====================================================================
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Visible = false // Hidden until a PDF is uploaded
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));  // Top Toolbar: 70px fixed
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Body: Takes remaining space
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));  // Bottom Toolbar: 80px fixed
            this.Controls.Add(mainLayout);

            // ====================================================================
            // 2. ROW 0: TOP TOOLBAR (3 Columns: Left, Center, Right)
            // ====================================================================
            topToolbar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 3,
                BackColor = Color.FromArgb(20, 20, 20)
            };
            topToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f)); // Left
            topToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f)); // Center
            topToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f)); // Right

            // Left: Drop All Button
            btnClearAll = CreateUiButton("🗑️ Drop All Pages", Color.IndianRed);
            btnClearAll.Anchor = AnchorStyles.Left; // Locks to the left of the cell
            btnClearAll.Margin = new Padding(20, 0, 0, 0);
            btnClearAll.Click += (s, e) => ResetWorkspace();

            // Center: Zoom Controls
            FlowLayoutPanel zoomPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                Anchor = AnchorStyles.None, // Locks to dead-center of the cell!
                WrapContents = false
            };
            btnZoomOut = CreateUiButton("➖ 🔍", Color.FromArgb(60, 60, 65), new Size(60, 40));
            btnZoomIn = CreateUiButton("➕ 🔍", Color.FromArgb(60, 60, 65), new Size(60, 40));
            btnZoomOut.Click += (s, e) => { if (globalScale > 0.4f) { globalScale -= 0.2f; UpdateGridScale(); } };
            btnZoomIn.Click += (s, e) => { if (globalScale < 2.5f) { globalScale += 0.2f; UpdateGridScale(); } };
            zoomPanel.Controls.Add(btnZoomOut);
            zoomPanel.Controls.Add(btnZoomIn);

            // Right: Add Another PDF Button
            btnAddPdf = CreateUiButton("📄 Add Another PDF", Color.SeaGreen);
            btnAddPdf.Anchor = AnchorStyles.Right; // Locks to the right of the cell
            btnAddPdf.Margin = new Padding(0, 0, 20, 0);
            btnAddPdf.Click += BtnAddPdf_Click;

            topToolbar.Controls.Add(btnClearAll, 0, 0);
            topToolbar.Controls.Add(zoomPanel, 1, 0);
            topToolbar.Controls.Add(btnAddPdf, 2, 0);
            mainLayout.Controls.Add(topToolbar, 0, 0);

            // ====================================================================
            // 3. ROW 1: THE BODY (PDF Pages Grid)
            // ====================================================================
            workspaceGrid = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(30, 30, 30),
                AllowDrop = true 
            };
            workspaceGrid.DragEnter += WorkspaceGrid_DragEnter;
            workspaceGrid.DragDrop += WorkspaceGrid_DragDrop;
            mainLayout.Controls.Add(workspaceGrid, 0, 1);

            // ====================================================================
            // 4. ROW 2: THE BOTTOM (Save & Export)
            // ====================================================================
            btnSaveExport = CreateUiButton("💾 Save & Export Document", Color.FromArgb(0, 120, 215), new Size(300, 50));
            btnSaveExport.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnSaveExport.Anchor = AnchorStyles.None; // Dead-center of the bottom row
            btnSaveExport.Click += (s, e) => MessageBox.Show("Save & Export triggered! Next step: Wiring up PDF_Engine.Core!");
            mainLayout.Controls.Add(btnSaveExport, 0, 2);

            // ====================================================================
            // 5. INITIAL STARTUP SCREEN BUTTONS
            // ====================================================================
            btnUploadInitial = CreateUiButton("➕ Upload PDF to Start", Color.FromArgb(0, 120, 215), new Size(300, 100));
            btnUploadInitial.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            btnUploadInitial.Click += BtnUploadInitial_Click;

            btnCloseApp = CreateUiButton("❌ Close App", Color.IndianRed, new Size(200, 50));
            btnCloseApp.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnCloseApp.Click += (s, e) => this.Close();

            this.Controls.Add(btnUploadInitial);
            this.Controls.Add(btnCloseApp);

            this.Resize += (s, e) => { if (btnUploadInitial.Visible) CenterInitialButtons(); };
        }

        // Helper method to create uniform buttons
        private Button CreateUiButton(string text, Color backColor, Size? size = null)
        {
            return new Button
            {
                Text = text,
                ForeColor = Color.White,
                BackColor = backColor,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand,
                Size = size ?? new Size(180, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            CenterInitialButtons();
        }

        private void CenterInitialButtons()
        {
            int spacing = 20;
            int totalHeight = btnUploadInitial.Height + spacing + btnCloseApp.Height;
            int startY = (this.ClientSize.Height - totalHeight) / 2;

            btnUploadInitial.Location = new Point((this.ClientSize.Width - btnUploadInitial.Width) / 2, startY);
            btnCloseApp.Location = new Point((this.ClientSize.Width - btnCloseApp.Width) / 2, startY + btnUploadInitial.Height + spacing);
        }

        // ====================================================================
        // WORKSPACE LOGIC
        // ====================================================================

        private void ResetWorkspace()
        {
            // The "Drop All Pages" & Failsafe Logic
            workspaceGrid.Controls.Clear();
            pageCards.Clear();
            mainLayout.Visible = false;      // Hide the entire UI Table
            btnUploadInitial.Visible = true; // Show start buttons
            btnCloseApp.Visible = true;
            currentPdfPath = string.Empty;
            globalScale = 1.0f;              // Reset zoom
            CenterInitialButtons();
        }

        private void UpdateGridScale()
        {
            workspaceGrid.SuspendLayout(); 
            foreach (Control c in workspaceGrid.Controls)
            {
                if (c is PdfPageCard card) card.ApplyScale(globalScale);
            }
            workspaceGrid.ResumeLayout(); 
        }

        private void BtnUploadInitial_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    currentPdfPath = openFileDialog.FileName;
                    btnUploadInitial.Visible = false;
                    btnCloseApp.Visible = false;
                    mainLayout.Visible = true; // Reveal the entire Table Layout!

                    LoadPdfIntoWorkspace(currentPdfPath, append: false);
                }
            }
        }

        private void BtnAddPdf_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // By passing 'append: true', it just adds the new pages to the end of the existing list!
                    LoadPdfIntoWorkspace(openFileDialog.FileName, append: true);
                }
            }
        }

        private void LoadPdfIntoWorkspace(string filePath, bool append)
        {
            if (!append)
            {
                workspaceGrid.Controls.Clear();
                pageCards.Clear();
            }

            try
            {
                using (var document = PdfDocument.Load(filePath))
                {
                    for (int i = 0; i < document.PageCount; i++)
                    {
                        Image pageImage = document.Render(i, 400, 600, true);
                        var card = new PdfPageCard(i, pageImage);
                        card.ApplyScale(globalScale);

                        card.MouseDown += Card_MouseDown;
                        card.PageThumbnail.MouseDown += Card_MouseDown; 

                        card.DeleteClicked += (s, e) => 
                        {
                            workspaceGrid.Controls.Remove(card); 
                            pageCards.Remove(card);              

                            // If we manually delete the very last page, trigger the full reset
                            if (workspaceGrid.Controls.Count == 0) ResetWorkspace();
                        };

                        pageCards.Add(card);
                        workspaceGrid.Controls.Add(card);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (pageCards.Count == 0) ResetWorkspace(); // Only reset if nothing is left on screen
            }
        }

        // ====================================================================
        // DRAG AND DROP ENGINE LOGIC
        // ====================================================================
        private void Card_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var card = sender as PdfPageCard ?? (sender as PictureBox)?.Parent as PdfPageCard;
                if (card != null) card.DoDragDrop(card, DragDropEffects.Move);
            }
        }

        private void WorkspaceGrid_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(typeof(PdfPageCard)))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void WorkspaceGrid_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            
            PdfPageCard? draggedCard = e.Data.GetData(typeof(PdfPageCard)) as PdfPageCard;
            if (draggedCard == null) return;

            Point clientPoint = workspaceGrid.PointToClient(new Point(e.X, e.Y));
            int targetIndex = workspaceGrid.Controls.Count - 1;
            bool explicitTargetFound = false;

            for (int i = 0; i < workspaceGrid.Controls.Count; i++)
            {
                Control c = workspaceGrid.Controls[i];
                if (clientPoint.Y < c.Top - c.Margin.Top)
                {
                    targetIndex = i;
                    explicitTargetFound = true;
                    break;
                }
                
                if (clientPoint.Y >= c.Top - c.Margin.Top && clientPoint.Y <= c.Bottom + c.Margin.Bottom)
                {
                    if (clientPoint.X < c.Left + (c.Width / 2))
                    {
                        targetIndex = i;
                        explicitTargetFound = true;
                        break;
                    }
                    else if (clientPoint.X < c.Right + c.Margin.Right)
                    {
                        targetIndex = i + 1;
                        explicitTargetFound = true;
                        break;
                    }
                }
            }

            int originalIndex = workspaceGrid.Controls.GetChildIndex(draggedCard);
            if (explicitTargetFound && originalIndex < targetIndex) targetIndex--;
            if (originalIndex != targetIndex) workspaceGrid.Controls.SetChildIndex(draggedCard, targetIndex);
        }
    }
}