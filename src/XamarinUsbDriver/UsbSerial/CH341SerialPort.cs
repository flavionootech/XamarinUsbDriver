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
using System.Threading.Tasks;

namespace XamarinUsbDriver.UsbSerial
{
    public class CH341SerialPort : CommonUsbSerialPort
    {
        private static int REQTYPE_HOST_FROM_DEVICE = UsbConstants.UsbTypeVendor | 128;
        private static int REQTYPE_HOST_TO_DEVICE = 0x40;
        private static int USB_TIMEOUT = 5000;
        private static int CH34X_300_1312 = 0xd980;
        private static int CH34X_300_0f2c = 0xeb;

        private static int CH34X_600_1312 = 0x6481;
        private static int CH34X_600_0f2c = 0x76;

        private static int CH34X_1200_1312 = 0xb281;
        private static int CH34X_1200_0f2c = 0x3b;

        private static int CH34X_2400_1312 = 0xd981;
        private static int CH34X_2400_0f2c = 0x1e;

        private static int CH34X_4800_1312 = 0x6482;
        private static int CH34X_4800_0f2c = 0x0f;

        private static int CH34X_9600_1312 = 0xb282;
        private static int CH34X_9600_0f2c = 0x08;

        private static int CH34X_19200_1312 = 0xd982;
        private static int CH34X_19200_0f2c_rest = 0x07;

        private static int CH34X_38400_1312 = 0x6483;

        private static int CH34X_57600_1312 = 0x9883;

        private static int CH34X_115200_1312 = 0xcc83;

        private static int CH34X_230400_1312 = 0xe683;

        private static int CH34X_460800_1312 = 0xf383;

        private static int CH34X_921600_1312 = 0xf387;

        private static int CH34X_1228800_1312 = 0xfb03;
        private static int CH34X_1228800_0f2c = 0x21;

        private static int CH34X_2000000_1312 = 0xfd03;
        private static int CH34X_2000000_0f2c = 0x02;

        private static int CH341_REQ_WRITE_REG = 0x9A;
        private static int CH341_REQ_READ_REG = 0x95;
        private static int CH341_REG_BREAK1 = 0x05;
        private static int CH341_REG_BREAK2 = 0x18;
        private static int CH341_NBREAK_BITS_REG1 = 0x01;
        private static int CH341_NBREAK_BITS_REG2 = 0x40;

        private static int CH34X_PARITY_NONE = 0xc3;
        private static int CH34X_PARITY_ODD = 0xcb;
        private static int CH34X_PARITY_EVEN = 0xdb;
        private static int CH34X_PARITY_MARK = 0xeb;
        private static int CH34X_PARITY_SPACE = 0xfb;

        private const int LCR_ENABLE_RX = 0x80;
        private const int LCR_ENABLE_TX = 0x40;

        private static int LCR_CS8 = 0x03;
        private static int LCR_CS7 = 0x02;
        private static int LCR_CS6 = 0x01;
        private static int LCR_CS5 = 0x00;

        private static int LCR_MARK_SPACE = 0x20;
        private static int LCR_PAR_EVEN = 0x10;
        private static int LCR_ENABLE_PAR = 0x08;

        private static int LCR_STOP_BITS_2 = 0x04;

        private static int DEFAULT_BAUD_RATE = 9600;
        public override bool Cd { get; }
        public override bool Cts { get; set; }
        public override bool Dsr { get; }
        public override bool Dtr { get; set; }
        public override bool Ri { get; }
        public override bool Rts { get; set; }
        private UsbEndpoint _readEndpoint;
        private UsbEndpoint _writeEndpoint;
        public override event EventHandler<bool> CtsChanged;

        public CH341SerialPort(UsbDevice device, int interfaceNumber) : base(device, interfaceNumber)
        {

        }

        public override void Close()
        {
            try
            {
                Connection.ReleaseInterface(Device.GetInterface(0));
            }
            finally
            {
                Connection = null;
            }
        }

        public override void Open(UsbDeviceConnection connection)
        {
            Connection = connection;

            var ccontrolInterface = Device.GetInterface(0);
            if (!connection.ClaimInterface(ccontrolInterface, true))
            {
                throw new System.Exception("Could not claim control interface.");
            }

            for (int i = 0; i < ccontrolInterface.EndpointCount; i++)
            {
                var ep = ccontrolInterface.GetEndpoint(i);
                if (ep.Type == UsbAddressing.XferBulk)
                {
                    if (ep.Direction == UsbAddressing.In)
                    {
                        _readEndpoint = ep;
                    }
                    else if (ep.Direction == UsbAddressing.Out)
                    {

                        _writeEndpoint = ep;
                    }
                }
            }

            Init();
            SetBaudRate(DEFAULT_BAUD_RATE);
        }

        private int Init()
        {
            if (SetControlCommandOut(0xa1, 0xc29c, 0xb2b9, null) < 0)
            {
                return -1;
            }

            if (SetControlCommandOut(0xa4, 0xdf, 0, null) < 0)
            {
                return -1;
            }

            if (SetControlCommandOut(0xa4, 0x9f, 0, null) < 0)
            {
                return -1;
            }

            if (CheckState("init #4", 0x95, 0x0706, new int[] { 0x9f, 0xee }) == -1)
                return -1;

            if (SetControlCommandOut(0x9a, 0x2727, 0x0000, null) < 0)
            {

                return -1;
            }

            if (SetControlCommandOut(0x9a, 0x1312, 0xb282, null) < 0)
            {
                return -1;
            }

            if (SetControlCommandOut(0x9a, 0x0f2c, 0x0008, null) < 0)
            {
                return -1;
            }

            if (SetControlCommandOut(0x9a, 0x2518, 0x00c3, null) < 0)
            {
                return -1;
            }

            if (CheckState("init #9", 0x95, 0x0706, new int[] { 0x9f, 0xee }) == -1)
                return -1;

            if (SetControlCommandOut(0x9a, 0x2727, 0x0000, null) < 0)
            {
                return -1;
            }

            return 1;
        }

        public bool SetBaudRate(int baudRate)
        {
            if (baudRate <= 300)
            {
                int ret = SetBaudRate(CH34X_300_1312, CH34X_300_0f2c); //300
                if (ret == -1)
                    return false;
            }
            else if (baudRate > 300 && baudRate <= 600)
            {
                int ret = SetBaudRate(CH34X_600_1312, CH34X_600_0f2c); //600
                if (ret == -1)
                    return false;

            }
            else if (baudRate > 600 && baudRate <= 1200)
            {
                int ret = SetBaudRate(CH34X_1200_1312, CH34X_1200_0f2c); //1200
                if (ret == -1)
                    return false;
            }
            else if (baudRate > 1200 && baudRate <= 2400)
            {
                int ret = SetBaudRate(CH34X_2400_1312, CH34X_2400_0f2c); //2400
                if (ret == -1)
                    return false;
            }
            else if (baudRate > 2400 && baudRate <= 4800)
            {
                int ret = SetBaudRate(CH34X_4800_1312, CH34X_4800_0f2c); //4800
                if (ret == -1)
                    return false;
            }
            else if (baudRate > 4800 && baudRate <= 9600)
            {
                int ret = SetBaudRate(CH34X_9600_1312, CH34X_9600_0f2c); //9600
                if (ret == -1)
                    return false;
            }
            else if (baudRate > 9600 && baudRate <= 19200)
            {
                int ret = SetBaudRate(CH34X_19200_1312, CH34X_19200_0f2c_rest); //19200
                if (ret == -1)
                    return false;
            }
            else if (baudRate > 19200 && baudRate <= 38400)
            {
                int ret = SetBaudRate(CH34X_38400_1312, CH34X_19200_0f2c_rest); //38400
                if (ret == -1)
                    return false;
            }
            else if (baudRate > 38400 && baudRate <= 57600)
            {
                int ret = SetBaudRate(CH34X_57600_1312, CH34X_19200_0f2c_rest); //57600
                if (ret == -1)
                    return false;
            }
            else if (baudRate > 57600 && baudRate <= 115200) //115200
            {
                int ret = SetBaudRate(CH34X_115200_1312, CH34X_19200_0f2c_rest);
                if (ret == -1)
                    return false;
            }
            else if (baudRate > 115200 && baudRate <= 230400) //230400
            {
                int ret = SetBaudRate(CH34X_230400_1312, CH34X_19200_0f2c_rest);
                if (ret == -1)
                    return false;
            }
            else if (baudRate > 230400 && baudRate <= 460800) //460800
            {
                int ret = SetBaudRate(CH34X_460800_1312, CH34X_19200_0f2c_rest);
                if (ret == -1)
                    return false;
            }
            else if (baudRate > 460800 && baudRate <= 921600)
            {
                int ret = SetBaudRate(CH34X_921600_1312, CH34X_19200_0f2c_rest);
                if (ret == -1)
                    return false;
            }
            else if (baudRate > 921600 && baudRate <= 1228800)
            {
                int ret = SetBaudRate(CH34X_1228800_1312, CH34X_1228800_0f2c);
                if (ret == -1)
                    return false;
            }
            else if (baudRate > 1228800 && baudRate <= 2000000)
            {
                int ret = SetBaudRate(CH34X_2000000_1312, CH34X_2000000_0f2c);
                if (ret == -1)
                    return false;
            }

            return true;
        }

        private int SetBaudRate(int index1312, int index0f2c)
        {
            if (SetControlCommandOut(CH341_REQ_WRITE_REG, 0x1312, index1312, null) < 0)
                return -1;
            if (SetControlCommandOut(CH341_REQ_WRITE_REG, 0x0f2c, index0f2c, null) < 0)
                return -1;
            if (CheckState("set_baud_rate", 0x95, 0x0706, new int[] { 0x9f, 0xee }) == -1)
                return -1;
            if (SetControlCommandOut(CH341_REQ_WRITE_REG, 0x2727, 0, null) < 0)
                return -1;
            return 0;
        }

        private int CheckState(string msg, int request, int value, int[] expected)
        {
            byte[] buffer = new byte[expected.Length];
            int ret = SetControlCommandIn(request, value, 0, buffer);

            if (ret != expected.Length)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        private int SetControlCommandIn(int request, int value, int index, byte[] data)
        {
            int dataLength = 0;
            if (data != null)
            {
                dataLength = data.Length;
            }

            return Connection.ControlTransfer((UsbAddressing)REQTYPE_HOST_FROM_DEVICE, request, value, index, data, dataLength, USB_TIMEOUT);
        }

        private int SetControlCommandOut(int request, int value, int index, byte[] data)
        {
            int dataLength = 0;
            if (data != null)
            {
                dataLength = data.Length;
            }

            return Connection.ControlTransfer((UsbAddressing)REQTYPE_HOST_TO_DEVICE, request, value, index, data, dataLength, USB_TIMEOUT);
        }

        public override int Read(byte[] dest, int timeoutMillis)
        {
            lock (ReadBufferLock)
            {
                int readAmt = Math.Min(dest.Length, ReadBuffer.Length);
                int numBytesRead = Connection.BulkTransfer(_readEndpoint, ReadBuffer, readAmt, timeoutMillis);
                if (numBytesRead < 0)
                {
                    return 0;
                }

                Buffer.BlockCopy(ReadBuffer, 0, dest, 0, numBytesRead);

                return numBytesRead;
            }
        }

        public override Task<int> ReadAsync(byte[] dest, int timeoutMillis)
        {
            throw new NotImplementedException();
        }

        public override void SetParameters(int baudRate, DataBits dataBits, StopBits stopBits, Parity parity)
        {
            SetBaudRate(baudRate);

            int lcr = LCR_ENABLE_RX | LCR_ENABLE_TX;

            lcr |= dataBits switch
            {
                DataBits._5 => LCR_CS5,
                DataBits._6 => LCR_CS6,
                DataBits._7 => LCR_CS7,
                DataBits._8 => LCR_CS8,
                _ => throw new Java.Lang.IllegalArgumentException("Invalid data bits: " + dataBits),
            };


            lcr |= parity switch
            {
                Parity.None => lcr,
                Parity.Odd => LCR_ENABLE_PAR,
                Parity.Even => LCR_ENABLE_PAR | LCR_PAR_EVEN,
                Parity.Mark => LCR_ENABLE_PAR | LCR_MARK_SPACE,
                Parity.Space => LCR_ENABLE_PAR | LCR_MARK_SPACE | LCR_PAR_EVEN,
                _ => throw new Java.Lang.IllegalArgumentException("Invalid parity: " + parity),
            };

            lcr |= stopBits switch
            {
                StopBits._1 => lcr,
                StopBits._1_5 => throw new Java.Lang.UnsupportedOperationException("Unsupported stop bits: 1.5"),
                StopBits._2 => LCR_STOP_BITS_2,
                _ => throw new Java.Lang.IllegalArgumentException("Invalid stop bits: " + stopBits)
            };

            var ret = SetControlCommandOut(0x9a, 0x2518, lcr, null);
            if (ret < 0)
            {
                throw new Exception("Error setting control byte");
            }
        }        

        public override int Write(byte[] src, int timeoutMillis)
        {
            int amtWritten;

            lock (WriteBufferLock)
            {
                amtWritten = Connection.BulkTransfer(_writeEndpoint, src, src.Length, timeoutMillis);
            }

            return amtWritten;
        }

        public override Task<int> WriteAsync(byte[] src, int timeoutMillis)
        {
            throw new NotImplementedException();
        }
    }
}