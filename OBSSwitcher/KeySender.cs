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
        // Hotkeys and name of OBS window.
        private PaneHotKeys HotKeys;
        private List<string> KeyStrings;
        private string ProcName;

        // Key builder strings.
        private string ModString = "+^%";
        public string ModStringExpanded = "CTL + ALT + SHIFT";

        [DllImport("USER32.DLL")]
        static extern int SetForegroundWindow(IntPtr point);

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);

        public KeySender(PaneHotKeys KeyItems)
        {
            // Store the hotkeys.
            HotKeys = KeyItems;
            KeyStrings = new List<string>();

            // Name of OBS Window.
            string ProcName = ConfigurationManager.AppSettings.Get("OBSWindowName");
            if (string.IsNullOrEmpty(ProcName)) { throw new Exception("PLEASE SPECIFY THE OBS WINDOW NAME AND RESTART THIS APP"); }

            // Convert the list of ConsoleKey items into string items.
            foreach (var KeyItem in HotKeys.HotKeysList)
                KeyStrings.Add(KeyItem.ToString());
        }

        /// <summary>
        /// Forces the program to the front of the UI.
        /// </summary>
        /// <param name="NameOfWindow">Name of window to pull</param>
        private void BringToFront(string NameOfWindow)
        {
            var OBSHandle = FindWindow("Qt5152QWindowIcon", NameOfWindow);
            if (OBSHandle == IntPtr.Zero) { return; }

            SetForegroundWindow(OBSHandle);
        }
        /// <summary>
        /// Change to the selected view in OBS using the hotkey for it.
        /// </summary>
        /// <param name="ViewIndex"></param>
        public void SwitchView(int ViewIndex)
        {
            // Focus OBS
            BringToFront(ProcName);

            // Send they HotKey here.
            SendKeys.SendWait(ModString + KeyStrings[ViewIndex]);
            System.Threading.Thread.Sleep(50);
            SendKeys.SendWait(ModString + KeyStrings[ViewIndex]);
        }
    }
}
