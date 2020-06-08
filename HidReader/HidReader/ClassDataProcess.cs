using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HidReader
{
    class ClassDataProcess
    {
        //第一步需要初始化，传入vid、pid，并开启自动连接
        public void Initial()
        {
            HIDInterface hid = new HIDInterface();
            hid.StatusConnected = StatusConnected;
            hid.DataReceived = DataReceived;
            HIDInterface.HidDevice hidDevice = new HIDInterface.HidDevice();
            hidDevice.vID = 0x1A86;
            hidDevice.pID = 0xE010;
            hidDevice.serial = "";
            hid.AutoConnect(hidDevice);

        }
        struct connectStatusStruct
        {
            public bool preStatus;
            public bool curStatus;
        }
        connectStatusStruct connectStatus = new connectStatusStruct();
       

        //推送接收数据信息
        public delegate void PushReceiveDataDele(byte[] datas);
        public PushReceiveDataDele pushReceiveData;


        //推送连接状态信息
        public delegate void isConnectedDelegate(bool isConnected);
        public isConnectedDelegate isConnectedFunc;
        //状态改变接收
        public void StatusConnected(object sender, bool isConnect)
        {
            connectStatus.curStatus = isConnect;
            if (connectStatus.curStatus == connectStatus.preStatus)  //connect
                return;
            connectStatus.preStatus = connectStatus.curStatus;

            if (connectStatus.curStatus)
            {
                isConnectedFunc(true);
                //ReportMessage(MessagesType.Message, "连接成功");
            }
            else //disconnect
            {
                isConnectedFunc(false);
                //ReportMessage(MessagesType.Error, "无法连接");
            }
        }
        //接受到数据
        public void DataReceived(object sender, byte[] e)
        {
            if (pushReceiveData != null)
                pushReceiveData(e);
        }
        


    }
}
