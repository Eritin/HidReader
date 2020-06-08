using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HidReader
{
    class HIDInterface
    {
        public delegate void DelegateStatusConnected(object sender, bool isConnect);
        public DelegateStatusConnected StatusConnected;
        public delegate void DelegateDataReceived(object sender, byte[] data);
        public DelegateDataReceived DataReceived;
        private static HIDInterface m_oInstance;
        protected virtual void RaiseEventDataReceived(byte[] buf)
        {
            if (null != DataReceived) DataReceived(this, buf);
        }
        public void HidDataReceived(object sender, report e)
        {

            try
            {
                //第一个字节为数据长度，因为Device 的HID数据固定长度为64字节，取有效数据
                byte[] buf = new byte[e.reportBuff[0]];
                Array.Copy(e.reportBuff, 1, buf, 0, e.reportBuff[0]);

                //推送数据
                RaiseEventDataReceived(buf);
            }
            catch
            {
                #region 消息通知
                ReusltString result = new ReusltString();
                result.Result = false;
                result.message = "Receive Error";
                RaiseEventConnectedState(result.Result);
                #endregion
            }

        }
        void HidDeviceRemoved(object sender, EventArgs e)
        {
            bConnected = false;
            #region 消息通知
            ReusltString result = new ReusltString();
            result.Result = false;
            result.message = "Device Remove";
            RaiseEventConnectedState(result.Result);
            #endregion
            if (oSp != null)
            {
                oSp.CloseDevice();
            }

        }
        public HIDInterface()
        {
            m_oInstance = this;
            oSp.DataReceived = HidDataReceived;
            oSp.DeviceRemoved = HidDeviceRemoved;
        }
        public void DisConnect()
        {
            bConnected = false;

            Thread.Sleep(200);
            if (oSp != null)
            {
                oSp.CloseDevice();
            }
        }
        public void Dispose()
        {
            try
            {
                this.DisConnect();
                oSp.DataReceived -= HidDataReceived;
                oSp.DeviceRemoved -= HidDeviceRemoved;
                ReadWriteThread.DoWork -= ReadWriteThread_DoWork;
                ReadWriteThread.CancelAsync();
                ReadWriteThread.Dispose();
            }
            catch
            { }
        }
        public struct HidDevice
        {
            public UInt16 vID;
            public UInt16 pID;
            public string serial;
        }
        HidDevice lowHidDevice = new HidDevice();
        Boolean ContinueConnectFlag = true;
        private BackgroundWorker ReadWriteThread = new BackgroundWorker();
        public bool bConnected = false;

        public struct ReusltString
        {
            public bool Result;
            public string message;
        }
        public Hid oSp = new Hid();
        protected virtual void RaiseEventConnectedState(bool isConnect)
        {
            if (null != StatusConnected) StatusConnected(this, isConnect);
        }

        public bool Connect(HidDevice hidDevice)
        {
            ReusltString result = new ReusltString();

            Hid.HID_RETURN hdrtn = oSp.OpenDevice(hidDevice.vID, hidDevice.pID, hidDevice.serial);

            if (hdrtn == Hid.HID_RETURN.SUCCESS)
            {

                bConnected = true;

                #region 消息通知
                result.Result = true;
                result.message = "Connect Success!";
                RaiseEventConnectedState(result.Result);
                #endregion


                return true;
            }

            bConnected = false;

            #region 消息通知
            result.Result = false;
            result.message = "Device Connect Error";
            RaiseEventConnectedState(result.Result);

            #endregion
            return false;
        }

        private void ReadWriteThread_DoWork(object sender, DoWorkEventArgs e)
        {
            while (ContinueConnectFlag)
            {
                try
                {
                    if (!bConnected)
                    {
                        Connect(lowHidDevice);

                    }
                    Thread.Sleep(500);
                }
                catch { }
            }
        }
        public void AutoConnect(HidDevice hidDevice)
        {
            lowHidDevice = hidDevice;
            ContinueConnectFlag = true;
            ReadWriteThread.DoWork += ReadWriteThread_DoWork;
            ReadWriteThread.WorkerSupportsCancellation = true;
            ReadWriteThread.RunWorkerAsync();	//Recommend performing USB read/write operations in a separate thread.  Otherwise,
        }
    }
}
