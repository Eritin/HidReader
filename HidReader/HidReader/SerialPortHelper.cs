using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HidReader
{
    public delegate void DataReceived(object obj);
    class SerialPortHelper
    {

        private Dictionary<string, SerialPortOperate> _serialPortOperates = new Dictionary<string, SerialPortOperate>();

        public SerialPortHelper(Dictionary<string, string> ports)
        {
            InitSerialPort(ports);
        }

        public void InitSerialPort(Dictionary<string, string> ports)
        {
            foreach (var item in ports)
            {
                _serialPortOperates.Add(item.Key, new SerialPortOperate(item.Value));
            }
        }
        #region ==血脂==
        // <summary>
        /// 获取数据
        /// </summary>
        /// <param name="dataReceived"></param>
        /// <returns></returns>
        public Tuple<bool, string> GetBFResult(DataReceived dataReceived)
        {
            return _serialPortOperates["BFMachine"].Open(dataReceived, BaudRate: 9600, ReceivedBytesThreshold: 1, Type: 2);
        }
        #endregion
    }
}
