using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HidReader
{
    public class report : EventArgs
    {
        public readonly byte reportID;
        public readonly byte[] reportBuff;
        public report(byte id, byte[] arrayBuff)
        {
            reportID = id;
            reportBuff = arrayBuff;
        }
    }
    class Hid : object
    {
        public enum HID_RETURN
        {
            SUCCESS = 0,
            NO_DEVICE_CONECTED,
            DEVICE_NOT_FIND,
            DEVICE_OPENED,
            WRITE_FAILD,
            READ_FAILD

        }
        public enum DIGCF
        {
            DIGCF_DEFAULT = 0x00000001, // only valid with DIGCF_DEVICEINTERFACE                 
            DIGCF_PRESENT = 0x00000002,
            DIGCF_ALLCLASSES = 0x00000004,
            DIGCF_PROFILE = 0x00000008,
            DIGCF_DEVICEINTERFACE = 0x00000010
        }
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid interfaceClassGuid;
            public int flags;
            public int reserved;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            internal int cbSize;
            internal short devicePath;
        }
        [StructLayout(LayoutKind.Sequential)]
        public class SP_DEVINFO_DATA
        {
            public int cbSize = Marshal.SizeOf(typeof(SP_DEVINFO_DATA));
            public Guid classGuid = Guid.Empty; // temp
            public int devInst = 0; // dumy
            public int reserved = 0;
        }
        private bool deviceOpened = false;
        private const int MAX_USB_DEVICES = 64;
        [DllImport("hid.dll")]
        private static extern void HidD_GetHidGuid(ref Guid HidGuid);
        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, uint Enumerator, IntPtr HwndParent, DIGCF Flags);
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern Boolean SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr deviceInfoSet, IntPtr deviceInfoData, ref Guid interfaceClassGuid, UInt32 memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData, int deviceInterfaceDetailDataSize, ref int requiredSize, SP_DEVINFO_DATA deviceInfoData);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(string fileName, uint desiredAccess, uint shareMode, uint securityAttributes, uint creationDisposition, uint flagsAndAttributes, uint templateFile);
        [DllImport("hid.dll")]
        private static extern Boolean HidD_GetAttributes(IntPtr hidDeviceObject, out HIDD_ATTRIBUTES attributes);
        [DllImport("hid.dll")]
        private static extern Boolean HidD_GetSerialNumberString(IntPtr hidDeviceObject, IntPtr buffer, int bufferLength);
        [DllImport("hid.dll")]
        private static extern Boolean HidD_GetPreparsedData(IntPtr hidDeviceObject, out IntPtr PreparsedData);
        [DllImport("hid.dll")]
        private static extern uint HidP_GetCaps(IntPtr PreparsedData, out HIDP_CAPS Capabilities);
        [DllImport("hid.dll")]
        private static extern Boolean HidD_FreePreparsedData(IntPtr PreparsedData);

        public static void GetHidDeviceList(ref List<string> deviceList)
        {
            Guid hUSB = Guid.Empty;
            uint index = 0;

            deviceList.Clear();
            // 取得hid设备全局id
            HidD_GetHidGuid(ref hUSB);
            //取得一个包含所有HID接口信息集合的句柄
            IntPtr hidInfoSet = SetupDiGetClassDevs(ref hUSB, 0, IntPtr.Zero, DIGCF.DIGCF_PRESENT | DIGCF.DIGCF_DEVICEINTERFACE);
            if (hidInfoSet != IntPtr.Zero)
            {
                SP_DEVICE_INTERFACE_DATA interfaceInfo = new SP_DEVICE_INTERFACE_DATA();
                interfaceInfo.cbSize = Marshal.SizeOf(interfaceInfo);
                //查询集合中每一个接口
                for (index = 0; index < MAX_USB_DEVICES; index++)
                {
                    //得到第index个接口信息
                    if (SetupDiEnumDeviceInterfaces(hidInfoSet, IntPtr.Zero, ref hUSB, index, ref interfaceInfo))
                    {
                        int buffsize = 0;
                        // 取得接口详细信息:第一次读取错误,但可以取得信息缓冲区的大小
                        SetupDiGetDeviceInterfaceDetail(hidInfoSet, ref interfaceInfo, IntPtr.Zero, buffsize, ref buffsize, null);
                        //构建接收缓冲
                        IntPtr pDetail = Marshal.AllocHGlobal(buffsize);
                        SP_DEVICE_INTERFACE_DETAIL_DATA detail = new SP_DEVICE_INTERFACE_DETAIL_DATA();
                        detail.cbSize = Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DETAIL_DATA));
                        Marshal.StructureToPtr(detail, pDetail, false);
                        if (SetupDiGetDeviceInterfaceDetail(hidInfoSet, ref interfaceInfo, pDetail, buffsize, ref buffsize, null))
                        {
                            deviceList.Add(Marshal.PtrToStringAuto((IntPtr)((int)pDetail + 4)));
                        }
                        Marshal.FreeHGlobal(pDetail);
                    }
                }
            }
            SetupDiDestroyDeviceInfoList(hidInfoSet);
            //return deviceList.ToArray();
        }
        static class DESIREDACCESS
        {
            public const uint GENERIC_READ = 0x80000000;
            public const uint GENERIC_WRITE = 0x40000000;
            public const uint GENERIC_EXECUTE = 0x20000000;
            public const uint GENERIC_ALL = 0x10000000;
        }
        static class CREATIONDISPOSITION
        {
            public const uint CREATE_NEW = 1;
            public const uint CREATE_ALWAYS = 2;
            public const uint OPEN_EXISTING = 3;
            public const uint OPEN_ALWAYS = 4;
            public const uint TRUNCATE_EXISTING = 5;
        }
        static class FLAGSANDATTRIBUTES
        {
            public const uint FILE_FLAG_WRITE_THROUGH = 0x80000000;
            public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
            public const uint FILE_FLAG_NO_BUFFERING = 0x20000000;
            public const uint FILE_FLAG_RANDOM_ACCESS = 0x10000000;
            public const uint FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000;
            public const uint FILE_FLAG_DELETE_ON_CLOSE = 0x04000000;
            public const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
            public const uint FILE_FLAG_POSIX_SEMANTICS = 0x01000000;
            public const uint FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;
            public const uint FILE_FLAG_OPEN_NO_RECALL = 0x00100000;
            public const uint FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000;
        }
        private IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        public struct HIDD_ATTRIBUTES
        {
            public int Size;
            public ushort VendorID;
            public ushort ProductID;
            public ushort VersionNumber;
        }

        public struct HIDP_CAPS
        {
            public ushort Usage;
            public ushort UsagePage;
            public ushort InputReportByteLength;
            public ushort OutputReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public ushort[] Reserved;
            public ushort NumberLinkCollectionNodes;
            public ushort NumberInputButtonCaps;
            public ushort NumberInputValueCaps;
            public ushort NumberInputDataIndices;
            public ushort NumberOutputButtonCaps;
            public ushort NumberOutputValueCaps;
            public ushort NumberOutputDataIndices;
            public ushort NumberFeatureButtonCaps;
            public ushort NumberFeatureValueCaps;
            public ushort NumberFeatureDataIndices;
        }
        int outputReportLength;//输出报告长度,包刮一个字节的报告ID
        int inputReportLength;//输入报告长度,包刮一个字节的报告ID   
        private FileStream hidDevice = null;
        public int InputReportLength { get { return inputReportLength; } }
        protected virtual void OnDeviceRemoved(EventArgs e)
        {
            if (DeviceRemoved != null) DeviceRemoved(this, e);
        }

        public delegate void DelegateStatusConnected(object sender, EventArgs e);
        public DelegateStatusConnected DeviceRemoved;

        public delegate void DelegateDataReceived(object sender, report e);
        //public event EventHandler<ConnectEventArg> StatusConnected;

        public DelegateDataReceived DataReceived;



        public void CloseDevice()
        {
            if (deviceOpened == true)
            {
                deviceOpened = false;
                hidDevice.Close();
            }
        }
        protected virtual void OnDataReceived(report e)
        {
            if (DataReceived != null) DataReceived(this, e);
        }
        private void ReadCompleted(IAsyncResult iResult)
        {
            byte[] readBuff = (byte[])(iResult.AsyncState);
            try
            {
                hidDevice.EndRead(iResult);//读取结束,如果读取错误就会产生一个异常
                byte[] reportData = new byte[readBuff.Length - 1];
                for (int i = 1; i < readBuff.Length; i++)
                    reportData[i - 1] = readBuff[i];
                report e = new report(readBuff[0], reportData);
                OnDataReceived(e); //发出数据到达消息
                if (!deviceOpened) return;
                BeginAsyncRead();//启动下一次读操作
            }
            catch //读写错误,设备已经被移除
            {
                //MyConsole.WriteLine("设备无法连接，请重新插入设备");
                EventArgs ex = new EventArgs();
                OnDeviceRemoved(ex);//发出设备移除消息
                CloseDevice();

            }
        }
        private void BeginAsyncRead()
        {
            byte[] inputBuff = new byte[InputReportLength];
            hidDevice.BeginRead(inputBuff, 0, InputReportLength, new AsyncCallback(ReadCompleted), inputBuff);
        }
        private IntPtr hHubDevice;

        public HID_RETURN OpenDevice(UInt16 vID, UInt16 pID, string serial)
        {
            if (deviceOpened == false)
            {
                //获取连接的HID列表
                List<string> deviceList = new List<string>();
                GetHidDeviceList(ref deviceList);
                if (deviceList.Count == 0)
                    return HID_RETURN.NO_DEVICE_CONECTED;
                for (int i = 0; i < deviceList.Count; i++)
                {
                    IntPtr device = CreateFile(deviceList[i],
                                                DESIREDACCESS.GENERIC_READ | DESIREDACCESS.GENERIC_WRITE,
                                                0,
                                                0,
                                                CREATIONDISPOSITION.OPEN_EXISTING,
                                                FLAGSANDATTRIBUTES.FILE_FLAG_OVERLAPPED,
                                                0);
                    if (device != INVALID_HANDLE_VALUE)
                    {
                        HIDD_ATTRIBUTES attributes;
                        IntPtr serialBuff = Marshal.AllocHGlobal(512);
                        HidD_GetAttributes(device, out attributes);
                        HidD_GetSerialNumberString(device, serialBuff, 512);
                        string deviceStr = Marshal.PtrToStringAuto(serialBuff);
                        Marshal.FreeHGlobal(serialBuff);
                        if (attributes.VendorID == vID && attributes.ProductID == pID && deviceStr.Contains(serial))
                        {
                            IntPtr preparseData;
                            HIDP_CAPS caps;
                            HidD_GetPreparsedData(device, out preparseData);
                            HidP_GetCaps(preparseData, out caps);
                            HidD_FreePreparsedData(preparseData);
                            outputReportLength = caps.OutputReportByteLength;
                            inputReportLength = caps.InputReportByteLength;

                            hidDevice = new FileStream(new SafeFileHandle(device, false), FileAccess.ReadWrite, inputReportLength, true);
                            deviceOpened = true;
                            BeginAsyncRead();

                            hHubDevice = device;
                            return HID_RETURN.SUCCESS;
                        }
                    }
                }
                return HID_RETURN.DEVICE_NOT_FIND;
            }
            else
                return HID_RETURN.DEVICE_OPENED;
        }
    }
}
