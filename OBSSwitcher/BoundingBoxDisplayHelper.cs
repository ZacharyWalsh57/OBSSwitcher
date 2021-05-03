using System;
using System.Collections.Generic;
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
            // Widths for rectangles here.
            int RectOneWidth = Sizes.PaneOneValues.Item2 - Sizes.PaneOneValues.Item1;
            int RectTwoWidth = Sizes.PaneTwoValues.Item2 - Sizes.PaneTwoValues.Item1;
            int RectThreeWidth = Sizes.PaneThreeValues.Item2 - Sizes.PaneThreeValues.Item1;

            // Make a new bounding box items.
            BoundingBoxList.Add(new BoundingBoxDisplayHelper(Color.Red, Sizes.PaneOneValues.Item1, RectOneWidth));
            BoundingBoxList.Add(new BoundingBoxDisplayHelper(Color.Green, Sizes.PaneTwoValues.Item1, RectTwoWidth));
            BoundingBoxList.Add(new BoundingBoxDisplayHelper(Color.Blue, Sizes.PaneThreeValues.Item1, RectThreeWidth));
        }
        /// <summary>
        /// Draws a single bounding box item based on the params passed in to this function.
        /// </summary>
        /// <param name="BrushColor"></param>
        /// <param name="StartLocation"></param>
        /// <param name="RectangleWidth"></param>
        public void DrawBoundingBox(Color BrushColor, int StartLocation, int RectangleWidth)
        {
            // Make bounding box item here.
            var NextBox = new BoundingBoxDisplayHelper(BrushColor, StartLocation, RectangleWidth);
            if (BoundingBoxList.Contains(NextBox)) { CloseBoundingBox(BrushColor); return; }

            // Add if it was not real.
            BoundingBoxList.Add(NextBox); 
        }
        /// <summary>
        /// Closes all bounding box forms which contain the given color object for the background.
        /// </summary>
        /// <param name="BrushColor"></param>
        public void CloseBoundingBox(Color BrushColor)
        {
            // Get all the boxes to close out.
            var BoxesToClose = BoundingBoxList.Where(BoxObj => BoxObj.BrushColorSet == BrushColor).ToList();
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

        public BoundingBoxDisplayHelper(Color BrushColor, int StartLocation, int RectangleWidth)
        {
            // Store brush color. Use this for indexing later on.
            BrushColorSet = BrushColor;

            // Set size of this form and the border style.
            BackColor = BrushColor;
            Size = new Size(RectangleWidth, MaxHeight);
            Left = StartLocation;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition = FormStartPosition.Manual;

            // Draw our rectangle here.
            Graphics FormGfx = CreateGraphics();
            Pen BrushPen = new Pen(BrushColor);
            FormGfx.DrawRectangle(BrushPen, StartLocation, 0, RectangleWidth, MaxHeight);

            // Show dialog here.
            ShowDialog();
        }
    }
}
