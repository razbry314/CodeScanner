/* Rick Zurek
 * 6/29/2016
 * using USBHIDDRIVER by Florian Leitner-Fischer
 * documentation and download avalible at: http://www.florian-leitner.de/index.php/2007/08/03/hid-usb-driver-library/
 * 
 * This is a simple program to automaticaly open URLs that have been scanned. 
 * Ideally running in the background and and only activating when something is scanned.
 * It connects to the device being used, when a code is scanned it uses a regex statment
 * to check if the code is a url, if it is it openes that in your default browser.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USBHIDDRIVER;
using System.Threading;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace CodeScanner
{
    class Program
    {


        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        static ManualResetEvent _quitEvent = new ManualResetEvent(false);


        static NotifyIcon notifyIcon;
        static IntPtr processHandle;
        static IntPtr WinShell;
        static IntPtr WinDesktop;
        static MenuItem HideMenu;
        static MenuItem RestoreMenu;

        static void Main(string[] args)
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon("TestICON.ico");
            notifyIcon.Text = "Monitor";
            notifyIcon.Visible = true;

            ContextMenu menu = new ContextMenu();
            HideMenu = new MenuItem("Hide", new EventHandler(Minimize_Click));
            RestoreMenu = new MenuItem("Restore", new EventHandler(Maximize_Click));

            menu.MenuItems.Add(RestoreMenu);
            menu.MenuItems.Add(HideMenu);
            menu.MenuItems.Add(new MenuItem("Exit", new EventHandler(CleanExit)));

            notifyIcon.ContextMenu = menu;

            //You need to spin off your actual work in a different thread so that the Notify Icon works correctly
            Task.Factory.StartNew(Run);

            processHandle = Process.GetCurrentProcess().MainWindowHandle;

            WinShell = GetShellWindow();

            WinDesktop = GetDesktopWindow();

            //Hide the Window
            ResizeWindow(false);

            ///This is required for triggering WinForms activity in Console app
            Application.Run();


        }

        static void Run()
        {
            USBHIDDRIVER.USBInterface usb = new USBInterface("vid_11fa", "pid_0202");

            usb.Connect();

            usb.enableUsbBufferEvent(new System.EventHandler(MyEventCacher));

            usb.startRead();

            Console.Write("Press CTRL+C To Quit" + Environment.NewLine);
            Console.CancelKeyPress += (sender, eArgs) =>
            {
                _quitEvent.Set();
                eArgs.Cancel = true;
            };

            _quitEvent.WaitOne();

            usb.stopRead();

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }


        private static void CleanExit(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
            Environment.Exit(1);
        }


        static void Minimize_Click(object sender, EventArgs e)
        {
            ResizeWindow(false);
        }


        static void Maximize_Click(object sender, EventArgs e)
        {
            ResizeWindow();
        }

        static void ResizeWindow(bool Restore = true)
        {
            if (Restore)
            {
                RestoreMenu.Enabled = false;
                HideMenu.Enabled = true;
                SetParent(processHandle, WinDesktop);
            }
            else
            {
                RestoreMenu.Enabled = true;
                HideMenu.Enabled = false;
                SetParent(processHandle, WinShell);
            }
        }

        public static void MyEventCacher(object sender, System.EventArgs e)
        {
            if (USBHIDDRIVER.USBInterface.usbBuffer.Count > 0)
            {
                int counter = 0;

                while ((byte[])USBHIDDRIVER.USBInterface.usbBuffer[counter] == null)
                {
                    lock (USBHIDDRIVER.USBInterface.usbBuffer.SyncRoot)
                    {
                        USBHIDDRIVER.USBInterface.usbBuffer.RemoveAt(0);
                    }
                }

                string result = System.Text.Encoding.ASCII.GetString((byte[])USBHIDDRIVER.USBInterface.usbBuffer[0]);

                lock (USBHIDDRIVER.USBInterface.usbBuffer.SyncRoot)
                {
                    USBHIDDRIVER.USBInterface.usbBuffer.RemoveAt(0);
                }

                bool isUrl;
                string UrlString;

                CheckUrl(result, out isUrl, out UrlString);


                if (isUrl)
                {
                    Process.Start(UrlString);
                }
                else
                {
                    Console.WriteLine("No Url: " + result.Replace("/0", "") + Environment.NewLine);
                }

            }
        }

        public static void CheckUrl(string input, out bool isUrl, out string UrlString)
        {
            Regex UrlRegex = new Regex("(http|ftp|https)://([\\w_-]+(?:(?:\\.[\\w_-]+)+))([\\w.,@?^=%&:/~+#-]*[\\w@?^=%&/~+#-])?");
            Match result = UrlRegex.Match(input);

            if (result.Success)
            {
                Console.WriteLine(result.Value + Environment.NewLine);
                isUrl = true;
                UrlString = (result.Value);
            }
            else
            {
                isUrl = false;
                UrlString = "";
            }

        }
    }
}
