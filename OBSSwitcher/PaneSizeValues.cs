using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OBSSwitcher
{
    public class PaneSizeValues
    {
        // Max Size of the main display
        public Tuple<int, int> StartScreenSizes = new Tuple<int, int>(0,0);
        public Tuple<int, int> MaxScreenSizes = new Tuple<int, int>(0,0);

        // List of all pane size values.
        public List<Tuple<int,int>> PaneSizesList = new List<Tuple<int, int>>();
        public List<Color> ConsolePaneColors = new List<Color>();

        /// <summary>
        /// Ctor for app config value pulling.
        /// </summary>
        public PaneSizeValues()
        {
            // Store the start and max window sizes.
            StartScreenSizes = new Tuple<int, int>(0, 0);
            MaxScreenSizes = new Tuple<int, int>(
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height
            );

            // Main Pane base color (White) and main sizes.
            // PaneSizesList.Add(new Tuple<int, int>(0, MaxScreenSizes.Item1));
            // ConsolePaneColors.Add(Color.White);

            // Get the pane size values from app config.
            bool KeepChecking = true;
            int PaneCounter = 1;
            while (KeepChecking)
            {
                // Get next pane item here. If it's null break off.
                string CurrentPane = ConfigurationManager.AppSettings.Get("Pane" + PaneCounter + "SizeValues");
                if (string.IsNullOrEmpty(CurrentPane)) { break; }

                // Convert the pane info here.
                string[] ValuesSplit = CurrentPane.Split(',');
                int xMin = int.Parse(ValuesSplit[0]);
                int xMax = int.Parse(ValuesSplit[1]);

                // RNG object
                var RandomGen = new Random();
                int ColorCount = ConsolePaneColors.Count;
                while (ConsolePaneColors.Count == ColorCount)
                {
                    // Get a color for this item.
                    var NextColor = Color.FromArgb(
                        RandomGen.Next(256),
                        RandomGen.Next(256),
                        RandomGen.Next(256)
                    );

                    // Add it if unique. Otherwise skip and make a new color.
                    if (!ConsolePaneColors.Contains(NextColor)) { ConsolePaneColors.Add(NextColor); }
                    else { Thread.Sleep(500); }
                }

                // Check for overalp.
                if (PaneSizesList.Count == 0) { PaneSizesList.Add(new Tuple<int, int>(xMin, xMax)); }
                else
                {
                    // Add one to the last X Value.
                    int LastXMax = PaneSizesList[PaneSizesList.Count - 1].Item2;
                    if (xMax <= LastXMax) { xMax = LastXMax + 1; }

                    // Add to list of panes here.
                    var NextSizes = new Tuple<int, int>(xMin, xMax);
                    PaneSizesList.Add(NextSizes);

                    // Save settings for this pane so we don't have to worry about it later.
                    Configuration AppConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    
                    // Store settings value as a string here.
                    // STORE AS COUNT - 1 SINCE WE HAVE ALREADY ADDED THESE VALUES TO THE PANE LIST
                    string PaneValueID = $"Pane{PaneSizesList.Count - 1}SizeValues";
                    string ValueString = NextSizes.Item1 + "," + NextSizes.Item2;
                    AppConfiguration.AppSettings.Settings[PaneValueID].Value = ValueString;

                    // Apply changes here and reload them
                    AppConfiguration.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                }

                // Tick pane counter 
                PaneCounter++;
            }

            #region OLD PANE SIZE METHOD
            /* REMOVED. Old pane setup with only three possible screen values.
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
            }*/
            #endregion
        }
    }
}
