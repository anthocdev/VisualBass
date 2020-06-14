using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visual.cs
{
    public static class DataVars
    {
        public static List<string> FileList = new List<string>();

        /// <summary>
        /// Getting Filename from directory
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFileName(string file)
        {
            string[] temp = file.Split('\\');
            return temp[temp.Length - 1];
        }
    }
}
