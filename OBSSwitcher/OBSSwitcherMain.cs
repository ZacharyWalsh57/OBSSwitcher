using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Colorful;

// Color Console output.
using Console = Colorful.Console;

namespace OBSSwitcher
{
    public class OBSSwitcherMain
    {
        // Pane sizes and the Hotkeys for them.
        public static PaneSizeValues PaneSizes;
        public static PaneHotKeys HotKeys;
        public static KeySender Sender;
        public static ConsoleResizeChecker ConsoleResizer;

        // Update time value.
        public static int DelayTime;

        public static void Main(string[] args)
        {
            // Set console size. 
            Console.SetWindowSize(60, 60);

            // Setup Pane Sizes and Hotkeys here.
            PaneSizes = new PaneSizeValues();
            HotKeys = new PaneHotKeys(PaneSizes);
            Sender = new KeySender(HotKeys);
            ConsoleResizer = new ConsoleResizeChecker(PaneSizes, Sender);

            // Get DelayTime
            if (!int.TryParse(ConfigurationManager.AppSettings.Get("DelayTime"), out DelayTime))
                DelayTime = 1000;

            // Run switcher here.
            while (true) { RunSwitcher(); }
        }

        /// <summary>
        /// Kicks off the pane switching loop.
        /// </summary>
        private static void RunSwitcher()
        {
            // Start the console resizer
            ConsoleResizer.ResizeWanted = true;
            ConsoleResizer.CheckForResize();

            // Write config info.
            WriteConfigInfo(PaneSizes, Sender);

            // Wait for user to hit enter to begin.
            // Check for next input key here.
            while (true)
            {
                // If no new key wait.
                if (!Console.KeyAvailable) { continue; }
                var NextKey = Console.ReadKey(true);

                if (NextKey.Key == ConsoleKey.Enter) { break; }
                if (NextKey.Key == ConsoleKey.R)
                {
                    // Check for R again to confirm.
                    var ResetKey = Console.ReadKey(true);
                    if (ResetKey.Key != ConsoleKey.R) { continue; }

                    // Reset the pane keys if wanted.
                    PaneSizes = new PaneSizeValues(true);
                    WriteConfigInfo(PaneSizes, Sender);
                }
                if (NextKey.Key == ConsoleKey.P)
                {
                    // Draw new boxes and update the UI
                    HotKeys.ProcessPaneKey();
                    PaneSizes = new PaneSizeValues();
                    WriteConfigInfo(PaneSizes, Sender);
                }
            }

            // Clear the console.
            Console.Clear();

            // Store last move so we dont repeat
            int LastMoveIndex = -1;

            // Loop only while we have not key pressed and we can read a key.
            while (!(Console.KeyAvailable && (Console.ReadKey(true).Key == ConsoleKey.Escape)))
            {
                // Wait the delay time and get mouse location
                Thread.Sleep(DelayTime);
                var XAndYPos = new MouseCords();

                // Check invalid Y pos value.
                if (XAndYPos.PosY < 0)
                {
                    // Make sure we wanna do this.
                    if (ConfigurationManager.AppSettings.Get("ForcePositiveY") != "TRUE") { continue; }

                    // Change the view if needed.
                    if (LastMoveIndex != -1)
                    {
                        PrintCurrentPane(-1, null);
                        Sender.SwitchView(-1);
                    }

                    // Move on the loop.
                    continue;
                }

                // Find the X Pane item range.
                if (PaneSizes.PaneSizesList.Count == 0) { throw new Exception("FAILED TO FIND ANY PANE SETTINGS ITEMS WHEN CHANGING VIEWS!"); }
                for (int PaneIndex = 0; PaneIndex < PaneSizes.PaneSizesList.Count; PaneIndex++)
                {
                    // Get the top and bottom range items.
                    int MinRange = PaneSizes.PaneSizesList[PaneIndex].Item1;
                    int MaxRange = PaneSizes.PaneSizesList[PaneIndex].Item2;

                    // Check if we're in range and need to move.
                    // RANGE IS SETUP AS MIN AND COUNT. NOT MIN AND MAX!!
                    if (!Enumerable.Range(MinRange, MaxRange - MinRange).Contains(XAndYPos.PosX)) { continue; }
                    if (PaneIndex == LastMoveIndex) { break; }

                    // Store the index of the pane we are on and print that info out.
                    Sender.SwitchView(PaneIndex + 1);
                    PrintCurrentPane(PaneIndex, XAndYPos);
                    LastMoveIndex = PaneIndex;

                    // Move on
                    break;
                }
            }

            // Stop resizer, move back to home and clear out.
            ConsoleResizer.ResizeWanted = false;
            Sender.SwitchView(-1);
            PrintCurrentPane(-1, null);
            Console.Clear();
        }


        /// <summary>
        /// Write out the current config for the application.
        /// </summary>
        /// <param name="PaneSize">Sizes object</param>
        /// <param name="Sender">Key sender</param>
        public static void WriteConfigInfo(PaneSizeValues PaneSize, KeySender Sender)
        {
            // Store a temp file.
            string TempFile = Path.GetTempFileName();
            using (var ConsoleWriter = new StreamWriter(TempFile))
            {
                // Set Console Output here.
                Console.Clear();
                Console.SetOut(ConsoleWriter);

                // Title info here.
                Console.Clear();
                Console.WriteLine("+---------------------------------------------+");
                Console.WriteLine("|                                             |");
                Console.WriteLine("|          OBS Switcher Version 1.4.3         |");
                Console.WriteLine("|~Created And Maintained By Zack Walsh - 2021~|");
                Console.WriteLine("|                                             |");
                Console.WriteLine("|---------------------------------------------|");
                Console.WriteLine("|    =Configuration Info For This Session=    |");
                Console.WriteLine("|---------------------------------------------|");

                // Print out hotkey info.
                Console.WriteLine("| {0,0} {1,33}", "OBS HotKeys", "|");

                int KeyCounter = 0;
                foreach (var HotKeyItem in HotKeys.HotKeysList)
                {
                    // TESTING FORMAT
                    // | \__ Full Output:    CTL + ALT + SHIFT + A   |
                    // | \__ Pane 1 HotKey:  CTL + ALT + SHIFT + B   |

                    // Store name and format key.
                    string KeyName = "Pane " + KeyCounter + " HotKey";
                    string FormatString = "{0,0} {1,0} {2,0} {3,3}";
                    string ValueString = Sender.ModStringExpanded + " + " + HotKeyItem;
                    if (KeyCounter == 0)
                    {
                        KeyName = "Full Output";
                        FormatString = "{0,0} {1,0} {2,23} {3,3}";
                    }

                    // Write out the formatted string.
                    Console.WriteLine(FormatString, "|", $"\\__ {KeyName}: ", ValueString, "|");
                    
                    // Tick key count.
                    KeyCounter++;
                }

                // Print splitter.
                Console.WriteLine("|---------------------------------------------|");

                // Print out pane size info.
                Console.WriteLine("| {0,0} {1,34}", "Pane Sizes", "|");
                int PaneCounter = 0;
                foreach (var PaneSizeItem in PaneSize.PaneSizesList)
                {
                    // TESTING FORMAT
                    // | \__ Pane 1 Size:       Left Edge - 1150px   |
                    // | \__ Pane 2 Size:  1151px - 2230px          |

                    // Store pane name and info.
                    string PaneName = "Pane " + (PaneCounter + 1) + " Size";
                    string FormatString = "{0,0} {1,0} {2,17} {3,9}";

                    // String format and write out.
                    string FormatPxEdge = PaneSizeItem.Item1  + "px";

                    // Check for 0px callout.
                    if (FormatPxEdge == "0px") { FormatPxEdge = "Left Edge"; }
                    if (PaneCounter == 0 || FormatPxEdge.Contains("Edge"))
                        FormatString = "{0,0} {1,5} {2,20} {3,6}";

                    // Store value here.
                    string ValueString = $"{FormatPxEdge} - {PaneSizeItem.Item2}px";

                    // Write out the line here.
                    Console.WriteLine(FormatString, "|", $"\\__ {PaneName}: ", ValueString, "|");

                    // Tick Pane count.
                    PaneCounter++;
                }

                // Command info here.
                Console.WriteLine("|---------------------------------------------|");
                Console.WriteLine("|    =OBS Pane Configuration Setup Helpers=   |");
                Console.WriteLine("|---------------------------------------------|");
                Console.WriteLine("|  Press 'P' and a number to toggle a pane's  |");
                Console.WriteLine("|  outline. Press 'P' twice to see all panes. |");
                Console.WriteLine("|  Or press 'P' and a number for a set pane.  |");
                Console.WriteLine("|                                             |");
                Console.WriteLine("|     These rectangles help visualize the     |");
                Console.WriteLine("|   sizes of panes OBS needs to be setup to   |");
                Console.WriteLine("|  switch between based on your cursor spot.  |");
                Console.WriteLine("|                                             |");
                Console.WriteLine("|  Other Pane Commands                        |");
                Console.WriteLine("|  - Press 'P' then 'C' to clear outlines     |");
                Console.WriteLine("|  - Press 'R' twice to reset pane sizes      |");
                Console.WriteLine("|---------------------------------------------|");
                Console.WriteLine("|          =Main Control Information=         |");
                Console.WriteLine("|---------------------------------------------|");
                Console.WriteLine("|      Press ESCAPE at any time to pause      |");
                Console.WriteLine("|          Press ENTER to continue            |");
                Console.WriteLine("+---------------------------------------------+");
            };

            // Write temp file to console here.
            var StandardConsoleOutput = new StreamWriter(Console.OpenStandardOutput());
            StandardConsoleOutput.AutoFlush = true;
            Console.SetOut(StandardConsoleOutput);

            // Store temp file contents and split into list.
            string[] AllFileLines = File.ReadAllLines(TempFile);
            int EnterCount = (Console.WindowHeight - AllFileLines.Length) / 2;
            for (int Counter = 0; Counter < EnterCount - 5; Counter++) { Console.WriteLine(); }
            foreach (var LineItem in AllFileLines)
                CenterConsolePrint(LineItem);

            // Delete Temp File.
            try { File.Delete(TempFile); }
            catch { }
        }
        /// <summary>
        /// Prints a string to the console using the center print function.
        /// </summary>
        /// <param name="PrintThis"></param>
        public static void CenterConsolePrint(string PrintThis)
        {
            // Printing color styles
            StyleSheet PrintStyleSheet = new StyleSheet(Color.White);
            PrintStyleSheet.AddStyle(@"OBS Switcher Version \d+(.|\s)\d+(.|\s)\d+\s", Color.Lime, match => match.ToString());
            PrintStyleSheet.AddStyle(@"~(\S+\s+)[^~]+~", Color.White, match => match.Replace("~", " ").ToString());
            PrintStyleSheet.AddStyle(@"=(\S+\s+)[^=]+=", Color.Yellow, match => match.Replace("=", " ").ToString());
            PrintStyleSheet.AddStyle(@"OBS HotKeys|Pane Sizes|Other Pane Commands", Color.GreenYellow, match => match.ToString());
            PrintStyleSheet.AddStyle(@"\+|-{2,}|\|", Color.DarkGray, match => match.ToString());
            PrintStyleSheet.AddStyle(@"Full View|Full Output", Color.LightSkyBlue, match => match.ToString());
            PrintStyleSheet.AddStyle(@"Pane \d+", Color.LightSkyBlue, match => match.ToString());
            PrintStyleSheet.AddStyle(@" \+ ", Color.White, match => match.ToString());
            PrintStyleSheet.AddStyle(@"CTL|ALT|SHIFT", Color.Gray, match => match.ToString().Replace("+", String.Empty));
            PrintStyleSheet.AddStyle(@"\+ (\S) ", Color.Aquamarine, match => match.Split('+').LastOrDefault());
            PrintStyleSheet.AddStyle(@"Left Edge", Color.Yellow, match => match.Replace("px", String.Empty));
            PrintStyleSheet.AddStyle(@"\d{4}", Color.Aquamarine, match => match.Replace("px", String.Empty));
            PrintStyleSheet.AddStyle(@"px", Color.White, match => match.ToString());
            PrintStyleSheet.AddStyle(@"\'\S{1}\'", Color.HotPink, match => match.ToString());
            PrintStyleSheet.AddStyle(@"ENTER|ESCAPE", Color.HotPink, match => match.ToString());

            // Print the styled console sheet.
            if (Console.WindowWidth % 2 != 0) Console.SetCursorPosition(((Console.WindowWidth - PrintThis.Length) / 2) + 1, Console.CursorTop);
            else { Console.SetCursorPosition(((Console.WindowWidth - PrintThis.Length) / 2), Console.CursorTop); }
            Console.WriteLineStyled(PrintThis, PrintStyleSheet);
        }
        /// <summary>
        /// Print out what pane is currently open 
        /// </summary>
        /// <param name="PaneIndex"></param>
        private static void PrintCurrentPane(int PaneIndex, MouseCords XAndYPos)
        {
            // Style mouse output.
            StyleSheet RunningOutputStyle = new StyleSheet(Color.White);
            RunningOutputStyle.AddStyle(@"\[|\]|", Color.DarkGray, match => match.ToString());
            RunningOutputStyle.AddStyle(@"X:|Y:", Color.Yellow, match => match.ToString());
            RunningOutputStyle.AddStyle(@"ZERO", Color.Orange, match => match.ToString());
            RunningOutputStyle.AddStyle(@"MAIN", Color.HotPink, match => match.ToString());

            // Write seperator line.
            for (int Count = 0; Count < Console.WindowWidth; Count++) { Console.WriteStyled("-", RunningOutputStyle); }
            Console.Write("");

            // Check for main here
            if (XAndYPos == null || PaneIndex == -1) { Console.WriteStyled("[X: MAIN][Y: MAIN] | ", RunningOutputStyle); }
            else
            {
                // Write mouse cords.
                string xPos = "[X: " + XAndYPos.PosX.ToString("D4") + "]";
                string yPos = "[Y: " + XAndYPos.PosY.ToString("D4") + "]";
                if (XAndYPos.PosY < 0) { yPos = "[Y: ZERO]"; }
                Console.WriteStyled(xPos + yPos + " | ", RunningOutputStyle);
            }

            // Write pane info.
            Console.ForegroundColor = Color.DarkGray;
            Console.Write("[");
            if (PaneIndex >= 0)
            {
                Console.ForegroundColor = PaneSizes.ConsolePaneColors[PaneIndex];
                Console.Write($"SWITCHING TO OBS PANE NUMBER {PaneIndex + 1}");
            }
            if (PaneIndex < 0)
            {
                Console.ForegroundColor = Color.White; 
                Console.Write("SWITCHING TO OBS MAIN PANE");
            }
            Console.ForegroundColor = Color.DarkGray;
            Console.WriteLine("]");

            // Write seperator line.
            for (int Count = 0; Count < Console.WindowWidth; Count++) { Console.WriteStyled("-", RunningOutputStyle); }
            Console.Write("");
        }
    }
}
