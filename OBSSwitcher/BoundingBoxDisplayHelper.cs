using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Windows.Forms;

namespace OBSSwitcher
{
    public class BoundingBoxBroker
    {        
        // Max height of the main display.
        public int MaxHeight = Screen.PrimaryScreen.Bounds.Height;
        public int MaxWidth = Screen.PrimaryScreen.Bounds.Width;

        // List of all the bounding box display objects.
        public List<BoundingBoxDisplayHelper> BoundingBoxList;

        // Pane size values.
        PaneSizeValues Sizes;

        /// <summary>
        /// CTOR which takes a sizes item to create forms on the spot for all three bounding sizes.
        /// </summary>
        /// <param name="Sizes"></param>
        public BoundingBoxBroker(PaneSizeValues Sizes)
        {
            // Make a list for us to hold box items in.
            BoundingBoxList = new List<BoundingBoxDisplayHelper>();
            this.Sizes = Sizes;
        }

        /// <summary>
        /// Draws all bounding box items on the display.
        /// </summary>
        public void DrawAllBoundingBoxes()
        {
            if (Sizes.PaneSizesList.Count == 0) { throw new Exception("FAILED TO FIND ANY PANE SETTINGS ITEMS WHEN DRAWING BOUNDING BOXES!"); }
            for (int PaneCounter = 1; PaneCounter < Sizes.PaneSizesList.Count; PaneCounter++)
            {
                // Store the color and pane sizes here.
                var PaneColor = Sizes.ConsolePaneColors[PaneCounter];
                var PaneSizeValues = Sizes.PaneSizesList[PaneCounter];
                var PaneWidth = PaneSizeValues.Item2 - PaneSizeValues.Item1;

                // Add box to the list of all boxes.
                BoundingBoxList.Add(new BoundingBoxDisplayHelper(
                    PaneCounter,
                    PaneColor,
                    PaneSizeValues.Item1,
                    PaneWidth)
                );
            }
        }
        /// <summary>
         /// Draws a single bounding box item based on the params passed in to this function.
         /// </summary>
         /// <param name="BrushColor"></param>
         /// <param name="StartLocation"></param>
         /// <param name="RectangleWidth"></param>
        public void DrawBoundingBox(int PaneNumber, Color BrushColor, int StartLocation, int RectangleWidth)
        {
            // Make bounding box item here.
            var NextBox = new BoundingBoxDisplayHelper(PaneNumber, BrushColor, StartLocation, RectangleWidth);
            if (BoundingBoxList.Contains(NextBox)) { CloseBoundingBox(PaneNumber); return; }

            // Add if it was not real.
            BoundingBoxList.Add(NextBox); 
        }
        /// <summary>
        /// Closes all bounding box forms which contain the given color object for the background.
        /// </summary>
        /// <param name="BrushColor"></param>
        public void CloseBoundingBox(int PaneNumber)
        {
            // Get all the boxes to close out.
            var BoxesToClose = BoundingBoxList.Where(BoxObj => BoxObj.PaneNumber == PaneNumber).ToList();
            if (BoxesToClose.Count == 0) { return; }

            // Close the box here and remove from list of items.
            foreach (var BoundingBox in BoxesToClose)
            {
                // Remove from list.
                int IndexOfBox = BoundingBoxList.IndexOf(BoundingBox);
                BoundingBoxList.RemoveAt(IndexOfBox);

                // Close box.
                BoundingBox.Close();
            }
        }
        /// <summary>
        /// Close out all bounding box items here.
        /// </summary>
        public void CloseAllBoxes()
        {
            // Close the box here and remove from list of items.
            foreach (var BoundingBox in BoundingBoxList)
                BoundingBox.Close();

            // Clear the list.
            BoundingBoxList = new List<BoundingBoxDisplayHelper>();
        }
    }

    public class BoundingBoxDisplayHelper : Form
    {
        // Max height of the main display.
        public int MaxHeight = Screen.PrimaryScreen.Bounds.Height;
        public int MaxWidth = Screen.PrimaryScreen.Bounds.Width;
        
        // Store the color of the bounding box set.
        public Color BrushColorSet;
        public int PaneNumber;

        public BoundingBoxDisplayHelper(int PaneNumber, Color BackgroundColor, int StartLocation, int RectangleWidth)
        {
            // Store brush color. Use this for indexing later on.
            BrushColorSet = BackgroundColor;
            this.PaneNumber = PaneNumber;

            // Set size of this form and the border style.
            TopMost = true;
            Left = StartLocation;
            BackColor = BrushColorSet;
            Size = new Size(RectangleWidth, MaxHeight);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            StartPosition = FormStartPosition.Manual;

            // Show the form
            ShowDialog();
        }

        /// <summary>
        /// Draws out the text size of the pane in pixels.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Show the size of the panes here.
            string TextToShow = $"Pane {PaneNumber + 1}\nSize: {Width}x{Height}";
            Font PaneSizeFont = new Font("Arial", 36);

            // Centring.
            Rectangle CenterBox = new Rectangle(0, 0, Width, Height);
            StringFormat PaneSizeStringFormat = new StringFormat();
            PaneSizeStringFormat.Alignment = StringAlignment.Center;
            PaneSizeStringFormat.LineAlignment = StringAlignment.Center;

            // Draw the form output.
            e.Graphics.DrawString(TextToShow, PaneSizeFont, Brushes.Black, CenterBox, PaneSizeStringFormat);
            e.Graphics.DrawRectangle(Pens.Black, CenterBox);

            // Store the closing info.
            this.Closing += OnClosing;
        }

        /// <summary>
        /// Stores new width and height of form object in the main.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClosing(object sender, CancelEventArgs e)
        {
            // Get the app config here.
            Configuration AppConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Store the new sizes here and in the list of tuple objects.
            // This isnt great but it works for now.

            // UPDATE - Version 1.3.2 - Fixed issue where the saved values were not actually for panes here.
            if (Left <= 50) { Left = 0; }
            var NewSizes = new Tuple<int, int>(Left, Left + Width);
            OBSSwitcherMain.PaneSizes.PaneSizesList[PaneNumber] = NewSizes;

            // Save settings here.
            // Add one to pane number for counting.
            string PaneValueID = $"Pane{PaneNumber + 1}SizeValues";
            string ValueString = NewSizes.Item1 + "," + NewSizes.Item2;
            AppConfiguration.AppSettings.Settings[PaneValueID].Value = ValueString;

            // Apply changes here and reload them
            AppConfiguration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
