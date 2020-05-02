using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTLCrosswordHelper
{
    // The process of solving a cell is split up in 3 phases.
    enum Phase
    {
        Initial,
        Difference,
        Check
    }

    public partial class frmMain : Form
    {
        private const string title = "RTL Crossword Helper";
         
        // Constants for the board I played with
        private const int boardOffsetX = 8;
        private const int boardOffsetY = 127;

        private const int captureWidth = 1050;
        private const int captureHeight = 1600;

        private const int cellWidth = 69;
        private const int cellHeight = 69;

        private const int analyzeCellWidth = 60;
        private const int analyzeCellHeight = 60;

        private const int maxCellIndexX = 14;
        private const int maxCellIndexY = 18;

        // The fields were an add. Therefore ignore
        private const int ignoreAreaStartX = 5;
        private const int ignoreAreaStartY = 7;
        private const int ignoreAreaStopX = 9;
        private const int ignoreAreaStopY = 12;

        // Constants for the browser I used (Chrome)
        // Speedwise, this is the limiting factor of this tool
        private const int moveTimeOut = 120;
        private const int writeTimeout = 20;

        // The value 150 is a windows scalinge factor found in the screen settings
        // I developped this tool on a 4k screen at 150% scale, hence the factor 150
        private const float windowsScreenScaleFactor = 100f / 150f;

        // Some older crosswords need an extra click. But most of them dont
        private const bool olderCrosswords = false;

        // Initially, I tried random characters. But turns out using characters with high probability first speeds things up by a lot
        // Since Luxemburgish is closest to German, I used the German character distribution
        private static char[] charachtersToTry = { 'E', 'N', 'I', 'S', 'R', 'A', 'T', 'D', 'H', 'U', 'L', 'C', 'G', 'M', 'O', 'B', 'W', 'F', 'K', 'Z', 'P', 'V', 'J', 'Y', 'X', 'Q' };

        int currentCharachtersToTryIndex = 0;

        private Phase phase = Phase.Initial;

        private int currentCellIndexX = 0;
        private int currentCellIndexY = 0;

        
        bool singleCheck = false;
        int currentSolutionCell = 0;

        Bitmap bmpScreenCapture;
        Bitmap bmpScreenCaptureWithHighlightedIndex;
        Bitmap bmpCurrCellContent;
        Rectangle bounds;

        // We often need to compare the current cell analyze with the old cell analyze
        int lastAnalyzePureBlackPixels = 0;
        int lastAnalyzePureWhitePixels = 0;
        int lastAnalyzePureRedPixels = 0;
        int lastAnalyzeGreyPixels = 0;
        int lastAnalyzeColoredPixels = 0;

        bool latestAnalyzeHasSameResultAsPreviousAnalyze = false;

        static Random random = new Random();

        public frmMain()
        {
            InitializeComponent();
            bounds = new Rectangle(0, 0, captureWidth, captureHeight);
            bmpScreenCapture = new Bitmap(bounds.Width, bounds.Height);
            bmpScreenCaptureWithHighlightedIndex = new Bitmap(bounds.Width, bounds.Height);
        }

        // Move the current cell index to the right, or to the next line, or stop the autosolve if done 
        private void moveCellIndex()
        {
            currentCellIndexX += 1;

            if (currentCellIndexX > maxCellIndexX)
            {
                currentCellIndexX = 0;
                currentCellIndexY += 1;
            }

            if (currentCellIndexY > maxCellIndexY)
            {
                tmProcessPhase.Enabled = false;
                currentCellIndexX = 0;
                currentCellIndexY = 0;
            }

            this.Text = title + "Cell " + currentCellIndexX + " " + currentCellIndexY;
        }

        // Capture the required gamespace and preview the field and the current cell on the UI
        private void captureScreenshot()
        {
            Graphics g = Graphics.FromImage(bmpScreenCapture);          
            g.CopyFromScreen(Point.Empty, Point.Empty, new Size(bounds.Width, bounds.Height));

            // First cutout the cell that is going to be processed
            bmpCurrCellContent = bmpScreenCapture.Clone(
                new Rectangle(boardOffsetX + (currentCellIndexX * cellWidth), boardOffsetY + (currentCellIndexY * cellHeight), analyzeCellWidth, analyzeCellHeight), 
                System.Drawing.Imaging.PixelFormat.DontCare);

            // Dispose old image to avoid mem leak
            if (pnlSingleCellPreview.BackgroundImage != null)
            {
                pnlSingleCellPreview.BackgroundImage.Dispose();
            }

            // Show cell preview
            pnlSingleCellPreview.BackgroundImage = bmpCurrCellContent;

            // Draw rectangle on screen capture to give visibe feedback to the user
            Pen pen = new Pen(Color.Blue, 5);
            g.DrawRectangle(pen, boardOffsetX + (currentCellIndexX * cellWidth), boardOffsetY + (currentCellIndexY * cellHeight), analyzeCellWidth, analyzeCellHeight);

            // Dispose old image to avoid mem leak
            if (pnlScreenCapture.BackgroundImage != null)
            {
                pnlScreenCapture.BackgroundImage.Dispose();
            }

            // Show the captured space
            pnlScreenCapture.BackgroundImage = bmpScreenCapture.Clone(
                new Rectangle(0, 0, bmpScreenCapture.Width, bmpScreenCapture.Height),
                System.Drawing.Imaging.PixelFormat.DontCare);
        }

        // Analyze cell content and compare to previous analyze
        private void analyzeCurrentCell()
        {
            int pureBlackPixels = 0;
            int pureWhitePixels = 0;
            int pureRedPixels = 0;
            int greyPixels = 0;
            int coloredPixels = 0;

            for (int x = 0; x < analyzeCellWidth; x++)
            {
                for (int y = 0; y < analyzeCellHeight; y++)
                {
                    Color currColor = bmpCurrCellContent.GetPixel(x, y);

                    if (currColor.R == 0 && currColor.G == 0 && currColor.B == 0) pureBlackPixels++;
                    else if (currColor.R == 255 && currColor.G == 255 && currColor.B == 255) pureWhitePixels++;
                    else if(currColor.R == 255 && currColor.G == 0 && currColor.B == 0) pureRedPixels++;
                    else if (currColor.R == currColor.G && currColor.G == currColor.B) greyPixels++;
                    else coloredPixels++;
                }
            }

            // If all values are the same, the field did not change
            if (lastAnalyzePureBlackPixels == pureBlackPixels &&
                lastAnalyzePureWhitePixels == pureWhitePixels &&
                lastAnalyzePureRedPixels == pureRedPixels &&
                lastAnalyzeGreyPixels == greyPixels &&
                lastAnalyzeColoredPixels == coloredPixels)
            {
                latestAnalyzeHasSameResultAsPreviousAnalyze = true;
            }
            else
            {
                latestAnalyzeHasSameResultAsPreviousAnalyze = false;
            }

            // Save current results for next compare
            lastAnalyzePureBlackPixels = pureBlackPixels;
            lastAnalyzePureWhitePixels = pureWhitePixels;
            lastAnalyzePureRedPixels = pureRedPixels;
            lastAnalyzeGreyPixels = greyPixels;
            lastAnalyzeColoredPixels = coloredPixels;

            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~");
            Console.WriteLine("pureBlackPixels: " + lastAnalyzePureBlackPixels);
            Console.WriteLine("pureWhitePixels: " + lastAnalyzePureWhitePixels);
            Console.WriteLine("pureRedPixels: " + lastAnalyzePureRedPixels);
            Console.WriteLine("greyShadesPixels: " + lastAnalyzeGreyPixels);
            Console.WriteLine("coloredPixels: " + lastAnalyzeColoredPixels);
            Console.WriteLine("Same analyze result: " + latestAnalyzeHasSameResultAsPreviousAnalyze);
        }

        // After clicking on a solution cell, we need to find the corresponding highlighted cell
        private void searchForRedishCell()
        {
            Color reddishestCellColor = Color.Black;
            bool firstCellToTest = true;
            int redishestCellX = -1;
            int redishestCellY = -1;

            // Dispose old image because we are cloning it newly
            // Otherwise, the old yellow box would remain visible
            if (bmpScreenCaptureWithHighlightedIndex != null)
            {
                bmpScreenCaptureWithHighlightedIndex.Dispose();
            }

            bmpScreenCaptureWithHighlightedIndex = (Bitmap)bmpScreenCapture.Clone();
            Graphics g = Graphics.FromImage(bmpScreenCaptureWithHighlightedIndex);

            Pen pen = null;

            // For every cell, test the center pixel to see if it's possibly a candidate
            // Candidates are cells where the color is no shade of grey (R != G != B)
            for (int x = 0; x <= maxCellIndexX; x++)
            {
                for (int y = 0; y <= maxCellIndexY; y++)
                {
                    int cellCenterX = Convert.ToUInt16(boardOffsetX + (x * cellWidth) + (cellWidth / 2f));
                    int cellCenterY = Convert.ToUInt16(boardOffsetY + (y * cellHeight) + (cellHeight / 2f));

                    Color color = bmpScreenCapture.GetPixel(cellCenterX, cellCenterY);

                    if (isCellIgnored(x, y))
                    {
                        // Mark cells to ignore with a red dot
                        pen = new Pen(Color.Red, 5);
                    }
                    else if (color.R != color.G && color.G != color.B)
                    {
                        // Mark possible candidates with a green dot
                        pen = new Pen(Color.Green, 5);

                        if (firstCellToTest || (color.R > reddishestCellColor.R))
                        {
                            firstCellToTest = false;
                            reddishestCellColor = color;
                            redishestCellX = x;
                            redishestCellY = y;
                        }
                    }
                    else
                    {
                        // Mark other cells with a yellow dot
                        pen = new Pen(Color.Yellow, 5);
                    }

                    g.DrawRectangle(pen, cellCenterX, cellCenterY, 1, 1);
                }
            }

            // Mark best candidate with a big white dot
            pen = new Pen(Color.White, 10);
            g.DrawRectangle(pen, redishestCellX, redishestCellY, 1, 1);

            Console.WriteLine("Redishest Color found at x: " + redishestCellX + " y: " + redishestCellY + " having " + reddishestCellColor);

            // Also set current cell index to the best candidate to get solved next
            currentCellIndexX = redishestCellX;
            currentCellIndexY = redishestCellY;

            // Dispose old image to avoid mem leak
            if (pnlScreenCapture.BackgroundImage != null)
            {
                pnlScreenCapture.BackgroundImage.Dispose();
            }

            // Show the screencapture with the marked pixels
            pnlScreenCapture.BackgroundImage = bmpScreenCaptureWithHighlightedIndex.Clone(
                new Rectangle(0, 0, bmpScreenCapture.Width, bmpScreenCapture.Height),
                System.Drawing.Imaging.PixelFormat.DontCare);
        }

        // Calculate the real coordinates of the current index cell center and click it
        private void moveCursorAndClickCurrentCell()
        {
            uint cursorPosX = Convert.ToUInt16((boardOffsetX + (currentCellIndexX * cellWidth) + (cellWidth / 2f)) * windowsScreenScaleFactor);
            uint cursorPosY = Convert.ToUInt16((boardOffsetY + (currentCellIndexY * cellHeight) + (cellHeight / 2f)) * windowsScreenScaleFactor);
            
            // The solution cells have an additional offset of a half cell
            if (currentCellIndexY == maxCellIndexY + 1)
            {
                cursorPosY += Convert.ToUInt16((cellWidth / 2f));
            }

            moveCursorAndClick(cursorPosX, cursorPosY);
        }

        // Calculate the real coordinates of a distant corner cell center and click it
        private void moveCursorAndClickCornerCell()
        {
            uint cursorPosX;
            uint cursorPosY;

            if ((currentCellIndexY * maxCellIndexX) < (maxCellIndexY * maxCellIndexX) / 2)
            {
                // In the upper half of the field, we target the most lower right cell center
                cursorPosX = Convert.ToUInt16((boardOffsetX + (cellWidth * maxCellIndexX) + (cellWidth / 2f)) * windowsScreenScaleFactor);
                cursorPosY = Convert.ToUInt16((boardOffsetY + (cellHeight * maxCellIndexY) + (cellHeight / 2f)) * windowsScreenScaleFactor);
            }
            else
            {
                // In the lower half of the field, we target the most upper left cell center
                cursorPosX = Convert.ToUInt16((boardOffsetX + (cellWidth / 2f)) * windowsScreenScaleFactor);
                cursorPosY = Convert.ToUInt16((boardOffsetY + (cellHeight / 2f)) * windowsScreenScaleFactor);
            }

            moveCursorAndClick(cursorPosX, cursorPosY);
        }

        // Move the cursor to real coordinates and perform a click
        private void moveCursorAndClick(uint cursorPosX, uint cursorPosY)
        {
            // First move cursor
            SetCursorPos(Convert.ToInt16(cursorPosX), Convert.ToInt16(cursorPosY));

            // Then perform click
            // Even tho the mouse_event takes coordinates, it does not work without moving the cursor beforehand
            mouse_event(MOUSEEVENTF_LEFTDOWN, cursorPosX, cursorPosY, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, cursorPosX, cursorPosY, 0, 0);
        }

        // Perform the next step in the solving algorithm
        private void processNextPhase()
        {
            Console.WriteLine("crackingPhase: " + phase);
            switch(phase)
            {
                case Phase.Initial:
                    // Take an initial picture of the cell
                    moveCursorAndClickCornerCell();
                    Thread.Sleep(moveTimeOut);
                    captureScreenshot();
                    analyzeCurrentCell();

                    if (lastAnalyzeColoredPixels > 100 || isCellIgnored(currentCellIndexX, currentCellIndexY))
                    {
                        moveCellIndex();
                        phase = Phase.Initial;
                    }
                    else
                    {
                        phase = Phase.Difference;
                    }

                    break;

                case Phase.Difference:
                    // Clicking in cell and take a second picture
                    moveCursorAndClickCurrentCell();
                    Thread.Sleep(moveTimeOut);
                    captureScreenshot();
                    analyzeCurrentCell();

                    if (latestAnalyzeHasSameResultAsPreviousAnalyze)
                    {
                        moveCellIndex();
                        phase = Phase.Initial;
                    }
                    else
                    {
                        phase = Phase.Check;
                    }

                    break;

                case Phase.Check:
                    // Write in cell and take another picture
                    SendKeys.Send(charachtersToTry[currentCharachtersToTryIndex].ToString());
                    Thread.Sleep(writeTimeout);

                    if (olderCrosswords)
                    {
                        // Some of the older crossword needed the check for errors button to be clicked after every character insertion
                        moveCursorAndClick(Convert.ToUInt16(257 * windowsScreenScaleFactor) , Convert.ToUInt16(1577 * windowsScreenScaleFactor));
                        Thread.Sleep(moveTimeOut * 2);
                    }
                    else
                    {
                        moveCursorAndClickCornerCell();
                        Thread.Sleep(moveTimeOut);
                    }
                    
                    captureScreenshot();
                    analyzeCurrentCell();

                    // Check if cell has a red or black character
                    if (lastAnalyzePureRedPixels == 0 || currentCharachtersToTryIndex >= charachtersToTry.Length - 1)
                    {
                        moveCellIndex();
                        phase = Phase.Initial;
                        currentCharachtersToTryIndex = 0;

                        if (singleCheck)
                        {
                            singleCheck = false;
                            tmProcessPhase.Enabled = false;
                        }
                    }
                    else
                    {
                        moveCursorAndClickCurrentCell();
                        phase = Phase.Check;
                        currentCharachtersToTryIndex += 1;
                    }

                    break;
            }
        }

        // Function that returns if coordinates are valid game cells
        private bool isCellIgnored(int cellIndexX, int cellIndexY)
        {
            // Ignore center cells and out of bounds cells
            return (cellIndexX >= ignoreAreaStartX &&
                    cellIndexX <= ignoreAreaStopX &&
                    cellIndexY >= ignoreAreaStartY &&
                    cellIndexY <= ignoreAreaStopY) ||
                    cellIndexX < 0 ||
                    cellIndexY < 0 ||
                    cellIndexX > maxCellIndexX ||
                    cellIndexY > maxCellIndexY;
        }

        // Highlight next cell index after clicking on screen capture preview
        private void pnlScreenCapture_MouseUp(object sender, MouseEventArgs e)
        {
            Image image = pnlScreenCapture.BackgroundImage;

            if (image == null)
            {
                return;
            }

            // Calculate the originally clicked pixel in the stretched picture
            int x = image.Width * e.X / pnlScreenCapture.Width;
            int y = image.Height * e.Y / pnlScreenCapture.Height;

            // Calculate clicked cell index
            currentCellIndexX = (x - boardOffsetX) / cellWidth;
            currentCellIndexY = (y - boardOffsetY) / cellHeight;

            this.Text = title + "Cell " + currentCellIndexX + " " + currentCellIndexY;
            Console.WriteLine("Original x: " + x + " y: " + y + " iX: " + currentCellIndexX + " iY: " + currentCellIndexY);

            // Highlight the clicked cell with a rectangle to give user feedback
            Pen pen = null;
            if (isCellIgnored(currentCellIndexX, currentCellIndexY))
            {
                Console.WriteLine("Cell to be ignored!");
                pen = new Pen(Color.Red, 5);
            }
            else
            {
                pen = new Pen(Color.Yellow, 5);
            }
            
            // Dispose old image because we are cloning it newly
            // Otherwise, the old yellow box would remain visible
            if (bmpScreenCaptureWithHighlightedIndex != null)
            {
                bmpScreenCaptureWithHighlightedIndex.Dispose();
            }

            // The blue box however stays because we added it right after capture
            bmpScreenCaptureWithHighlightedIndex = (Bitmap)bmpScreenCapture.Clone();
            Graphics g = Graphics.FromImage(bmpScreenCaptureWithHighlightedIndex);

            // Draw the new rectangle 
            g.DrawRectangle(pen, boardOffsetX + (currentCellIndexX * cellWidth), boardOffsetY + (currentCellIndexY * cellHeight), analyzeCellWidth, analyzeCellHeight);

            // Dispose old image to avoid mem leak
            if (pnlScreenCapture.BackgroundImage != null)
            {
                pnlScreenCapture.BackgroundImage.Dispose();
            }

            // Set the panels background image with the modified copy of the original screen capture
            pnlScreenCapture.BackgroundImage = bmpScreenCaptureWithHighlightedIndex.Clone(
                new Rectangle(0, 0, bmpScreenCapture.Width, bmpScreenCapture.Height),
                System.Drawing.Imaging.PixelFormat.DontCare);

            // Running out of memory quickly if we don't triggering GC manually
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }

        // Start solving algorithm
        private void btnSolve_Click(object sender, EventArgs e)
        {
            tmProcessPhase.Enabled = true;
        }

        // Start solving algorithm for one cell
        private void btnSolveSingleCell_Click(object sender, EventArgs e)
        {
            // Starts normal solve but through singleCheck = true, the timer gets stopped after the first find 
            phase = Phase.Initial;
            moveCursorAndClickCornerCell();
            singleCheck = true;
            tmProcessPhase.Enabled = true;
        }

        // Start solving algorithm for the solution
        private void btnSolveSolution_Click(object sender, EventArgs e)
        {
            // Stats the timer to solve the solution fields
            currentSolutionCell = 0;
            tmSolveSolution.Enabled = true;
        }

        // Recapture and save crossword state
        private void btnSave_Click(object sender, EventArgs e)
        {
            // Save grabs a fresh screenshot (we only have one with a blue rectangle) and saves it
            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // Capture screenshot
                Graphics g = Graphics.FromImage(bmpScreenCapture);
                g.CopyFromScreen(Point.Empty, Point.Empty, new Size(bounds.Width, bounds.Height));

                // Dispose old image to avoid mem leak
                if (pnlScreenCapture.BackgroundImage != null)
                {
                    pnlScreenCapture.BackgroundImage.Dispose();
                }

                // Show the new screenshot to the user
                pnlScreenCapture.BackgroundImage = bmpScreenCapture.Clone(
                    new Rectangle(0, 0, bmpScreenCapture.Width, bmpScreenCapture.Height),
                    System.Drawing.Imaging.PixelFormat.DontCare);

                // And save it to disk
                pnlScreenCapture.BackgroundImage.Save(dialog.FileName, ImageFormat.Png);
            }
        }

        // Capture a (new) screenshot of the crossword
        private void btnCaptureScreenshot_Click(object sender, EventArgs e)
        {
            captureScreenshot();
        }

        // Timer that triggers processNextPhase
        private void tmProcessPhase_Tick(object sender, EventArgs e)
        {
            processNextPhase();
        }

        // Timer that has the solution solving algorithm
        private void tmSolveSolution_Tick(object sender, EventArgs e)
        {
            // If a single solve is still running, wait for next tick of this timer
            if (tmProcessPhase.Enabled)
            {
                return;
            }

            // Click on a solution field
            currentCellIndexX = currentSolutionCell;
            currentCellIndexY = maxCellIndexY + 1;
            moveCursorAndClickCurrentCell();

            Thread.Sleep(moveTimeOut);

            // Search for highlighted cell
            captureScreenshot();
            searchForRedishCell();

            // Goto and solve highlighted cell
            moveCursorAndClickCornerCell();
            singleCheck = true;
            tmProcessPhase.Enabled = true;

            // it is possible that more solution cells are visible than maxCellIndex 
            // In that case, the user needs solve them with single solve
            currentSolutionCell++;

            if (currentSolutionCell > maxCellIndexX)
            {
                tmSolveSolution.Enabled = false;
            }
        }

        // Code I found for systemwide keyboard shortcuts. This is absolutely nessesary since this tool takes over your mouse and keyboard with no UI focus.
        // Source: https://www.fluxbytes.com/csharp/how-to-register-a-global-hotkey-for-your-application-in-c/
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0312)
            {
                /* Note that the three lines below are not needed if you only want to register one hotkey.
                 * The below lines are useful in case you want to register multiple keys, which you can use a switch with the id as argument, or if you want to know which key/modifier was pressed for some particular reason. */

                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);                  // The key of the hotkey that was pressed.
                KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);       // The modifier of the hotkey that was pressed.
                int id = m.WParam.ToInt32();                                        // The id of the hotkey that was pressed.


                Console.WriteLine("Hotkey has been pressed!");
                // do something

                // Stop the timers to regain control over mouse and keyboard
                tmProcessPhase.Enabled = false;
                tmSolveSolution.Enabled = false;
                currentCharachtersToTryIndex = 0;
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            int id = 0;     // The id of the hotkey. 
            RegisterHotKey(this.Handle, id, (int)KeyModifier.None, Keys.Escape.GetHashCode());       // Register Shift + A as global hotkey. 
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, 0);       // Unregister hotkey with id 0 before closing the form. You might want to call this more than once with different id values if you are planning to register more than one hotkey.
        }

        // Code I found for simulating a mouse click event on a different program
        // https://stackoverflow.com/questions/2416748/how-do-you-simulate-mouse-click-in-c
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        //This is a replacement for Cursor.Position in WinForms
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);
    }
}
