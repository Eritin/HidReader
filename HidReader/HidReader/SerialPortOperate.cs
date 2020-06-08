using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HidReader
{
    class SerialPortOperate
    {

        public static SerialPortHelper serial;
        private Dictionary<string, SerialPortOperate> _serialPortOperates = new Dictionary<string, SerialPortOperate>();
        
        
        public SerialPort _serialPort { get; set; }
        private string _portName;
        public SerialPortOperate(string PortName)
        {
            _portName = PortName;
        }
        // 数据读取成功后返回
        DataReceived _dataReceived { get; set; }
        private bool _comOpen = false;
        // 串口获取数据
        public byte[] ReceivedDataByte { get; set; }
        void DataReceivedBPHandler(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(100);
            ReceivedDataByte = new byte[_serialPort.BytesToRead];
            _serialPort.Read(ReceivedDataByte, 0, _serialPort.BytesToRead);
            if (ReceivedDataByte.Length > 6)
            {
                _dataReceived(ReceivedDataByte);
            }
        }
        private void SetComStausAfterClose()
        {
            //串口状态设置为关闭状态
            _comOpen = false;
        }
        public void Close()
        {
            try//尝试关闭串口
            {
                _serialPort.DiscardOutBuffer();//清发送缓存
                _serialPort.DiscardInBuffer();//清接收缓存
                _serialPort.Close();//关闭串口
                SetComStausAfterClose();//成功关闭串口或串口丢失后的设置
            }
            catch//如果在未关闭串口前，串口就已丢失，这时关闭串口会出现异常
            {
                //当前串口状态，
                //如果ComPort.IsOpen == false，说明串口已丢失，直接改状态；
                //如果ComPort.IsOpen == true，未知原因，但是串口的状态是未关闭
                if (_serialPort.IsOpen == false)
                {
                    SetComStausAfterClose();
                }
            }
        }
        void DataReceivedBSHandler(object sender, SerialDataReceivedEventArgs e)
        {
            ReceivedDataByte = new byte[_serialPort.BytesToRead];
            _serialPort.Read(ReceivedDataByte, 0, ReceivedDataByte.Length);
            Close();
            _dataReceived(ReceivedDataByte);
        }
        void DataReceivedBFHandler(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(2000);
            ReceivedDataByte = new byte[_serialPort.BytesToRead];
            _serialPort.Read(ReceivedDataByte, 0, ReceivedDataByte.Length);
            Close();
            _dataReceived(ReceivedDataByte);
        }
        public Tuple<bool, string> Open(DataReceived dataReceived = null, int BaudRate = 9600, Parity Parity = Parity.None, int DataBits = 8, StopBits StopBits = StopBits.One, bool DtrEnable = true, bool RtsEnable = true, int ReadTimeout = 500, int WriteTimeout = 500, int ReadBufferSize = 51200, int WriteBufferSize = 1024, int ReceivedBytesThreshold = 6, int Type = 0)
        {
            if (!SerialPort.GetPortNames().Contains(_portName))
            {
                return new Tuple<bool, string>(false, "找不到端口，请确定是否插入数据线或者端口设置错误");
            }
            if (dataReceived != null)
            {
                _dataReceived = dataReceived;
            }
            if (!_comOpen) //ComPortIsOpen == false当前串口为关闭状态，按钮事件为打开串口
            {
                try //尝试打开串口
                {
                    _serialPort = new SerialPort()
                    {
                        PortName = _portName,//串口名称
                        BaudRate = BaudRate,//串行波特率
                        Parity = Parity,//奇偶校验检查协议
                        DataBits = DataBits,//每个字节的标准数据位长度
                        StopBits = StopBits,//每个字节的标准停止位数
                        DtrEnable = DtrEnable,//串行通信中启用数据终端就绪（DTR）信号
                        RtsEnable = RtsEnable,//串行通信中启用请求发送（RTS）信号
                        ReadTimeout = ReadTimeout, //读取操作未完成时发生超时之前的毫秒数
                        WriteTimeout = WriteTimeout, //写入操作未完成时发生超时之前的毫秒数
                        ReadBufferSize = ReadBufferSize, //输入缓冲区的大小
                        WriteBufferSize = WriteBufferSize, //输入缓冲区的大小
                        ReceivedBytesThreshold = ReceivedBytesThreshold // 触发DataReceived事件需要字符送达端口的数量
                    };
                    switch (Type)
                    {
                        case 0:
                            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedBPHandler); //串口接收
                            break;
                        case 1:
                            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedBSHandler); //串口接收
                            break;
                        case 2:
                            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedBFHandler); //串口接收
                            break;
                        default:
                            break;
                    }
                    _serialPort.Open();
                    if (_serialPort.IsOpen)
                    {
                        _comOpen = true; //串口打开状态字改为true   
                    }
                }
                catch (Exception ex) //如果串口被其他占用，则无法打开
                {
                    _comOpen = false;
                    return new Tuple<bool, string>(false, "无法打开端口：" + ex.Message);
                }
            }
            return new Tuple<bool, string>(true, "成功");
        }
    }

}
