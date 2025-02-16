using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJackClasses.Model
{
    public static class FilesHelper
    {

        public static bool EnsureFolderExists(string folderPath)
        {
            try
            {
                string HolderPath = folderPath;

                if (Directory.Exists(HolderPath))
                {
                    return true;
                }
                else
                {
                    Directory.CreateDirectory(HolderPath);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                //AddOutput(e.Message);
                return false;
            }
        }
    }
}
