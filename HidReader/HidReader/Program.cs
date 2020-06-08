using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HidReader
{
    class Program
    {
        static void Main(string[] args)
        {
            ClassDataProcess CDProcess = new ClassDataProcess();
            CDProcess.Initial();

            Program Received = new Program();
            CDProcess.isConnectedFunc = Received.ConnectedStatus;
            CDProcess.pushReceiveData = Received.DataReceived;

            Console.WriteLine("finish");
            
            Console.ReadLine();
        }
        void ConnectedStatus(bool isConnected)
        {

                if (isConnected)
                {
                Console.WriteLine("已连接");
                
                }
                else
                {
                Console.WriteLine("未连接");
            }
            
        }
        
        
        public byte[] Data = new byte[] {};
       
        public void DataReceived(byte[] data)
        {
            
            string srecData = BitConverter.ToString(data);
            srecData = srecData.Replace('-', ' ');
            Data = Data.Concat(data).ToArray();
            //string temp = Data.ToString() + srecData;
            //Data = System.Text.Encoding.Default.GetBytes(temp);
            int i = Data.Length;
            if (i>800)
            {
                Transfer transfer = new Transfer();
                transfer.OperateBFMachine(Data);
            }
            Console.WriteLine(srecData);
        }

    }
}
