using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBSSwitcher
{
    public class OBSSwitcherMain
    {
        public static void Main(string[] args)
        {
            if (!CheckArgsFilled()) { return; }
            while (true)
            {
                RunSwitcher();
                Console.Clear();
            }            
        }

        private static void RunSwitcher()
        {
            // Make a pane size object and a key sending object.
            PaneSizeValues PaneSizes = new PaneSizeValues();
            KeySender Sender = new KeySender();

            // Display output info to the user.
            WriteConfigInfo(PaneSizes, Sender);

            // Store last move so we dont repeat
            int LastMoveIndex = -1;

            // Loop only while we have not key pressed and we can read a key.
            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                // Get new sleep time and wait.
                if (!int.TryParse(ConfigurationManager.AppSettings.Get("DelayTime"), out int DelayTime)) { DelayTime = 1000; }
                System.Threading.Thread.Sleep(DelayTime);

                // Get/Write cords start and then tack on the movement type.
                var XAndYPos = new MouseCords();

                // Y Value is out of normal bounds.
                if (XAndYPos.PosY < 0)
                {
                    if (LastMoveIndex == 0) { continue; }

                    WriteMouseInfo(XAndYPos);

                    Console.Write("[");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("SWITCH TO MAIN EDITOR VIEW");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("]");

                    LastMoveIndex = 0;
                    Sender.SwitchView(0);
                    continue;
                }

                // If Y Value is real and in range here.
                else
                {
                    // Pane 1 switch. 
                    if (XAndYPos.PosX >= PaneSizes.PaneOneValues.Item1 && XAndYPos.PosX <= PaneSizes.PaneOneValues.Item2)
                    {
                        if (LastMoveIndex == 1) { continue; }

                        WriteMouseInfo(XAndYPos);

                        Console.Write("[");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("SWITCH TO EDITOR ONE VIEW");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("]");

                        LastMoveIndex = 1;
                        Sender.SwitchView(1);
                        continue;
                    }

                    // Pane 2 switch. 
                    else if (XAndYPos.PosX >= PaneSizes.PaneTwoValues.Item1 && XAndYPos.PosX <= PaneSizes.PaneTwoValues.Item2)
                    {
                        if (LastMoveIndex == 2) { continue; }

                        WriteMouseInfo(XAndYPos);

                        Console.Write("[");
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write("SWITCH TO EDITOR TWO VIEW");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("]");

                        LastMoveIndex = 2;
                        Sender.SwitchView(2);
                        continue;
                    }

                    // Pane 3 switch. 
                    else if (XAndYPos.PosX >= PaneSizes.PaneThreeValues.Item1 && XAndYPos.PosX <= PaneSizes.PaneThreeValues.Item2)
                    {
                        if (LastMoveIndex == 3) { continue; }

                        WriteMouseInfo(XAndYPos);

                        Console.Write("[");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("SWITCH TO EDITOR THREE VIEW");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("]");

                        LastMoveIndex = 3;
                        Sender.SwitchView(3);
                        continue;
                    }
                }
            }
        }


        private static bool CheckArgsFilled()
        {
            List<string> ParamsToCheck = new List<string>
            {
                "OBSWindowName",

                "MainPaneKey",
                "Pane1HotKey",
                "Pane2HotKey",
                "Pane3HotKey",

                "Pane1SizeValues",
                "Pane2SizeValues",
                "Pane3SizeValues",
            };
            List<string> EmptyValues = new List<string>();

            foreach (var ParamString in ParamsToCheck)
            {
                if (ConfigurationManager.AppSettings.Get(ParamString) != "") { continue; }
                EmptyValues.Add(ParamString);
            }

            if (EmptyValues.Count == 0) { return true; }

            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR WHILE FINDING ALL ARGS");
            Console.BackgroundColor = ConsoleColor.Black;

            Console.WriteLine("\\__ NOT ALL VALUES IN THE APP CONFIG FILE ARE FILLED IN!");
            Console.WriteLine("\\__ VALUES: " + string.Join(", ", EmptyValues));
            return false;
        }
        private static void WriteConfigInfo(PaneSizeValues PaneSize, KeySender Sender)
        {
            // Sep Line is 45 Chars

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("     Configuration Info For This Session     |");
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("{0,0} {1,37}", "Hot Keys", "|");
            Console.WriteLine("{0,-10} {1,5} {2,11}", "\\__ Main:   ", Sender.ModStringExpanded + " + " + Sender.MainPaneKey, "|");
            Console.WriteLine("{0,-10} {1,5} {2,11}", "\\__ Pane 1: ", Sender.ModStringExpanded + " + " + Sender.Pane1HotKey, "|");
            Console.WriteLine("{0,-10} {1,5} {2,11}", "\\__ Pane 2: ", Sender.ModStringExpanded + " + " + Sender.Pane2HotKey, "|");
            Console.WriteLine("{0,-10} {1,5} {2,11}", "\\__ Pane 3: ", Sender.ModStringExpanded + " + " + Sender.Pane3HotKey, "|");
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("{0,0} {1,35}", "Pane Sizes", "|");
            Console.WriteLine("{0,-10} {1,5} {2,26}", "\\__ Pane 1: ", (PaneSize.PaneOneValues.Item1 + "," + PaneSize.PaneOneValues.Item2), "|");
            Console.WriteLine("{0,-10} {1,5} {2,23}", "\\__ Pane 2: ", (PaneSize.PaneTwoValues.Item1 + "," + PaneSize.PaneTwoValues.Item2), "|");
            Console.WriteLine("{0,-10} {1,5} {2,23}", "\\__ Pane 3: ", (PaneSize.PaneThreeValues.Item1 + "," + PaneSize.PaneThreeValues.Item2), "|");
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("      Press ESCAPE at any time to pause      |");
            Console.WriteLine("          Press ENTER to continue            |");
            Console.WriteLine("----------------------------------------------");
            Console.ReadLine();
            Console.Clear();
        }
        private static void WriteMouseInfo(MouseCords XAndYPos)
        {
            string xPos = "[X: " + XAndYPos.PosX.ToString("D4") + "]";
            string yPos = "[Y: " + XAndYPos.PosY.ToString("D4") + "]";
            if (XAndYPos.PosY < 0) { yPos = "[Y: ZERO]"; }
            Console.Write(xPos + yPos + " -- ");
        }
    }
}
