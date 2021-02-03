using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;

namespace OBSSwitcher
{
    public class KeySender
    {
        private string ModString = "+^%";
        public string ModStringExpanded = "CTL + ALT + SHIFT";

        public string MainPaneKey { get; set; }
        public string Pane1HotKey { get; set; }
        public string Pane2HotKey { get; set; }
        public string Pane3HotKey{ get; set; }

        [DllImport("USER32.DLL")]
        static extern int SetForegroundWindow(IntPtr point);

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);

        public KeySender()
        {
            MainPaneKey = ConfigurationManager.AppSettings.Get("MainPaneKey");
            Pane1HotKey = ConfigurationManager.AppSettings.Get("Pane1HotKey");
            Pane2HotKey = ConfigurationManager.AppSettings.Get("Pane2HotKey");
            Pane3HotKey = ConfigurationManager.AppSettings.Get("Pane3HotKey");
        }

        private void BringToFront(string NameOfWindow)
        {
            var OBSHandle = FindWindow("Qt5152QWindowIcon", NameOfWindow);
            if (OBSHandle == IntPtr.Zero) { return; }

            SetForegroundWindow(OBSHandle);
        }

        public void SwitchView(int ViewIndex)
        {
            string ProcName = "OBS 26.1.1 (64-bit, windows) - Profile: Untitled - Scenes: Untitled";
            switch (ViewIndex)
            {
                case (0):
                    BringToFront(ProcName);

                    SendKeys.SendWait(ModString + MainPaneKey);
                    System.Threading.Thread.Sleep(50);
                    SendKeys.SendWait(ModString + MainPaneKey);

                    break;

                case (1):
                    BringToFront(ProcName);

                    SendKeys.SendWait(ModString + Pane1HotKey);
                    System.Threading.Thread.Sleep(50);
                    SendKeys.SendWait(ModString + Pane1HotKey);

                    break;

                case (2):
                    BringToFront(ProcName);

                    SendKeys.SendWait(ModString + Pane2HotKey);
                    System.Threading.Thread.Sleep(50);
                    SendKeys.SendWait(ModString + Pane2HotKey);

                    break;

                case (3):
                    BringToFront(ProcName);

                    SendKeys.SendWait(ModString + Pane3HotKey);
                    System.Threading.Thread.Sleep(50);
                    SendKeys.SendWait(ModString + Pane3HotKey);

                    break;
            }
        }
    }
}
