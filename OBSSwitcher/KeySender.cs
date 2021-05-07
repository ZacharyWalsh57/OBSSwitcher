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
        private string ServiceAppType;

        // OBS Pointer
        private IntPtr OBSHandle = IntPtr.Zero;

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
            ProcName = ConfigurationManager.AppSettings.Get("OBSWindowName");
            ServiceAppType = ConfigurationManager.AppSettings.Get("StreamServiceSW");

            // Check for null.
            if (string.IsNullOrEmpty(ServiceAppType)) { throw new Exception("PLEASE SPECIFY SERVICE APP TYPE (OBS, XPLIT, ETC)"); }
            if (string.IsNullOrEmpty(ProcName)) { throw new Exception("PLEASE SPECIFY THE OBS WINDOW NAME AND RESTART THIS APP"); }

            // Convert the list of ConsoleKey items into string items.
            foreach (var KeyItem in HotKeys.HotKeysList)
                KeyStrings.Add(KeyItem.ToString());
        }

        /// <summary>
        /// Forces the program to the front of the UI.
        /// </summary>
        /// <param name="NameOfWindow">Name of window to pull</param>
        private void BringToFront()
        {
            try
            {
                // Get Process list first if needed.
                Process[] OBSProcessList = Process.GetProcesses().Where(ProcObj =>
                    ProcObj.ProcessName.ToUpper().Contains(ServiceAppType)).ToArray();

                // Check for processes found or use app config value.
                if (OBSProcessList.Length != 0)
                {
                    ProcName = ServiceAppType;
                    OBSHandle = OBSProcessList.FirstOrDefault().MainWindowHandle;
                }
                else
                {
                    // Go for config value.
                    OBSHandle = FindWindow("Qt5152QWindowIcon", ProcName);
                    if (OBSHandle == IntPtr.Zero) { throw new Exception("PLEASE ENSURE THE OBS WINDOW NAME IS SET CORRECTLY!"); }
                }

                // Set foreground window.
                SetForegroundWindow(OBSHandle);
            }
            catch
            {
                if (OBSHandle != IntPtr.Zero) { SetForegroundWindow(OBSHandle); }
                else { throw new Exception("FAILED TO FIND OBS WINDOW!"); }
            }
        }
        /// <summary>
        /// Change to the selected view in OBS using the hotkey for it.
        /// </summary>
        /// <param name="ViewIndex"></param>
        public void SwitchView(int ViewIndex)
        {
            // Do this if not in front window.
            if (!Debugger.IsAttached) { BringToFront(); }

            // Set main view or not. -1 means show full display output.
            if (ViewIndex == -1) { ViewIndex = 0; }

            // Store hotkey string. first.
            string KeyToSend = ModString + KeyStrings[ViewIndex];

            // Return if the Debugger is on for this app
            if (!Debugger.IsAttached)
            {
                // Focus OBS and send the HotKey here.
                SendKeys.SendWait(KeyToSend);
                System.Threading.Thread.Sleep(50);
                SendKeys.SendWait(KeyToSend);
            }
        }
    }
}
