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

        public int SendAttempts = 2;      // Number of times to try and send the command 
        public int SendDelayTime = 50;    // The time to wait between each send.

        public bool ForceFocusOBS = false;  // Force OBS to focus each time we loop the send command.

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

            // Find the refocus value.
            bool.TryParse(ConfigurationManager.AppSettings.Get("ForceRefocus").ToLower(), out ForceFocusOBS);

            // Check for null.
            if (string.IsNullOrEmpty(ServiceAppType)) { throw new Exception("PLEASE SPECIFY SERVICE APP TYPE (OBS, XPLIT, ETC)"); }
            if (string.IsNullOrEmpty(ProcName)) { throw new Exception("PLEASE SPECIFY THE OBS WINDOW NAME AND RESTART THIS APP"); }

            // Pull in the SendAttempts and SendDelayTime
            if (!int.TryParse(ConfigurationManager.AppSettings.Get("KeySendCount"), out SendAttempts))
                SendAttempts = 2;   // Default send count.

            if (!int.TryParse(ConfigurationManager.AppSettings.Get("SendDelayValue"), out SendDelayTime))
                SendDelayTime = 50; // Default delay time.

            // Make sure we don't have some absurd value here.
            uint CheckNegativeSendAttempts = (uint)SendAttempts;
            uint CheckNegativeSendDelayTime = (uint)SendDelayTime;

            // Negative value check.
            SendAttempts = (int)CheckNegativeSendAttempts;
            SendDelayTime = (int)CheckNegativeSendDelayTime;

            // Gigantic number check
            if (SendAttempts >= 5) { SendAttempts = 5; }
            if (SendDelayTime >= 500) { SendAttempts = 500; }

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
                // 5/13. Fixed a bug where the switcher app was being considered obs.
                Process[] OBSProcessList = Process.GetProcesses().Where(ProcObj =>
                    ProcObj.ProcessName.ToUpper().Contains(ServiceAppType) &&
                    !ProcObj.ProcessName.ToUpper().Contains("OBSSWITCHER"))
                .ToArray();

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
            // Return if the Debugger is on for this app
            if (Debugger.IsAttached) { return; }

            // Build sending string.   
            if (ViewIndex == -1) { ViewIndex = 0; }
            string KeyToSend = ModString + KeyStrings[ViewIndex];

            // Run Loop to keep sending while needed.
            BringToFront();
            for (int SendCounter = 0; SendCounter < SendAttempts; SendCounter++)
            {
                // Refocus if the force bool is set to true.
                // Default is false.
                if (ForceFocusOBS) BringToFront();

                // Send the HotKey here.
                SendKeys.SendWait(KeyToSend);
            
                // Wait if not on last loop.
                if (SendCounter == SendAttempts - 1) { break; }
                System.Threading.Thread.Sleep(SendDelayTime);
            }
        }
    }
}
