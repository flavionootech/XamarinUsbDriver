using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XamarinUsbDriver.UsbSerial
{
    public class CH341SerialDriver : IUsbSerialDriver
    {
        public UsbDevice Device { get; }
        public List<IUsbSerialPort> Ports { get; }

        public CH341SerialDriver(UsbDevice device)
        {
            Device = device;
            Ports = new List<IUsbSerialPort> { new CH341SerialPort(device, 0) };
        }

        public static Dictionary<int, int[]> GetSupportedDevices()
        {
            return new Dictionary<int, int[]>
            {
                {
                    UsbId.VendorQinheng, new[]
                    {
                        UsbId.QinhengHl340
                    }
                }
            };
        }
    }
}