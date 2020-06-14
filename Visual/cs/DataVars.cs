using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visual.cs
{
    public static class DataVars
    {

        public static MainWindow Core;
        //Core directory of the project
        public static string AppPath = AppDomain.CurrentDomain.BaseDirectory;

        public static List<string> FileList = new List<string>();

        /// <summary>
        /// Value of currently selected track
        /// </summary>
        public static int CurrentTrack;

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
