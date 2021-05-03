using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBSSwitcher
{
    public class PaneSizeValues
    {
        // Max Size of the main display
        public Tuple<int, int> StartScreenSizes = new Tuple<int, int>(0,0);
        public Tuple<int, int> MaxScreenSizes = new Tuple<int, int>(0,0);

        // Pairs of window widths for the current window selected.
        public Tuple<int, int> PaneOneValues { get; set; }
        public Tuple<int, int> PaneTwoValues { get; set; }
        public Tuple<int, int> PaneThreeValues { get; set; }

        /// <summary>
        /// Ctor for non app config use.
        /// </summary>
        /// <param name="WindowOneMax"></param>
        /// <param name="WindowTwoMax"></param>
        /// <param name="WindowThreeMax"></param>
        public PaneSizeValues(int WindowOneMax, int WindowTwoMax, int WindowThreeMax)
        {
            // Set the tuple values for the current set of windows/panes.
            PaneOneValues = new Tuple<int, int>(0, WindowOneMax);
            PaneTwoValues = new Tuple<int, int>(WindowOneMax + 1, WindowTwoMax);
            PaneThreeValues = new Tuple<int, int>(WindowTwoMax + 1, WindowThreeMax);

            // Set the max sizes and min sizes.
            StartScreenSizes = new Tuple<int, int>(0, 0);
            MaxScreenSizes = new Tuple<int, int>(
                    System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                    System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height
            );
        }

        /// <summary>
        /// Ctor for app config value pulling.
        /// </summary>
        public PaneSizeValues()
        {
            for (int PaneInt = 1; PaneInt <= 3; PaneInt++)
            {
                string CurrentPane = ConfigurationManager.AppSettings.Get("Pane" + PaneInt + "SizeValues");
                string[] ValuesSplit = CurrentPane.Split(',');

                int xMin = int.Parse(ValuesSplit[0]);
                int xMax = int.Parse(ValuesSplit[1]);

                var ThisTuple = new Tuple<int, int>(xMin, xMax);
                switch (PaneInt)
                {
                    case (1):
                        PaneOneValues = ThisTuple;
                        break;

                    case (2):
                        PaneTwoValues = ThisTuple;
                        break;

                    case (3):
                        PaneThreeValues = ThisTuple;
                        break;
                }
            }
        }
    }
}
