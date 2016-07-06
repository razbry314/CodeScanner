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
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);

        public static void MyEventCacher(object sender, System.EventArgs e)
        {
            Console.Write("Event Caught");
        }


        static void Main(string[] args)
        {
            USBHIDDRIVER.USBInterface usb = new USBInterface("vid_11fa", "pid_0202");

            usb.Connect();

            usb.enableUsbBufferEvent(new System.EventHandler(MyEventCacher));

            usb.startRead();

            Console.Write("Press CTRL+C To Quit");

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
