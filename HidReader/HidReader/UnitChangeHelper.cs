using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HidReader
{
    /// <summary>
    /// 单位转换，默认从mmol/L转到mg/dL
    /// </summary>
    class UnitChangeHelper
    {
        public static SerialPortHelper serial;
        /// <summary>
        /// 血糖转换
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double GluChange(double data)
        {
            return data * 18;
        }
        /// <summary>
        /// 血糖逆转换
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double GluChangeBack(double data)
        {
            return Math.Round(data / 18, 2);
        }
        /// <summary>
        /// 总胆固醇、高密度脂蛋白、低密度脂蛋白转换
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double TCChange(double data)
        {
            return data * 38.66;
        }
        /// <summary>
        /// 总胆固醇、高密度脂蛋白、低密度脂蛋白逆转换
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double TCChangeBack(double data)
        {
            return Math.Round(data / 38.66, 2);
        }
        /// <summary>
        /// 甘油三酯转换
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double TGChange(double data)
        {
            return data * 88.545;
        }
        /// <summary>
        /// 甘油三酯逆转换
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double TGChangeBack(double data)
        {
            return Math.Round(data / 88.545, 2);
        }
        /// <summary>
        /// 尿酸转换
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double UAChange(double data)
        {
            return data * 16.81;
        }
        /// <summary>
        /// 尿酸逆转换
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double UAChangeBack(double data)
        {
            return Math.Round(data / 16.81, 2);
        }
        /// <summary>
        /// 字节转int
        /// </summary>
        /// <param name="byt"></param>
        /// <returns></returns>
        public static int ByteToInt(byte byt)
        {
            int convertInt = 0;
            if (byt >= 48 && byt <= 57)
            {
                convertInt = byt - 48;
            }
            else if (byt >= 65 && byt <= 90)//十六进制的转成十进制（大写字符）
            {
                convertInt = byt - 65 + 10;

            }
            else if (byt >= 97 && byt <= 122)//十六进制的转成十进制（小写字符）
            {
                convertInt = byt - 97 + 10;

            }
            return convertInt;
        }
    }
}
