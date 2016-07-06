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

namespace CodeScanner
{
    class Program
    {
        public const 
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);

        public static void MyEventCacher(object sender, System.EventArgs e)
        {
            Console.Write("Event Caught" + Environment.NewLine);
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



            }
        }


        static void Main(string[] args)
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
        }



    }
}
