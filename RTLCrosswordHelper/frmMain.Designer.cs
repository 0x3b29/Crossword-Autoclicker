namespace RTLCrosswordHelper
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnCaptureScreenshot = new System.Windows.Forms.Button();
            this.tmProcessPhase = new System.Windows.Forms.Timer(this.components);
            this.pnlScreenCapture = new System.Windows.Forms.Panel();
            this.pnlSingleCellPreview = new System.Windows.Forms.Panel();
            this.btnSolve = new System.Windows.Forms.Button();
            this.btnSolveSingleCell = new System.Windows.Forms.Button();
            this.btnSolveSolution = new System.Windows.Forms.Button();
            this.tmSolveSolution = new System.Windows.Forms.Timer(this.components);
            this.btnSave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnCaptureScreenshot
            // 
            this.btnCaptureScreenshot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCaptureScreenshot.Font = new System.Drawing.Font("Arial", 32F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCaptureScreenshot.Location = new System.Drawing.Point(826, 12);
            this.btnCaptureScreenshot.Margin = new System.Windows.Forms.Padding(0);
            this.btnCaptureScreenshot.Name = "btnCaptureScreenshot";
            this.btnCaptureScreenshot.Size = new System.Drawing.Size(125, 117);
            this.btnCaptureScreenshot.TabIndex = 5;
            this.btnCaptureScreenshot.Text = "↻";
            this.btnCaptureScreenshot.UseVisualStyleBackColor = true;
            this.btnCaptureScreenshot.Click += new System.EventHandler(this.btnCaptureScreenshot_Click);
            // 
            // tmProcessPhase
            // 
            this.tmProcessPhase.Interval = 1;
            this.tmProcessPhase.Tick += new System.EventHandler(this.tmProcessPhase_Tick);
            // 
            // pnlScreenCapture
            // 
            this.pnlScreenCapture.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlScreenCapture.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.pnlScreenCapture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pnlScreenCapture.Location = new System.Drawing.Point(12, 145);
            this.pnlScreenCapture.Name = "pnlScreenCapture";
            this.pnlScreenCapture.Size = new System.Drawing.Size(1078, 1234);
            this.pnlScreenCapture.TabIndex = 7;
            this.pnlScreenCapture.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnlScreenCapture_MouseUp);
            // 
            // pnlSingleCellPreview
            // 
            this.pnlSingleCellPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlSingleCellPreview.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.pnlSingleCellPreview.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pnlSingleCellPreview.Location = new System.Drawing.Point(969, 12);
            this.pnlSingleCellPreview.Name = "pnlSingleCellPreview";
            this.pnlSingleCellPreview.Size = new System.Drawing.Size(121, 117);
            this.pnlSingleCellPreview.TabIndex = 6;
            // 
            // btnSolve
            // 
            this.btnSolve.Location = new System.Drawing.Point(12, 53);
            this.btnSolve.Name = "btnSolve";
            this.btnSolve.Size = new System.Drawing.Size(229, 35);
            this.btnSolve.TabIndex = 2;
            this.btnSolve.Text = "Solve &All (slow)";
            this.btnSolve.UseVisualStyleBackColor = true;
            this.btnSolve.Click += new System.EventHandler(this.btnSolve_Click);
            // 
            // btnSolveSingleCell
            // 
            this.btnSolveSingleCell.Location = new System.Drawing.Point(12, 94);
            this.btnSolveSingleCell.Name = "btnSolveSingleCell";
            this.btnSolveSingleCell.Size = new System.Drawing.Size(229, 35);
            this.btnSolveSingleCell.TabIndex = 3;
            this.btnSolveSingleCell.Text = "Solve Singl&e";
            this.btnSolveSingleCell.UseVisualStyleBackColor = true;
            this.btnSolveSingleCell.Click += new System.EventHandler(this.btnSolveSingleCell_Click);
            // 
            // btnSolveSolution
            // 
            this.btnSolveSolution.Location = new System.Drawing.Point(12, 12);
            this.btnSolveSolution.Name = "btnSolveSolution";
            this.btnSolveSolution.Size = new System.Drawing.Size(229, 35);
            this.btnSolveSolution.TabIndex = 1;
            this.btnSolveSolution.Text = "Solve Solu&tion (fast)";
            this.btnSolveSolution.UseVisualStyleBackColor = true;
            this.btnSolveSolution.Click += new System.EventHandler(this.btnSolveSolution_Click);
            // 
            // tmSolveSolution
            // 
            this.tmSolveSolution.Tick += new System.EventHandler(this.tmSolveSolution_Tick);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(247, 12);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(126, 117);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "&Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1102, 1391);
            this.Controls.Add(this.btnCaptureScreenshot);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnSolveSolution);
            this.Controls.Add(this.btnSolveSingleCell);
            this.Controls.Add(this.btnSolve);
            this.Controls.Add(this.pnlSingleCellPreview);
            this.Controls.Add(this.pnlScreenCapture);
            this.DoubleBuffered = true;
            this.Name = "frmMain";
            this.Text = "RTL Crossword Helper";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCaptureScreenshot;
        private System.Windows.Forms.Timer tmProcessPhase;
        private System.Windows.Forms.Panel pnlScreenCapture;
        private System.Windows.Forms.Panel pnlSingleCellPreview;
        private System.Windows.Forms.Button btnSolve;
        private System.Windows.Forms.Button btnSolveSingleCell;
        private System.Windows.Forms.Button btnSolveSolution;
        private System.Windows.Forms.Timer tmSolveSolution;
        private System.Windows.Forms.Button btnSave;
    }
}

