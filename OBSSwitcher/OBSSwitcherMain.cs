using System;
using System.Collections.Generic;
using System.Configuration;
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

        // Update time value.
        public static int DelayTime;

        public static void Main(string[] args)
        {
            // Set console size. 
            Console.SetWindowSize(65, 60);

            // Setup Pane Sizes and Hotkeys here.
            PaneSizes = new PaneSizeValues();
            HotKeys = new PaneHotKeys(PaneSizes);
            Sender = new KeySender(HotKeys);

            // Get DelayTime
            if (!int.TryParse(ConfigurationManager.AppSettings.Get("DelayTime"), out DelayTime))
                DelayTime = 1000;

            // Run switcher here.
            while (true)
            {
                // Start program.
                RunSwitcher();

                // Store mouse cords here.
                var XAndYPos = new MouseCords();
                WriteMouseInfo(XAndYPos);

                // Move to pane 1 to start.
                PrintCurrentPane(0);

                // Store keysender item and process key.
                Sender.SwitchView(0);
                Console.Clear();
            }            
        }

        /// <summary>
        /// Kicks off the pane switching loop.
        /// </summary>
        private static void RunSwitcher()
        {
            // Display output info to the user.
            WriteConfigInfo(PaneSizes, Sender);

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
                    if (LastMoveIndex != 0) { Sender.SwitchView(0); }
                    continue;
                }

                // Find the X Pane item range.
                for (int PaneIndex = 0; PaneIndex < PaneSizes.PaneSizesList.Count; PaneIndex++)
                {
                    // Get the top and bottom range items.
                    int MinRange = PaneSizes.PaneSizesList[PaneIndex].Item1;
                    int MaxRange = PaneSizes.PaneSizesList[PaneIndex].Item2;

                    // Check if we're in range and need to move.
                    if (!Enumerable.Range(MinRange, MaxRange).Contains(XAndYPos.PosX)) { continue; }
                    if (PaneIndex == LastMoveIndex) { break; }

                    // Store the index of the pane we are on and print that info out.
                    PrintCurrentPane(PaneIndex);
                    Sender.SwitchView(PaneIndex);
                    LastMoveIndex = PaneIndex;
                }
            }
        }


        /// <summary>
        /// Write out the current config for the application.
        /// </summary>
        /// <param name="PaneSize">Sizes object</param>
        /// <param name="Sender">Key sender</param>
        private static void WriteConfigInfo(PaneSizeValues PaneSize, KeySender Sender)
        {
            // Store a temp file.
            string TempFile = Path.GetTempFileName();
            using (var ConsoleWriter = new StreamWriter(TempFile))
            {
                // Set Console Output here.
                Console.SetOut(ConsoleWriter);

                // Title info here.
                Console.Clear();
                Console.WriteLine("+---------------------------------------------+");
                Console.WriteLine("|                                             |");
                Console.WriteLine("|          OBS Switcher Version 1.2.1         |");
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
                    if (PaneCounter == 0) { FormatString = "{0,0} {1,5} {2,20} {3,6}"; }

                    // String format and write out.
                    string FormatPxEdge = PaneSizeItem.Item1  + "px";
                    if (FormatPxEdge == "0px") { FormatPxEdge = "Left Edge"; }
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
                Console.WriteLine("|  Press 'P' then 'C' to clear pane outlines  |");
                Console.WriteLine("|---------------------------------------------|");
                Console.WriteLine("|        =Hot Key Control Information=        |");
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

            // Check for next input key here.
            while (true)
            {
                // If no new key wait.
                if (!Console.KeyAvailable) { continue; }
                var NextKey = Console.ReadKey(true);

                if (NextKey.Key == ConsoleKey.Enter) { Console.Clear(); return; }   // Start app
                if (NextKey.Key == ConsoleKey.P) { HotKeys.ProcessPaneKey(); }      // Draw bounding boxes.
            }
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
            PrintStyleSheet.AddStyle(@"OBS HotKeys|Pane Sizes", Color.GreenYellow, match => match.ToString());
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
            Console.SetCursorPosition(((Console.WindowWidth - PrintThis.Length) / 2) + 1, Console.CursorTop);
            Console.WriteLineStyled(PrintThis, PrintStyleSheet);
        }
        /// <summary>
        /// Print out he current mouse location
        /// </summary>
        /// <param name="XAndYPos">X and Y cords object of the current mouse spot.</param>
        private static void WriteMouseInfo(MouseCords XAndYPos)
        {
            string xPos = "[X: " + XAndYPos.PosX.ToString("D4") + "]";
            string yPos = "[Y: " + XAndYPos.PosY.ToString("D4") + "]";
            if (XAndYPos.PosY < 0) { yPos = "[Y: ZERO]"; }
            Console.Write(xPos + yPos + " -- ");
        }
        /// <summary>
        /// Print out what pane is currently open 
        /// </summary>
        /// <param name="PaneIndex"></param>
        private static void PrintCurrentPane(int PaneIndex)
        {
            Console.ForegroundColor = Color.DarkGray;
            Console.Write("[");
            Console.ForegroundColor = PaneSizes.ConsolePaneColors[PaneIndex];
            if (PaneIndex == 0) { Console.Write("SWITCH TO MAIN PANE VIEW"); }
            if (PaneIndex != 0) { Console.Write($"SWITCH TO PANE #{PaneIndex} VIEW"); }
            Console.ForegroundColor = Color.DarkGray;
            Console.WriteLine("]");
        }
    }
}
