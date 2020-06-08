using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HidReader
{
    class Transfer
    {
        public void OperateBFMachine(object result)
        {
            try
            {
                byte[] content = (byte[])result;
                string bfStr = Encoding.ASCII.GetString(content).ToUpper();
                int count = bfStr.IndexOf("P1") >= 0 ? bfStr.IndexOf("P1") : bfStr.IndexOf("P2") >= 0 ? bfStr.IndexOf("P2") : bfStr.IndexOf("P3");
                if (count > 0)
                    bfStr = bfStr.Substring(3, count - 4);
                var bfArray = bfStr.Split('\n');
                foreach (var item in bfArray)
                {
                    var itemStr = item.Substring(item.IndexOf('"') + 1, item.LastIndexOf('"') - item.IndexOf('"') - 1).Replace(" ", "");
                    if (itemStr.Contains("TC/HDL"))
                    {
                        Console.WriteLine(itemStr.Substring(7,4));
                        //TC_HDL = double.Parse(itemStr.Substring(itemStr.IndexOf(':') + 1, 3));
                    }
                    else if (itemStr.Contains("HDLCHOL"))
                    {
                        Console.WriteLine(itemStr.Substring(8, 4));
                        //HDL = double.Parse(itemStr.Substring(itemStr.IndexOf(':') + 1, 4));
                    }
                    else if (itemStr.Contains("TRIG"))
                    {
                        Console.WriteLine(itemStr.Substring(5, 4));
                        //TG = double.Parse(itemStr.Substring(itemStr.IndexOf(':') + 1, 4));
                    }
                    else if (itemStr.Contains("CALCLDL"))
                    {
                        Console.WriteLine(itemStr.Substring(8, 4));
                        //Console.WriteLine(double.Parse(itemStr.Substring( 8, 11)));
                    }
                    else if (itemStr.Contains("CHOL"))
                    {
                        Console.WriteLine(itemStr.Substring(5, 4));
                        //Console.WriteLine(double.Parse(itemStr.Substring(5, 8)));
                    }
                }
                MeasureTime = DateTime.Now;
               if (_unit != "mmol/L")
                {
                    TC = UnitChangeHelper.TCChange(TC);
                    HDL = UnitChangeHelper.TCChange(HDL);
                    LDL = UnitChangeHelper.TCChange(LDL);
                    TG = UnitChangeHelper.TGChange(TG);
                }
                IsHandWrite = false;
            }
            catch (Exception e)
            {
                Console.WriteLine("端口获取数据异常");
            }
           // OpenCom();
        }
        
        
       
        public void OpenCom()
        {
            int count = 0;
            Tuple<bool, string> isSuccess = new Tuple<bool, string>(false, "first");
            while (!isSuccess.Item1)
            {
                isSuccess = SerialPortOperate.serial.GetBFResult(new DataReceived(OperateBFMachine));
                if (count < 3)
                {
                    count++;
                }
                else if (count >= 3)
                {
                    Console.WriteLine("端口打开失败");
                    break;
                }
            }
        }
        private double _tc_hdl = 0;
        private bool _isHandWrite = true;
        private double _hdl = 0;
        private double _tg = 0;
        private double _ldl = 0;
        private double _tc = 0;
        private DateTime _measureTime = DateTime.Now;

 
        public bool IsHandWrite
        {
            get { return _isHandWrite; }
            set { _isHandWrite = value; }
        }
        public double TC_HDL
        {
            get { return _tc_hdl; }
            set { _tc_hdl = value; }
        }
        public double HDL
        {
            get { return _hdl; }
            set { _hdl = value; }
        }
        public double TG
        {
            get { return _tg; }
            set { _tg = value; }
        }
        public double LDL
        {
            get { return _ldl; }
            set { _ldl = value; }
        }
        public double TC
        {
            get { return _tc; }
            set { _tc = value; }
        }
        public DateTime MeasureTime
        {
            get { return _measureTime; }
            set { _measureTime = value; }
        }
        private string _unit;
    }
}
