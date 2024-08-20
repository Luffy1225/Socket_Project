using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Luffy_Tool
{
    static public class Socket_Tool
    {


        /// <summary>
        /// 利用 正則表達式 來判斷 來源字串 是否是 IPv4 格式
        /// </summary>
        /// <param name="in_ip">來源ip字串</param>
        /// <returns></returns>
        static public bool Is_IPv4(string in_ip)
        {
            if (string.IsNullOrEmpty(in_ip))
                return false;


            // IPv4 正則表達式 
            string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(in_ip);
        }

        /// <summary>
        /// 利用 正則表達式 來判斷 來源字串 是否是 IPv6 格式
        /// </summary>
        /// <param name="in_ip">來源ip字串</param>
        /// <returns></returns>
        static public bool Is_IPv6(string in_ip)
        {
            if (string.IsNullOrEmpty(in_ip))
                return false;

            // IPv6 正則表達式
            string pattern = @"^((?:[0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}|(?=(?:[^:]+:){0,6}[^:]+$)([0-9a-fA-F]{1,4}::?|::)([0-9a-fA-F]{1,4}:){0,6}[0-9a-fA-F]{1,4}|(?:[0-9a-fA-F]{1,4}:){0,7}[0-9a-fA-F]{1,4})$";
            Regex regex = new Regex(pattern, RegexOptions.Compiled);

            return regex.IsMatch(in_ip);
        }


        static public bool Is_Port(string str_por)
        {
            int port = -1;
            if (int.TryParse(str_por, out port))
            {
                return Is_Port(port);
            }
            return false;
        }

        static public bool Is_Port(int int_port)
        {
            return 0 <= int_port && int_port <= 65535;
        }


    }


}
